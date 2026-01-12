using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompeteDesk.Data;
using CompeteDesk.ViewModels.Metrics;

namespace CompeteDesk.Controllers;

[Authorize]
public class MetricsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public MetricsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private async Task<string> GetUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? string.Empty;
    }

    // GET: /Metrics
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Metrics";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var model = new MetricsViewModel
        {
            WorkspacesCount = await _db.Workspaces.AsNoTracking().CountAsync(x => x.OwnerId == userId),
            StrategiesCount = await _db.Strategies.AsNoTracking().CountAsync(x => x.OwnerId == userId),
            ActionsCount = await _db.Actions.AsNoTracking().CountAsync(x => x.OwnerId == userId),

            HabitsCount = await _db.Habits.AsNoTracking().CountAsync(x => x.OwnerId == userId),
            ActiveHabitsCount = await _db.Habits.AsNoTracking().CountAsync(x => x.OwnerId == userId && x.IsActive),

            WarIntelCount = await _db.WarIntel.AsNoTracking().CountAsync(x => x.OwnerId == userId),
            WarPlansCount = await _db.WarPlans.AsNoTracking().CountAsync(x => x.OwnerId == userId),

            WebsiteReportsCount = await _db.WebsiteAnalysisReports.AsNoTracking().CountAsync(x => x.OwnerId == userId),
            BusinessReportsCount = await _db.BusinessAnalysisReports.AsNoTracking().CountAsync(x => x.OwnerId == userId),

            AiTracesCount = await _db.DecisionTraces.AsNoTracking().CountAsync(x => x.OwnerId == userId),
        };

        // Action status breakdown (Planned / In Progress / Done / Archived etc.)
        var statusGroups = await _db.Actions.AsNoTracking()
            .Where(x => x.OwnerId == userId)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Status)
            .ToListAsync();

        foreach (var g in statusGroups)
        {
            model.ActionStatuses.Add(new MetricsViewModel.StatusCount
            {
                Status = string.IsNullOrWhiteSpace(g.Status) ? "(none)" : g.Status,
                Count = g.Count
            });
        }

        return View(model);
    }
}
