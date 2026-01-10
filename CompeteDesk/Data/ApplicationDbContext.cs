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
	public DbSet<ActionItem> Actions => Set<ActionItem>();
	public DbSet<WarIntel> WarIntel => Set<WarIntel>();
	public DbSet<WarPlan> WarPlans => Set<WarPlan>();
    public DbSet<WebsiteAnalysisReport> WebsiteAnalysisReports => Set<WebsiteAnalysisReport>();

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
            b.Property(x => x.AiInsightsJson);
            b.Property(x => x.AiSummary);

            b.HasIndex(x => new { x.OwnerId, x.Status });
            b.HasIndex(x => new { x.WorkspaceId, x.OwnerId });
        });

		builder.Entity<ActionItem>(b =>
		{
			b.Property(x => x.Title).IsRequired().HasMaxLength(200);
			b.Property(x => x.Description).HasMaxLength(2000);
			b.Property(x => x.Status).IsRequired().HasMaxLength(24);
			b.Property(x => x.Category).HasMaxLength(80);
			b.Property(x => x.SourceBook).HasMaxLength(120);

			b.HasIndex(x => new { x.OwnerId, x.Status });
			b.HasIndex(x => new { x.StrategyId, x.OwnerId });
			b.HasIndex(x => new { x.WorkspaceId, x.OwnerId });
		});

		builder.Entity<WarIntel>(b =>
		{
			b.Property(x => x.Title).IsRequired().HasMaxLength(200);
			b.Property(x => x.Subject).HasMaxLength(120);
			b.Property(x => x.Signal).HasMaxLength(2000);
			b.Property(x => x.Source).HasMaxLength(300);
			b.Property(x => x.Tags).HasMaxLength(200);
			b.Property(x => x.Notes).HasMaxLength(4000);

			b.HasIndex(x => new { x.OwnerId, x.Confidence });
			b.HasIndex(x => new { x.WorkspaceId, x.OwnerId });
		});

		builder.Entity<WarPlan>(b =>
		{
			b.Property(x => x.Name).IsRequired().HasMaxLength(200);
			b.Property(x => x.Objective).HasMaxLength(2000);
			b.Property(x => x.Approach).HasMaxLength(2000);
			b.Property(x => x.Assumptions).HasMaxLength(4000);
			b.Property(x => x.Risks).HasMaxLength(4000);
			b.Property(x => x.Contingencies).HasMaxLength(4000);
			b.Property(x => x.Status).IsRequired().HasMaxLength(24);
			b.Property(x => x.SourceBook).HasMaxLength(120);

			b.HasIndex(x => new { x.OwnerId, x.Status });
			b.HasIndex(x => new { x.WorkspaceId, x.OwnerId });
		});
        builder.Entity<WebsiteAnalysisReport>(b =>
        {
            b.Property(x => x.Url).IsRequired().HasMaxLength(2048);
            b.Property(x => x.FinalUrl).HasMaxLength(512);
            b.Property(x => x.Title).HasMaxLength(512);
            b.Property(x => x.MetaDescription).HasMaxLength(1024);
            b.Property(x => x.AiInsightsJson);
            b.Property(x => x.AiSummary);
            b.Property(x => x.OwnerId).IsRequired();
            b.HasIndex(x => new { x.OwnerId, x.CreatedAtUtc });
            b.HasIndex(x => new { x.OwnerId, x.Url });
            b.HasIndex(x => x.WorkspaceId);
        });

    }
}
