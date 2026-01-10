using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompeteDesk.Data;
using CompeteDesk.Models;
using CompeteDesk.Services.BusinessAnalysis;
using CompeteDesk.ViewModels.Dashboard;

namespace CompeteDesk.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly BusinessAnalysisService _biz;

    public DashboardController(ApplicationDbContext db, UserManager<IdentityUser> userManager, BusinessAnalysisService biz)
    {
        _db = db;
        _userManager = userManager;
        _biz = biz;
    }

    private async Task<string> GetUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? string.Empty;
    }

    // GET: /Dashboard
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        // Use latest workspace as the active context.
        var ws = await _db.Workspaces
            .AsNoTracking()
            .Where(w => w.OwnerId == userId)
            .OrderByDescending(w => w.UpdatedAtUtc ?? w.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (ws is null)
        {
            // No workspace yet — send user to create one.
            return RedirectToAction("Create", "Workspaces");
        }

        var vm = DashboardViewModel.Sample(User?.Identity?.Name ?? "Strategist");
        vm.WorkspaceId = ws.Id;
        vm.WorkspaceName = ws.Name;
        vm.UserDisplayName = User?.Identity?.Name ?? "Strategist";
        vm.BusinessType = ws.BusinessType;
        vm.Country = ws.Country;
        vm.NeedsBusinessProfile = string.IsNullOrWhiteSpace(ws.BusinessType) || string.IsNullOrWhiteSpace(ws.Country);

        var latest = await _db.BusinessAnalysisReports
            .AsNoTracking()
            .Where(r => r.OwnerId == userId && r.WorkspaceId == ws.Id)
            .OrderByDescending(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (latest != null)
        {
            vm.BusinessAnalysis = MapBusinessAnalysis(latest);
        }

        ViewData["Title"] = "Dashboard";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        return View(vm);
    }

    // POST: /Dashboard/SetBusinessProfile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetBusinessProfile(int workspaceId, string businessType, string country, CancellationToken ct)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var ws = await _db.Workspaces
            .Where(w => w.Id == workspaceId && w.OwnerId == userId)
            .FirstOrDefaultAsync(ct);

        if (ws == null) return NotFound();

        ws.BusinessType = (businessType ?? string.Empty).Trim();
        ws.Country = (country ?? string.Empty).Trim();
        ws.BusinessProfileUpdatedAtUtc = DateTime.UtcNow;
        ws.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Ok(new { ok = true });
    }

    // POST: /Dashboard/GenerateBusinessAnalysis
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateBusinessAnalysis(int workspaceId, CancellationToken ct)
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var ws = await _db.Workspaces
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == workspaceId && w.OwnerId == userId, ct);

        if (ws == null) return NotFound();
        if (string.IsNullOrWhiteSpace(ws.BusinessType) || string.IsNullOrWhiteSpace(ws.Country))
            return BadRequest(new { ok = false, error = "Missing business profile." });

        var output = await _biz.GenerateAsync(
            new BusinessAnalysisService.GenerateInput(ws.Name, ws.BusinessType!, ws.Country!),
            ct);

        var report = new BusinessAnalysisReport
        {
            WorkspaceId = ws.Id,
            OwnerId = userId,
            BusinessType = ws.BusinessType!,
            Country = ws.Country!,
            AiInsightsJson = string.IsNullOrWhiteSpace(output.Json) ? "{}" : output.Json,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.BusinessAnalysisReports.Add(report);
        await _db.SaveChangesAsync(ct);

        return Ok(new { ok = true });
    }

    private static BusinessAnalysisViewModel MapBusinessAnalysis(BusinessAnalysisReport report)
    {
        // Default
        var vm = new BusinessAnalysisViewModel
        {
            CreatedAtUtc = report.CreatedAtUtc
        };

        try
        {
            var parsed = JsonSerializer.Deserialize<BusinessAnalysisResult>(report.AiInsightsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null) return vm;

            vm.Strengths = parsed.Swot?.Strengths ?? new();
            vm.Weaknesses = parsed.Swot?.Weaknesses ?? new();
            vm.Opportunities = parsed.Swot?.Opportunities ?? new();
            vm.Threats = parsed.Swot?.Threats ?? new();

            vm.Rivalry = new ForceVm { Score = parsed.FiveForces?.Rivalry?.Score ?? 0, Notes = parsed.FiveForces?.Rivalry?.Notes };
            vm.NewEntrants = new ForceVm { Score = parsed.FiveForces?.NewEntrants?.Score ?? 0, Notes = parsed.FiveForces?.NewEntrants?.Notes };
            vm.Substitutes = new ForceVm { Score = parsed.FiveForces?.Substitutes?.Score ?? 0, Notes = parsed.FiveForces?.Substitutes?.Notes };
            vm.SupplierPower = new ForceVm { Score = parsed.FiveForces?.SupplierPower?.Score ?? 0, Notes = parsed.FiveForces?.SupplierPower?.Notes };
            vm.BuyerPower = new ForceVm { Score = parsed.FiveForces?.BuyerPower?.Score ?? 0, Notes = parsed.FiveForces?.BuyerPower?.Notes };

            if (parsed.Competitors != null)
            {
                foreach (var c in parsed.Competitors)
                {
                    vm.Competitors.Add(new CompetitorVm
                    {
                        Name = c.Name,
                        WhyRelevant = c.WhyRelevant,
                        Rivalry = new ForceVm { Score = c.FiveForces?.Rivalry?.Score ?? 0, Notes = c.FiveForces?.Rivalry?.Notes },
                        NewEntrants = new ForceVm { Score = c.FiveForces?.NewEntrants?.Score ?? 0, Notes = c.FiveForces?.NewEntrants?.Notes },
                        Substitutes = new ForceVm { Score = c.FiveForces?.Substitutes?.Score ?? 0, Notes = c.FiveForces?.Substitutes?.Notes },
                        SupplierPower = new ForceVm { Score = c.FiveForces?.SupplierPower?.Score ?? 0, Notes = c.FiveForces?.SupplierPower?.Notes },
                        BuyerPower = new ForceVm { Score = c.FiveForces?.BuyerPower?.Score ?? 0, Notes = c.FiveForces?.BuyerPower?.Notes }
                    });
                }
            }
        }
        catch
        {
            // Ignore parse errors.
        }

        return vm;
    }
}
