using CURSOVA.Domain.Notifications;

namespace CURSOVA.Models;

public static class NotificationLabels
{
    public static string For(NotificationKind kind) => kind switch
    {
        NotificationKind.NewIssue => "Новий випуск",
        NotificationKind.ExclusiveContent => "Premium",
        NotificationKind.PartnerPromo => "Партнер",
        _ => "Сповіщення"
    };

    public static string CssClass(NotificationKind kind) => kind switch
    {
        NotificationKind.ExclusiveContent => "kind-premium",
        NotificationKind.PartnerPromo => "kind-partner",
        _ => "kind-issue"
    };
}
