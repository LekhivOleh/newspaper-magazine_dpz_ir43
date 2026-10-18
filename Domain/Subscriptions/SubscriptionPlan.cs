namespace CURSOVA.Domain.Subscriptions;

public record SubscriptionPlan(string Key, string Title, int PriceUah, int Days)
{
    public static readonly SubscriptionPlan Monthly = new("monthly", "Premium на місяць", 99, 30);
    public static readonly SubscriptionPlan Yearly = new("yearly", "Premium на рік", 990, 365);

    public static readonly IReadOnlyList<SubscriptionPlan> All = [Monthly, Yearly];

    public static SubscriptionPlan? Find(string? key) => All.FirstOrDefault(p => p.Key == key);
}

public enum SubscriptionStatus
{
    Free,
    Active,
    Cancelled,
    Expired
}

public enum PaymentKind
{
    Purchase,
    Renewal,
    Gift
}
