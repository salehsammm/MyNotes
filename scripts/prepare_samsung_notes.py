"""Prepare an additive Samsung PDF import; never modifies the source folder.

Requires pdfplumber and python-bidi. Keep output outside the repository/webroot:
it contains private text and copies of the original PDFs.
"""
import argparse
from collections import Counter
from datetime import datetime
import hashlib
import json
from pathlib import Path
import re
import shutil
import statistics
import unicodedata

import pdfplumber
from bidi.algorithm import get_display


def digest(data):
    return hashlib.sha256(data).hexdigest()


def letters_and_numbers(text):
    return Counter(c for c in unicodedata.normalize("NFKC", text)
                   if unicodedata.category(c)[0] in "LNM")


def logical_line(text, previous_direction):
    # Invert visual ordering BEFORE expanding presentation-form ligatures.
    # Expanding first reverses lam/alef within a ligature and corrupts words.
    direction = next(("R" if unicodedata.bidirectional(c) in ("R", "AL") else "L"
                      for c in text if unicodedata.bidirectional(c) in ("R", "AL", "L")),
                     previous_direction)
    return unicodedata.normalize("NFKC", get_display(text, base_dir=direction)), direction


def prepare(source, output):
    source, output = source.resolve(), output.resolve()
    if source == output or source in output.parents:
        raise ValueError("Output must be outside the source folder")
    output.mkdir(parents=True, exist_ok=False)
    originals = output / "originals"
    originals.mkdir()
    notes = []
    for pdf_path in sorted(source.glob("*.pdf")):
        original = pdf_path.read_bytes()
        match = re.fullmatch(r"(.+)_(\d{6})_(\d{6})", pdf_path.stem)
        if not match:
            raise ValueError(f"Unrecognized filename date: {pdf_path.name}")
        timestamp = datetime.strptime(match[2] + match[3], "%y%m%d%H%M%S")
        title = match[1].strip()
        if not title or len(title) > 200:
            raise ValueError("Title exceeds the application's limits")
        shutil.copy2(pdf_path, originals / pdf_path.name)
        if digest((originals / pdf_path.name).read_bytes()) != digest(original):
            raise ValueError("PDF backup hash mismatch")
        pages = []
        with pdfplumber.open(pdf_path) as pdf:
            for number, page in enumerate(pdf.pages, 1):
                original_glyphs = len(page.chars)
                # Samsung simulates bold by painting the same glyph twice at the
                # same position. Remove only coordinate-overlapping duplicates.
                page = page.dedupe_chars(tolerance=1)
                lines = page.extract_text_lines(return_chars=False)
                gaps = [b["top"] - a["top"] for a, b in zip(lines, lines[1:])]
                normal_gap = statistics.median(gaps) if gaps else 0
                restored, direction = [], "L"
                for i, line in enumerate(lines):
                    if i and normal_gap and gaps[i-1] > normal_gap * 1.45:
                        restored.append("")
                    text, direction = logical_line(line["text"], direction)
                    restored.append(text)
                raw = "\n".join(line["text"] for line in lines)
                text = "\n".join(restored)
                if letters_and_numbers(raw) != letters_and_numbers(text):
                    raise ValueError(f"Letter/number preservation failed on page {number}")
                # Check against a separate extraction path, including every PDF glyph.
                if letters_and_numbers(raw) != letters_and_numbers(page.extract_text() or ""):
                    raise ValueError(f"Extraction disagreement on page {number}")
                pages.append({"number": number, "text": text, "rawVisualText": raw,
                              "removedOverprintGlyphs": original_glyphs - len(page.chars)})
        details = "\n\n".join(page["text"] for page in pages)
        if not details.strip():
            raise ValueError("Empty extraction; do not silently import a blank note")
        notes.append({"fileName": pdf_path.name, "sourceHash": digest(original),
                      "title": title, "noteDate": timestamp.date().isoformat(),
                      "filenameTimestamp": timestamp.isoformat(),
                      "kind": 2 if title.casefold() == "my feelings" else 1,
                      "details": details, "textHash": digest(details.encode("utf-8")),
                      "pages": pages})
        print(f"Prepared {len(notes):02}: {len(pages)} pages; characters verified", flush=True)
    if not notes:
        raise ValueError("No PDF files found")
    if len({n["sourceHash"] for n in notes}) != len(notes):
        raise ValueError("Duplicate PDFs detected; review before import")
    manifest = {"version": 1, "sourceFolder": str(source), "notes": notes}
    (output / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Ready: {len(notes)} notes, {sum(len(n['pages']) for n in notes)} pages")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    prepare(args.source, args.output)
