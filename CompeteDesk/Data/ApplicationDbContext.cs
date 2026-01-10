using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CompeteDesk.Models;

namespace CompeteDesk.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Strategy> Strategies => Set<Strategy>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Workspace>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(120);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.HasIndex(x => new { x.OwnerId, x.Name });
        });

        builder.Entity<Strategy>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(160);
            b.Property(x => x.SourceBook).HasMaxLength(120);
            b.Property(x => x.CorePrinciple).HasMaxLength(300);
            b.Property(x => x.Summary).HasMaxLength(2000);
            b.Property(x => x.Category).HasMaxLength(80);
            b.Property(x => x.Status).IsRequired().HasMaxLength(24);

            b.HasIndex(x => new { x.OwnerId, x.Status });
            b.HasIndex(x => new { x.WorkspaceId, x.OwnerId });
        });
    }
}
