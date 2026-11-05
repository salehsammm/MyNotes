using System.Globalization;
using System.Text.RegularExpressions;

namespace MovieReviews.Services;

public static class ReviewTextCleanup
{
    // Clean adjacent output artifacts only. Preserve line breaks, numbers, ratings
    // (fav++, --) and intentional ellipses; never rewrite the stored review fields.
    public static string Clean(string text)
    {
        text = Regex.Replace(text, @"\b([\p{L}\p{M}][\p{L}\p{M}\p{N}]*)(?:[ \t]+\1\b)+", "$1",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var icons = new HashSet<string>();
        var elements = StringInfo.GetTextElementEnumerator(text);
        while (elements.MoveNext())
        {
            var element = elements.GetTextElement();
            if (CharUnicodeInfo.GetUnicodeCategory(element, 0) == UnicodeCategory.OtherSymbol) icons.Add(element);
        }
        foreach (var icon in icons)
        {
            var escaped = Regex.Escape(icon);
            text = Regex.Replace(text, $@"{escaped}(?:[ \t]*{escaped})+", _ => icon);
        }
        text = Regex.Replace(text, @"([,،؛;:!؟?])(?:[ \t]*\1)+", "$1");
        text = Regex.Replace(text, @":[ \t]*[,،](?:[ \t]*[,،])*", ":");
        return text;
    }
}
