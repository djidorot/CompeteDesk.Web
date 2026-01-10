using Microsoft.EntityFrameworkCore;

namespace CompeteDesk.Data
{
    public static class DbBootstrapper
    {
        // Overload 1: call with IServiceProvider (app.Services)
        public static async Task EnsureWorkspacesTableAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await EnsureWorkspacesTableAsync(db);
        }

        // Overload 2: call with ApplicationDbContext (db)
        public static async Task EnsureWorkspacesTableAsync(ApplicationDbContext db)
        {
            // Create DB file + tables based on EF model (including Workspaces)
            await db.Database.EnsureCreatedAsync();
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
