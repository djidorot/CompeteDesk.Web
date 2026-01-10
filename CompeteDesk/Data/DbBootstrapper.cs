using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CompeteDesk.Data
{
    public static class DbBootstrapper
    {
        /// <summary>
        /// Create missing tables for the current MVP without requiring EF migrations.
        /// Why: The project currently uses Identity migrations, but MVP tables (Workspaces, Strategies)
        /// are created at runtime so devs can just run the app.
        /// </summary>
        public static async Task EnsureCoreTablesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await EnsureCoreTablesAsync(db);
        }

        public static async Task EnsureCoreTablesAsync(ApplicationDbContext db)
        {
            // Ensure SQLite file exists and Identity tables exist (via migrations/ensure)
            await db.Database.EnsureCreatedAsync();

            // IMPORTANT:
            // - EnsureCreatedAsync() will NOT update an existing schema.
            // - Your repo may already contain an older app.db with a Workspaces table
            //   missing newer columns (e.g., OwnerId). Creating indexes on missing columns
            //   will crash the app.
            // So: create tables if missing, otherwise "patch" missing columns before indexes.

            await EnsureWorkspacesTableAsync(db);
            await EnsureStrategiesTableAsync(db);
			await EnsureActionsTableAsync(db);
			await EnsureWarIntelTableAsync(db);
			await EnsureWarPlansTableAsync(db);
			await EnsureWebsiteAnalysisReportsTableAsync(db);
        }

		private static async Task EnsureWarIntelTableAsync(ApplicationDbContext db)
		{
			if (!await TableExistsAsync(db, "WarIntel"))
			{
				await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE WarIntel (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkspaceId INTEGER NULL,
    OwnerId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Subject TEXT NULL,
    Signal TEXT NULL,
    Source TEXT NULL,
    Confidence INTEGER NOT NULL DEFAULT 3,
    Tags TEXT NULL,
    Notes TEXT NULL,
    ObservedAtUtc TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NULL,
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces (Id) ON DELETE SET NULL
);");
			}
			else
			{
				await EnsureColumnAsync(db, "WarIntel", "WorkspaceId", "INTEGER", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "OwnerId", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "Title", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "Subject", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "Signal", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "Source", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "Confidence", "INTEGER", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "Tags", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "Notes", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "ObservedAtUtc", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "CreatedAtUtc", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarIntel", "UpdatedAtUtc", "TEXT", nullable: true);
			}

			await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_WarIntel_OwnerId_Confidence
ON WarIntel (OwnerId, Confidence);");

			await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_WarIntel_WorkspaceId_OwnerId
ON WarIntel (WorkspaceId, OwnerId);");
		}

		private static async Task EnsureWarPlansTableAsync(ApplicationDbContext db)
		{
			if (!await TableExistsAsync(db, "WarPlans"))
			{
				await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE WarPlans (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkspaceId INTEGER NULL,
    OwnerId TEXT NOT NULL,
    Name TEXT NOT NULL,
    Objective TEXT NULL,
    Approach TEXT NULL,
    Assumptions TEXT NULL,
    Risks TEXT NULL,
    Contingencies TEXT NULL,
    Status TEXT NOT NULL,
    StartAtUtc TEXT NULL,
    EndAtUtc TEXT NULL,
    SourceBook TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NULL,
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces (Id) ON DELETE SET NULL
);");
			}
			else
			{
				await EnsureColumnAsync(db, "WarPlans", "WorkspaceId", "INTEGER", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "OwnerId", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "Name", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "Objective", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "Approach", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "Assumptions", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "Risks", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "Contingencies", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "Status", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "StartAtUtc", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "EndAtUtc", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "SourceBook", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "CreatedAtUtc", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "WarPlans", "UpdatedAtUtc", "TEXT", nullable: true);
			}

			await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_WarPlans_OwnerId_Status
ON WarPlans (OwnerId, Status);");

			await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_WarPlans_WorkspaceId_OwnerId
ON WarPlans (WorkspaceId, OwnerId);");
		}

        private static async Task EnsureWorkspacesTableAsync(ApplicationDbContext db)
        {
            if (!await TableExistsAsync(db, "Workspaces"))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE Workspaces (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NULL,
    OwnerId TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NULL
);");
            }
            else
            {
                // Patch older schemas safely (nullable columns + defaults avoided).
                await EnsureColumnAsync(db, "Workspaces", "Name", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Workspaces", "Description", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Workspaces", "OwnerId", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Workspaces", "CreatedAtUtc", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Workspaces", "UpdatedAtUtc", "TEXT", nullable: true);
            }

            // Create indexes AFTER columns exist.
            await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_Workspaces_OwnerId_Name
ON Workspaces (OwnerId, Name);");
        }

        private static async Task EnsureStrategiesTableAsync(ApplicationDbContext db)
        {
            if (!await TableExistsAsync(db, "Strategies"))
            {
                await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE Strategies (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkspaceId INTEGER NULL,
    OwnerId TEXT NOT NULL,
    Name TEXT NOT NULL,
    SourceBook TEXT NULL,
    CorePrinciple TEXT NULL,
    Summary TEXT NULL,
    Category TEXT NULL,
    Status TEXT NOT NULL,
    Priority INTEGER NOT NULL DEFAULT 0,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NULL,
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces (Id) ON DELETE SET NULL
);");
            }
            else
            {
                await EnsureColumnAsync(db, "Strategies", "WorkspaceId", "INTEGER", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "OwnerId", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "Name", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "SourceBook", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "CorePrinciple", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "Summary", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "Category", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "Status", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "Priority", "INTEGER", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "CreatedAtUtc", "TEXT", nullable: true);
                await EnsureColumnAsync(db, "Strategies", "UpdatedAtUtc", "TEXT", nullable: true);
            }

            await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_Strategies_OwnerId_Status
ON Strategies (OwnerId, Status);");

            await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_Strategies_WorkspaceId_OwnerId
ON Strategies (WorkspaceId, OwnerId);");
        }

		private static async Task EnsureActionsTableAsync(ApplicationDbContext db)
		{
			if (!await TableExistsAsync(db, "Actions"))
			{
				await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE Actions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkspaceId INTEGER NULL,
    StrategyId INTEGER NULL,
    OwnerId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    Category TEXT NULL,
    Status TEXT NOT NULL,
    Priority INTEGER NOT NULL DEFAULT 0,
    DueAtUtc TEXT NULL,
    SourceBook TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    UpdatedAtUtc TEXT NULL,
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces (Id) ON DELETE SET NULL,
    FOREIGN KEY (StrategyId) REFERENCES Strategies (Id) ON DELETE SET NULL
);");
			}
			else
			{
				await EnsureColumnAsync(db, "Actions", "WorkspaceId", "INTEGER", nullable: true);
				await EnsureColumnAsync(db, "Actions", "StrategyId", "INTEGER", nullable: true);
				await EnsureColumnAsync(db, "Actions", "OwnerId", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "Title", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "Description", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "Category", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "Status", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "Priority", "INTEGER", nullable: true);
				await EnsureColumnAsync(db, "Actions", "DueAtUtc", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "SourceBook", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "CreatedAtUtc", "TEXT", nullable: true);
				await EnsureColumnAsync(db, "Actions", "UpdatedAtUtc", "TEXT", nullable: true);
			}

			await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_Actions_OwnerId_Status
ON Actions (OwnerId, Status);");

			await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_Actions_StrategyId_OwnerId
ON Actions (StrategyId, OwnerId);");

			await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS IX_Actions_WorkspaceId_OwnerId
ON Actions (WorkspaceId, OwnerId);");
		}

        private static async Task<bool> TableExistsAsync(ApplicationDbContext db, string tableName)
        {
            // sqlite_master query is safe and fast.
            var conn = (SqliteConnection)db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name=$name LIMIT 1;";
                cmd.Parameters.AddWithValue("$name", tableName);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }

        private static async Task EnsureColumnAsync(
            ApplicationDbContext db,
            string tableName,
            string columnName,
            string sqliteType,
            bool nullable)
        {
            var conn = (SqliteConnection)db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync();
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA table_info(\"{tableName}\");";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    if (string.Equals(name, columnName, StringComparison.OrdinalIgnoreCase))
                        return;
                }
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }

            // If missing, add it.
            // NOTE: SQLite has limited ALTER TABLE support; ADD COLUMN is supported.
            var nullSql = nullable ? "NULL" : "NOT NULL";
            await db.Database.ExecuteSqlRawAsync($"ALTER TABLE \"{tableName}\" ADD COLUMN \"{columnName}\" {sqliteType} {nullSql};");
        }

		private static async Task EnsureWebsiteAnalysisReportsTableAsync(ApplicationDbContext db)
		{
			if (!await TableExistsAsync(db, "WebsiteAnalysisReports"))
			{
				await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE WebsiteAnalysisReports (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkspaceId INTEGER NULL,
    OwnerId TEXT NOT NULL,
    Url TEXT NOT NULL,
    FinalUrl TEXT NULL,
    HttpStatusCode INTEGER NOT NULL DEFAULT 0,
    ResponseTimeMs INTEGER NOT NULL DEFAULT 0,
    Title TEXT NULL,
    MetaDescription TEXT NULL,
    WordCount INTEGER NOT NULL DEFAULT 0,
    H1Count INTEGER NOT NULL DEFAULT 0,
    H2Count INTEGER NOT NULL DEFAULT 0,
    InternalLinkCount INTEGER NOT NULL DEFAULT 0,
    ExternalLinkCount INTEGER NOT NULL DEFAULT 0,
    ImageCount INTEGER NOT NULL DEFAULT 0,
    ImagesMissingAltCount INTEGER NOT NULL DEFAULT 0,
    IsHttps INTEGER NOT NULL DEFAULT 0,
    HasViewportMeta INTEGER NOT NULL DEFAULT 0,
    HasRobotsNoindex INTEGER NOT NULL DEFAULT 0,
    HasCanonicalLink INTEGER NOT NULL DEFAULT 0,
    HasOpenGraph INTEGER NOT NULL DEFAULT 0,
    HasCspHeader INTEGER NOT NULL DEFAULT 0,
    HasHstsHeader INTEGER NOT NULL DEFAULT 0,
    AiInsightsJson TEXT NULL,
    AiSummary TEXT NULL,
    CreatedAtUtc TEXT NOT NULL,
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces (Id) ON DELETE SET NULL
);");

				// Helpful indexes for listing per user.
				await EnsureIndexAsync(db, "IX_WebsiteAnalysisReports_Owner_CreatedAtUtc",
					"WebsiteAnalysisReports", "(OwnerId, CreatedAtUtc)");
				await EnsureIndexAsync(db, "IX_WebsiteAnalysisReports_Owner_Url",
					"WebsiteAnalysisReports", "(OwnerId, Url)");
			}
			else
			{
				// Patch older schemas (if the table exists but is missing columns).
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "WorkspaceId", "INTEGER", true);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "OwnerId", "TEXT", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "Url", "TEXT", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "FinalUrl", "TEXT", true);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "HttpStatusCode", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "ResponseTimeMs", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "Title", "TEXT", true);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "MetaDescription", "TEXT", true);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "WordCount", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "H1Count", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "H2Count", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "InternalLinkCount", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "ExternalLinkCount", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "ImageCount", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "ImagesMissingAltCount", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "IsHttps", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "HasViewportMeta", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "HasRobotsNoindex", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "HasCanonicalLink", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "HasOpenGraph", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "HasCspHeader", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "HasHstsHeader", "INTEGER", false);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "AiInsightsJson", "TEXT", true);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "AiSummary", "TEXT", true);
				await EnsureColumnAsync(db, "WebsiteAnalysisReports", "CreatedAtUtc", "TEXT", false);

				await EnsureIndexAsync(db, "IX_WebsiteAnalysisReports_Owner_CreatedAtUtc",
					"WebsiteAnalysisReports", "(OwnerId, CreatedAtUtc)");
				await EnsureIndexAsync(db, "IX_WebsiteAnalysisReports_Owner_Url",
					"WebsiteAnalysisReports", "(OwnerId, Url)");
			}
		}
        // Optional for later (migrations-based)
        public static void EnsureDatabaseUpToDate(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }
}
