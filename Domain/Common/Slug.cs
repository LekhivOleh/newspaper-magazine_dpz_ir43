using System.Text;

namespace CURSOVA.Domain.Common;

public static class Slug
{
    private static readonly Dictionary<char, string> Transliteration = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "h", ['ґ'] = "g", ['д'] = "d", ['е'] = "e",
        ['є'] = "ie", ['ж'] = "zh", ['з'] = "z", ['и'] = "y", ['і'] = "i", ['ї'] = "i", ['й'] = "i",
        ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r",
        ['с'] = "s", ['т'] = "t", ['у'] = "u", ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch",
        ['ш'] = "sh", ['щ'] = "shch", ['ь'] = "", ['ю'] = "iu", ['я'] = "ia", ['\''] = "", ['’'] = ""
    };

    public static string From(string text)
    {
        var sb = new StringBuilder(text.Length);
        var pendingDash = false;

        foreach (var ch in text.ToLowerInvariant())
        {
            var part = Transliteration.TryGetValue(ch, out var latin) ? latin
                : char.IsAsciiLetterOrDigit(ch) ? ch.ToString()
                : null;

            if (part is null)
            {
                pendingDash = sb.Length > 0;
                continue;
            }

            if (part.Length == 0)
                continue;

            if (pendingDash)
            {
                sb.Append('-');
                pendingDash = false;
            }
            sb.Append(part);
        }

        if (sb.Length == 0)
            throw new DomainException("Не вдалося згенерувати slug із заданого тексту.");

        return sb.ToString();
    }
}
