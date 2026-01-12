// ADDED FILE: CompeteDesk/Controllers/SettingsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CompeteDesk.Models;

namespace CompeteDesk.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var vm = new ViewModels.Settings.SettingsIndexViewModel
            {
                Email = user?.Email ?? "",
                DisplayName = user?.UserName ?? "",
            };

            return View(vm);
        }
    }
}
