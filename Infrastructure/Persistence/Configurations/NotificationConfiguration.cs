using CURSOVA.Domain.Notifications;
using CURSOVA.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasDiscriminator<string>("Type")
            .HasValue<NewIssueNotification>("new-issue")
            .HasValue<ExclusiveContentNotification>("exclusive")
            .HasValue<PartnerPromoNotification>("partner-promo");
        builder.Property("Type").HasMaxLength(20);

        builder.Ignore(n => n.Kind);
        builder.Ignore(n => n.IsRead);
        builder.Property(n => n.Title).HasMaxLength(Notification.TitleMaxLength);
        builder.Property(n => n.Message).HasMaxLength(Notification.MessageMaxLength);
        builder.Property(n => n.Link).HasMaxLength(500);
        builder.Property(n => n.ImageUrl).HasMaxLength(500);

        builder.HasIndex(n => new { n.UserId, n.CreatedAt });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
