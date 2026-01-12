using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompeteDesk.Data;
using CompeteDesk.Models;
using CompeteDesk.ViewModels.Settings;

namespace CompeteDesk.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public SettingsController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id ?? string.Empty;

            var prefs = string.IsNullOrWhiteSpace(userId)
                ? null
                : await _db.UserAiPreferences.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);

            var vm = new SettingsIndexViewModel
            {
                Email = user?.Email ?? string.Empty,
                DisplayName = user?.UserName ?? string.Empty,

                Verbosity = prefs?.Verbosity ?? "Balanced",
                Tone = prefs?.Tone ?? "Analytical",
                AutoDraftPlans = prefs?.AutoDraftPlans ?? true,
                AutoSummaries = prefs?.AutoSummaries ?? true,
                AutoRecommendations = prefs?.AutoRecommendations ?? true,
                StoreDecisionTraces = prefs?.StoreDecisionTraces ?? true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsIndexViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            // Basic validation (keep it lightweight)
            vm.Verbosity = NormalizeEnum(vm.Verbosity, "Balanced", "Short", "Balanced", "Detailed");
            vm.Tone = NormalizeEnum(vm.Tone, "Analytical", "Executive", "Analytical", "Tactical");

            var prefs = await _db.UserAiPreferences.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (prefs == null)
            {
                prefs = new UserAiPreferences
                {
                    UserId = user.Id,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _db.UserAiPreferences.Add(prefs);
            }

            prefs.Verbosity = vm.Verbosity;
            prefs.Tone = vm.Tone;
            prefs.AutoDraftPlans = vm.AutoDraftPlans;
            prefs.AutoSummaries = vm.AutoSummaries;
            prefs.AutoRecommendations = vm.AutoRecommendations;
            prefs.StoreDecisionTraces = vm.StoreDecisionTraces;
            prefs.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SettingsSaved"] = "AI preferences updated.";
            return RedirectToAction(nameof(Index));
        }

        private static string NormalizeEnum(string? value, string fallback, params string[] allowed)
        {
            var v = (value ?? "").Trim();
            foreach (var a in allowed)
            {
                if (string.Equals(v, a, StringComparison.OrdinalIgnoreCase))
                    return a;
            }
            return fallback;
        }
    }
}
