using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompeteDesk.Controllers;

[Authorize]
public class HabitsController : Controller
{
    // GET: /Habits
    public IActionResult Index()
    {
        ViewData["Title"] = "Habits";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;
        return View();
    }
}
