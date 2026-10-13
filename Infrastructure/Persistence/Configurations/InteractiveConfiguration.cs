using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Interactive.Polls;
using CURSOVA.Domain.Interactive.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CURSOVA.Infrastructure.Persistence.Configurations;

public class InteractiveElementConfiguration : IEntityTypeConfiguration<InteractiveElement>
{
    public void Configure(EntityTypeBuilder<InteractiveElement> builder)
    {
        builder.ToTable("InteractiveElements");
        builder.HasDiscriminator<string>("Kind").HasValue<Poll>("poll").HasValue<Quiz>("quiz");
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

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.Property(q => q.ScoringMode).HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.CoverImageUrl).HasMaxLength(500);

        builder.HasMany(q => q.Questions).WithOne().HasForeignKey(q => q.QuizId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(q => q.Questions).HasField("_questions").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(q => q.Outcomes).WithOne().HasForeignKey(o => o.QuizId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(q => q.Outcomes).HasField("_outcomes").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions");
        builder.Property(q => q.Text).HasMaxLength(300).IsRequired();
        builder.HasMany(q => q.Options).WithOne().HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(q => q.Options).HasField("_options").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
{
    public void Configure(EntityTypeBuilder<QuizOption> builder)
    {
        builder.ToTable("QuizOptions");
        builder.Property(o => o.Text).HasMaxLength(200).IsRequired();
        builder.Property(o => o.OutcomeKey).HasMaxLength(50);
    }
}

public class QuizOutcomeConfiguration : IEntityTypeConfiguration<QuizOutcome>
{
    public void Configure(EntityTypeBuilder<QuizOutcome> builder)
    {
        builder.ToTable("QuizOutcomes");
        builder.Property(o => o.Key).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Title).HasMaxLength(150).IsRequired();
        builder.Property(o => o.Description).HasMaxLength(1000);
    }
}
