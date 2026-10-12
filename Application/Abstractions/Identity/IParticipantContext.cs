namespace CURSOVA.Application.Abstractions.Identity;

/// <summary>Identifies whoever takes part in polls and quizzes — a member or an anonymous visitor.</summary>
public interface IParticipantContext
{
    string Key { get; }
    int? UserId { get; }
}
