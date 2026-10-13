using System.Text.RegularExpressions;
using CURSOVA.Domain.Comments;

namespace CURSOVA.Application.Comments.Moderation;

public class LengthHandler : ModerationHandler
{
    public const int MinLength = 3;

    protected override ModerationResult Process(CommentDraft draft)
    {
        var length = draft.Text.Trim().Length;
        if (length < MinLength)
            return Reject(draft, $"Коментар занадто короткий (мінімум {MinLength} символи).");
        if (length > Comment.BodyMaxLength)
            return Reject(draft, $"Коментар занадто довгий (максимум {Comment.BodyMaxLength} символів).");

        return PassOn(draft with { Text = draft.Text.Trim() });
    }
}

public class RateLimitHandler : ModerationHandler
{
    public const int MaxPerWindow = 5;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    protected override ModerationResult Process(CommentDraft draft) =>
        draft.RecentCommentCount >= MaxPerWindow
            ? Reject(draft, "Забагато коментарів поспіль. Спробуйте за кілька хвилин.")
            : PassOn(draft);
}

public class DuplicateHandler : ModerationHandler
{
    protected override ModerationResult Process(CommentDraft draft)
    {
        var text = Normalize(draft.Text);
        return draft.PreviousTexts.Any(previous => string.Equals(text, Normalize(previous), StringComparison.OrdinalIgnoreCase))
            ? Reject(draft, "Ви вже надсилали такий самий коментар.")
            : PassOn(draft);
    }

    private static string Normalize(string? text) => Regex.Replace(text ?? "", @"\s+", " ").Trim();
}

/// <summary>Doesn't stop the chain — masks the words and lets the next handler continue.</summary>
public partial class ProfanityHandler : ModerationHandler
{
    private static readonly string[] Stems = ["дурн", "ідіот", "придур", "тупий", "тупа", "лох", "stupid", "idiot", "moron"];

    protected override ModerationResult Process(CommentDraft draft)
    {
        var masked = WordRegex().Replace(draft.Text, m =>
            Stems.Any(stem => m.Value.StartsWith(stem, StringComparison.OrdinalIgnoreCase))
                ? m.Value[0] + new string('*', m.Value.Length - 1)
                : m.Value);

        return PassOn(draft with { Text = masked });
    }

    [GeneratedRegex(@"\p{L}+")]
    private static partial Regex WordRegex();
}

public partial class LinkHandler : ModerationHandler
{
    public const int MaxLinks = 2;

    protected override ModerationResult Process(CommentDraft draft)
    {
        var links = LinkRegex().Matches(draft.Text).Count;
        if (links > MaxLinks)
            return Reject(draft, "Забагато посилань — схоже на спам.");
        if (links > 0)
            return new ModerationResult(ModerationDecision.Pending, draft.Text, "Коментар із посиланням — перевірить модератор.");

        return PassOn(draft);
    }

    [GeneratedRegex(@"(https?://|www\.)\S+", RegexOptions.IgnoreCase)]
    private static partial Regex LinkRegex();
}
