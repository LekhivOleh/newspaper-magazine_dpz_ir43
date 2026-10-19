namespace CURSOVA.Application.Notifications;

/// <summary>Something happened in the magazine that readers may want to hear about.</summary>
public abstract record MagazineEvent;

public record IssuePublished(int IssueId, string Title, string Slug, string Description, string? CoverImageUrl) : MagazineEvent;

public record ExclusiveContentPublished(int ArticleId, string Title, string Slug, string Lead, string? CoverImageUrl, bool IsPremiumArticle)
    : MagazineEvent;

public record PartnerPromotionLaunched(int AdId, int? PartnerId, string? PartnerName, string Headline, string? Body, string? ImageUrl)
    : MagazineEvent;

/// <summary>Observer pattern: the observer interface. Each observer reacts to events in its own way.</summary>
public interface IMagazineEventObserver
{
    Task OnEventAsync(MagazineEvent e, CancellationToken ct = default);
}

/// <summary>Observer pattern: the subject that services report events to.</summary>
public interface IMagazineEventPublisher
{
    void Attach(IMagazineEventObserver observer);
    void Detach(IMagazineEventObserver observer);
    Task PublishAsync(MagazineEvent e, CancellationToken ct = default);
}

public class MagazineEventPublisher(IEnumerable<IMagazineEventObserver> observers, ILogger<MagazineEventPublisher> logger)
    : IMagazineEventPublisher
{
    private readonly List<IMagazineEventObserver> _observers = observers.ToList();

    public void Attach(IMagazineEventObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Detach(IMagazineEventObserver observer) => _observers.Remove(observer);

    // The action that raised the event has already succeeded, so a failing observer is logged instead of undoing it.
    public async Task PublishAsync(MagazineEvent e, CancellationToken ct = default)
    {
        foreach (var observer in _observers.ToList())
        {
            try
            {
                await observer.OnEventAsync(e, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "{Observer} failed to handle {Event}", observer.GetType().Name, e.GetType().Name);
            }
        }
    }
}
