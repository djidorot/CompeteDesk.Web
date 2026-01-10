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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Workspace>(b =>
        {
            b.Property(x => x.Name).IsRequired().HasMaxLength(120);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.HasIndex(x => new { x.OwnerId, x.Name });
        });
    }
}
