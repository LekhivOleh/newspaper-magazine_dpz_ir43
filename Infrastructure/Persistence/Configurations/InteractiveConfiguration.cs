using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Polls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class InteractiveElementConfiguration : IEntityTypeConfiguration<InteractiveElement>
{
    public void Configure(EntityTypeBuilder<InteractiveElement> builder)
    {
        builder.ToTable("InteractiveElements");
        builder.HasDiscriminator<string>("Kind").HasValue<Poll>("poll");
        builder.Property<string>("Kind").HasMaxLength(20);

        builder.Property(e => e.Title).HasMaxLength(InteractiveElement.TitleMaxLength).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.HasOne<Article>().WithMany().HasForeignKey(e => e.ArticleId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(e => new { e.ArticleId, e.IsActive });
    }
}

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.Property(p => p.Question).HasMaxLength(Poll.QuestionMaxLength);
        builder.HasMany(p => p.Options).WithOne().HasForeignKey(o => o.PollId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Options).HasField("_options").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(p => p.TotalVotes);
    }
}

public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        builder.ToTable("PollOptions");
        builder.Property(o => o.Text).HasMaxLength(200).IsRequired();
    }
}

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.Property(s => s.ParticipantKey).HasMaxLength(64).IsRequired();
        builder.Property(s => s.ResultKey).HasMaxLength(100);
        builder.HasOne<InteractiveElement>().WithMany().HasForeignKey(s => s.ElementId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(s => new { s.ElementId, s.ParticipantKey });
    }
}
