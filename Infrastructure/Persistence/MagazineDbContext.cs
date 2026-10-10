using CURSOVA.Domain.Articles;
using CURSOVA.Domain.Articles.Blocks;
using CURSOVA.Domain.Authors;
using CURSOVA.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence;

public class MagazineDbContext(DbContextOptions<MagazineDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ContentBlock> ContentBlocks => Set<ContentBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MagazineDbContext).Assembly);
    }
}
