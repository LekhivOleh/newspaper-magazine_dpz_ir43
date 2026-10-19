using CURSOVA.Application.Abstractions.Messaging;

namespace CURSOVA.Infrastructure.Messaging;

/// <summary>No mail server in this project: outgoing emails are written to the application log.</summary>
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        logger.LogInformation("Email to {To}: {Subject}\n{Body}", message.To, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}
