using CURSOVA.Domain.Subscriptions;
using CURSOVA.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.PlanKey).HasMaxLength(20);
        builder.Ignore(s => s.State);
        builder.Ignore(s => s.Plan);

        builder.HasIndex(s => s.UserId).IsUnique();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Payments).WithOne().HasForeignKey(p => p.SubscriptionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Payments).HasField("_payments").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.ToTable("SubscriptionPayments");
        builder.Property(p => p.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.PlanKey).HasMaxLength(20);
    }
}
