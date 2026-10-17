using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Articles.Blocks;
using CURSOVA.Domain.Authors;
using CURSOVA.Domain.Categories;
using CURSOVA.Domain.Comments;
using CURSOVA.Domain.Interactive;
using CURSOVA.Domain.Issues;
using CURSOVA.Domain.Sharing;
using CURSOVA.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence;

public class MagazineDbContext(DbContextOptions<MagazineDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ContentBlock> ContentBlocks => Set<ContentBlock>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<InteractiveElement> InteractiveElements => Set<InteractiveElement>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ShareEvent> ShareEvents => Set<ShareEvent>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MagazineDbContext).Assembly);

        modelBuilder.Entity<ApplicationUser>().Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
    }
}
