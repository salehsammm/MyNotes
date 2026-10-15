import unittest
from prepare_samsung_notes import letters_and_numbers, logical_line


class PersianExtractionTests(unittest.TestCase):
    def test_ligature_expansion_happens_after_reordering(self):
        self.assertEqual(logical_line("ﻡﻼﺳ", "L")[0], "سلام")

    def test_mixed_language_keeps_english_word_order(self):
        self.assertEqual(logical_line("Hello = ﻡﻼﺳ", "L")[0], "Hello = سلام")

    def test_persian_numbers_are_not_reversed(self):
        self.assertEqual(logical_line("ﻡﻼﺳ - ۱۲", "L")[0], "۱۲ - سلام")

    def test_plain_english_stays_in_order(self):
        self.assertEqual(logical_line("A simple note, 13.1.1405", "R")[0], "A simple note, 13.1.1405")

    def test_letter_number_and_diacritic_inventory_is_preserved(self):
        text = "ﻡﻼﺳ - ۱۲ Hello"
        self.assertEqual(letters_and_numbers(text), letters_and_numbers(logical_line(text, "L")[0]))


if __name__ == "__main__":
    unittest.main()
