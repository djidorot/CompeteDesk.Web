// UPDATED FILE: CompeteDesk/Controllers/SettingsController.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CompeteDesk.ViewModels.Settings;

namespace CompeteDesk.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public SettingsController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var vm = new SettingsIndexViewModel
            {
                Email = user?.Email ?? string.Empty,
                DisplayName = user?.UserName ?? string.Empty,
            };

            return View(vm);
        }
    }
}
