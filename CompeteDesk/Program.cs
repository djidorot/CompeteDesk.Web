using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CompeteDesk.Data;
using CompeteDesk.Services.OpenAI;
using CompeteDesk.Services.WebsiteAnalysis;
using CompeteDesk.Services.BusinessAnalysis;
using CompeteDesk.Services.WarRoom;
using CompeteDesk.Services.Ai;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ------------------------------------------------------------
// Website Analysis + OpenAI
// ------------------------------------------------------------
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));

// HttpClient for site fetches (analysis).
builder.Services.AddHttpClient("site-analyzer", c =>
{
    c.Timeout = TimeSpan.FromSeconds(20);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("CompeteDeskSiteAnalyzer/1.0");
});

// HttpClient for OpenAI.
builder.Services.AddHttpClient<OpenAiChatClient>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(40);
});

builder.Services.AddScoped<WebsiteAnalysisService>();
builder.Services.AddScoped<BusinessAnalysisService>();
builder.Services.AddScoped<WarRoomAiService>();
builder.Services.AddScoped<DecisionTraceService>();
builder.Services.AddScoped<AiContextPackBuilder>();

// Identity + External Login (Google)
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        // For consumer apps, requiring confirmed account often blocks external logins
        // unless you implement an email confirmation flow. Keep it simple for now.
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services
    .AddAuthentication()
    ;

// External Login (Google) - only enable if credentials exist.
// Prevents runtime crash: ArgumentException "ClientId cannot be empty".
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services
        .AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure Workspace CRUD works even if you already have an existing app.db.
// (Creates the Workspaces table if missing.)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbBootstrapper.EnsureCoreTablesAsync(app.Services);

}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// IMPORTANT: Auth must run before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
