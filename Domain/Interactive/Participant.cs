namespace CURSOVA.Domain.Interactive;

/// <param name="Key">Stable identity: "user:{id}" for members, "anon:{guid}" for guests.</param>
/// <param name="HasParticipated">Whether this participant already submitted to the element.</param>
public record Participant(string Key, int? UserId, bool HasParticipated);

public record SubmissionOutcome(string? ResultKey = null, int? Score = null, int? MaxScore = null)
{
    public static readonly SubmissionOutcome None = new();
}
