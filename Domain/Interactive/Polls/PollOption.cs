using CURSOVA.Domain.Common;

namespace CURSOVA.Domain.Interactive.Polls;

public class PollOption : Entity
{
    public int PollId { get; private set; }
    public string Text { get; private set; } = null!;
    public int Position { get; private set; }
    public int VoteCount { get; private set; }

    private PollOption() { }

    internal PollOption(string text, int position)
    {
        Text = Guard.NotEmpty(text, "Варіант відповіді", 200);
        Position = position;
    }

    internal void RegisterVote() => VoteCount++;
}
