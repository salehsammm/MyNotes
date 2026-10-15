# Samsung Notes PDF import

The local import tools append one personal note per PDF, under a dedicated Samsung Notes group. They do not update or delete existing notes, groups, research, journal titles, or episodes. All original text stays in Details; conclusions and summaries are not invented. The source named My feelings is marked as a diary, without guessing dates or splitting its passages.

The note date comes from the timestamp in the export filename. This is explicitly recorded as an export date, not an original diary date. Dates embedded in the text stay unchanged. PDF highlighting, drawings, and layout remain in archived originals; the editor contains searchable plain text.

Keep the archive outside the repository and all webroots. It contains private source PDFs, extracted text, a pre-import database snapshot, a verified SQL backup location, and per-file progress and verification records. Do not commit any of these generated files.

1. With Python packages `pdfplumber` and `python-bidi` installed, run `python scripts/prepare_samsung_notes.py <source-folder> <new-private-archive-folder>`. The script copies every PDF, verifies hashes, restores visual Persian ordering before normalizing presentation forms, and checks letters/numbers on every page against independent extraction. Source files are never changed.
2. Review representative extracted Persian, mixed-language, numeric, and English passages against the PDFs before applying. PDF text extraction can lose styling and cannot interpret arbitrary handwriting.
3. Run `pwsh -File scripts/import_samsung_notes.ps1 -Manifest <archive>/manifest.json -Deployment <existing-IIS-folder> -Apply`. This uses the deployed journal connection configuration, creates a COPY_ONLY database backup and verifies it, snapshots existing rows, then inserts and verifies one PDF per transaction. It stops on any mismatch. It never automatically rolls back previously committed notes by deleting them.
4. Rerun without `-Apply` to verify every stored note and every pre-existing row. SHA-256 source markers in research metadata prevent duplicate imports on retries. A changed existing import causes a verification error and is never overwritten. Import into an unused Samsung Notes group name; conflicting pre-existing groups are not modified.
5. Verify the notes route and search in the browser. The data is immediately available to the existing IIS app; no schema changes are required.

Samsung's simulated bold can paint each glyph twice. Preparation deduplicates glyphs only when text, font, size, and coordinates overlap within one PDF point. It records removed overprint counts per page; repeated sentences or overlapping page content are retained. The archived PDF is always byte-identical to its source.

For an extraction correction only, retain the previous manifest separately and pass `-Apply -PreviousManifest <previous-manifest>` with the corrected manifest at the original archive location. This creates and verifies another database backup, requires exactly the same PDF hashes, and updates only imported Details and provenance after checking that the entire stored note still matches the previous manifest. It refuses to overwrite user edits. Ordinary import and verification never update notes. All pre-import rows are compared against the original baseline after a repair too.

The importer compiles a temporary console outside the web project. It exposes no import web endpoint and logs counts/IDs rather than private note bodies. SQL Server must permit a database backup to its default backup directory. Retain the original export folder and the private archive after import.
