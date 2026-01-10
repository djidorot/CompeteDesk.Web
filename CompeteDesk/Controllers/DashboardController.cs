using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompeteDesk.ViewModels.Dashboard;

namespace CompeteDesk.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // NOTE: This is seed/sample data for the initial UI.
            // Replace with real data from your domain/services later.
            var vm = DashboardViewModel.Sample(User?.Identity?.Name ?? "Strategist");

            // Make dashboard full-width inside _Layout
            ViewData["LayoutFluid"] = true;

            return View(vm);
        }
    }
}
