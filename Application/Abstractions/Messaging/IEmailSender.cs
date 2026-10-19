namespace CURSOVA.Application.Abstractions.Messaging;

public record EmailMessage(string To, string Subject, string Body);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
