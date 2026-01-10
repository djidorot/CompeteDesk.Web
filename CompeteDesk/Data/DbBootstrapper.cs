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

        // Optional for later (migrations-based)
        public static void EnsureDatabaseUpToDate(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }
}
