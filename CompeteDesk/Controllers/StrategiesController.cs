using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompeteDesk.Data;
using CompeteDesk.Models;

namespace CompeteDesk.Controllers;

[Authorize]
public class StrategiesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public StrategiesController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private async Task<string> GetUserIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.Id ?? string.Empty;
    }

    // GET: /Strategies
    public async Task<IActionResult> Index(string? q, string status = "Active")
    {
        ViewData["Title"] = "Strategies";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var query = _db.Strategies
            .AsNoTracking()
            .Where(x => x.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.Name.Contains(term) ||
                (x.CorePrinciple != null && x.CorePrinciple.Contains(term)) ||
                (x.Category != null && x.Category.Contains(term)));
        }

        var items = await query
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .ThenBy(x => x.Name)
            .ToListAsync();

        ViewBag.Query = q ?? string.Empty;
        ViewBag.Status = status;

        return View(items);
    }

    // GET: /Strategies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        ViewData["Title"] = "Strategy Details";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        var userId = await GetUserIdAsync();
        var item = await _db.Strategies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);

        return item == null ? NotFound() : View(item);
    }

    // GET: /Strategies/Create
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New Strategy";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        // Default values
        var model = new Strategy
        {
            SourceBook = "The 33 Strategies of War",
            Status = "Active",
            Priority = 0
        };

        // If user has 0 strategies, gently suggest seeding.
        var userId = await GetUserIdAsync();
        ViewBag.HasAny = await _db.Strategies.AnyAsync(x => x.OwnerId == userId);

        return View(model);
    }

    // POST: /Strategies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Strategy model)
    {
        ViewData["Title"] = "New Strategy";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        model.OwnerId = userId;
        model.CreatedAtUtc = DateTime.UtcNow;
        model.UpdatedAtUtc = DateTime.UtcNow;
        model.Status = string.IsNullOrWhiteSpace(model.Status) ? "Active" : model.Status;
        model.SourceBook = string.IsNullOrWhiteSpace(model.SourceBook) ? "The 33 Strategies of War" : model.SourceBook;

        if (!ModelState.IsValid) return View(model);

        _db.Strategies.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Strategies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        ViewData["Title"] = "Edit Strategy";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        var userId = await GetUserIdAsync();
        var item = await _db.Strategies.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        return item == null ? NotFound() : View(item);
    }

    // POST: /Strategies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Strategy model)
    {
        if (id != model.Id) return NotFound();

        ViewData["Title"] = "Edit Strategy";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        var userId = await GetUserIdAsync();
        var item = await _db.Strategies.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        if (item == null) return NotFound();

        if (!ModelState.IsValid) return View(model);

        // Update whitelisted fields
        item.Name = model.Name;
        item.SourceBook = model.SourceBook;
        item.CorePrinciple = model.CorePrinciple;
        item.Summary = model.Summary;
        item.Category = model.Category;
        item.Status = string.IsNullOrWhiteSpace(model.Status) ? item.Status : model.Status;
        item.Priority = model.Priority;
        item.WorkspaceId = model.WorkspaceId;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Strategies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        ViewData["Title"] = "Delete Strategy";
        ViewData["LayoutFluid"] = true;
        ViewData["UseSidebar"] = true;

        var userId = await GetUserIdAsync();
        var item = await _db.Strategies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);

        return item == null ? NotFound() : View(item);
    }

    // POST: /Strategies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = await GetUserIdAsync();
        var item = await _db.Strategies.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == userId);
        if (item == null) return NotFound();

        _db.Strategies.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: /Strategies/SeedFromBook
    // Inserts the 33 strategy titles (as "Active") if the user has none yet.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedFromBook()
    {
        var userId = await GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var any = await _db.Strategies.AnyAsync(x => x.OwnerId == userId);
        if (any) return RedirectToAction(nameof(Index));

        var now = DateTime.UtcNow;

        // Titles based on the book’s opening table-of-contents section.
        // Keep CorePrinciple short; users can expand each strategy with their own business context.
        var seeds = new[]
        {
            // Part I — Self-Directed Warfare
            new Strategy { Name = "Declare War on Your Enemies", Category = "Self-Directed", CorePrinciple = "Identify the real opposition and commit.", Priority = 50 },
            new Strategy { Name = "Do Not Fight the Last War", Category = "Self-Directed", CorePrinciple = "Adapt to the present; avoid repeating patterns.", Priority = 50 },
            new Strategy { Name = "Amidst the Turmoil of Events, Do Not Lose Your Presence of Mind", Category = "Self-Directed", CorePrinciple = "Stay balanced under pressure.", Priority = 50 },
            new Strategy { Name = "Create a Sense of Urgency and Desperation", Category = "Self-Directed", CorePrinciple = "Use constraints to force action.", Priority = 50 },

            // Part II — Organizational (Team) Warfare
            new Strategy { Name = "Avoid the Snares of Groupthink", Category = "Organizational", CorePrinciple = "Lead clearly without herd irrationality.", Priority = 40 },
            new Strategy { Name = "Segment Your Forces", Category = "Organizational", CorePrinciple = "Small autonomous units move faster.", Priority = 40 },
            new Strategy { Name = "Transform Your War into a Crusade", Category = "Organizational", CorePrinciple = "Morale rises when mission is shared.", Priority = 40 },

            // Part III — Defensive Warfare
            new Strategy { Name = "Pick Your Battles Carefully", Category = "Defensive", CorePrinciple = "Choose conflicts with favorable economics.", Priority = 30 },
            new Strategy { Name = "Turn the Tables", Category = "Defensive", CorePrinciple = "Let them move first; counter from advantage.", Priority = 30 },
            new Strategy { Name = "Create a Threatening Presence", Category = "Defensive", CorePrinciple = "Deterrence through reputation and uncertainty.", Priority = 30 },
            new Strategy { Name = "Trade Space for Time", Category = "Defensive", CorePrinciple = "Nonengagement buys time to reposition.", Priority = 30 },

            // Part IV — Offensive Warfare
            new Strategy { Name = "Lose Battles but Win the War", Category = "Offensive", CorePrinciple = "Optimize for the ultimate objective.", Priority = 35 },
            new Strategy { Name = "Know Your Enemy", Category = "Offensive", CorePrinciple = "Intelligence: read motives and signals.", Priority = 35 },
            new Strategy { Name = "Overwhelm Resistance with Speed and Suddenness", Category = "Offensive", CorePrinciple = "Speed creates imbalance and error.", Priority = 35 },
            new Strategy { Name = "Control the Dynamic", Category = "Offensive", CorePrinciple = "Define the relationship’s terms.", Priority = 25 },
            new Strategy { Name = "Hit Them Where It Hurts", Category = "Offensive", CorePrinciple = "Attack the center of gravity.", Priority = 25 },
            new Strategy { Name = "Defeat Them in Detail", Category = "Offensive", CorePrinciple = "Split the problem; win sequentially.", Priority = 25 },
            new Strategy { Name = "Expose and Attack Your Opponent's Soft Flank", Category = "Offensive", CorePrinciple = "Avoid head-on resistance; attack the side.", Priority = 25 },
            new Strategy { Name = "Envelop the Enemy", Category = "Offensive", CorePrinciple = "Apply pressure from all sides.", Priority = 25 },
            new Strategy { Name = "Maneuver Them into Weakness", Category = "Offensive", CorePrinciple = "Position them so victory is easy.", Priority = 25 },
            new Strategy { Name = "Negotiate While Advancing", Category = "Offensive", CorePrinciple = "Talk while creating relentless leverage.", Priority = 25 },
            new Strategy { Name = "Know How to End Things", Category = "Offensive", CorePrinciple = "Choose exits; end cleanly.", Priority = 25 },

            // Part V — Unconventional (Dirty) Warfare
            new Strategy { Name = "Weave a Seamless Blend of Fact and Fiction", Category = "Unconventional", CorePrinciple = "Control perceptions; manufacture reality.", Priority = 20 },
            new Strategy { Name = "Take the Line of Least Expectation", Category = "Unconventional", CorePrinciple = "Upset patterns; strike where unexpected.", Priority = 20 },
            new Strategy { Name = "Occupy the Moral High Ground", Category = "Unconventional", CorePrinciple = "Frame your cause as more just.", Priority = 20 },
            new Strategy { Name = "Deny Them Targets", Category = "Unconventional", CorePrinciple = "Be elusive; offer no clean target.", Priority = 20 },
            new Strategy { Name = "Seem to Work for the Interests of Others While Furthering Your Own", Category = "Unconventional", CorePrinciple = "Alliances: let others do the work.", Priority = 20 },
            new Strategy { Name = "Give Your Rivals Enough Rope to Hang Themselves", Category = "Unconventional", CorePrinciple = "Make them self-destruct.", Priority = 20 },
            new Strategy { Name = "Take Small Bites", Category = "Unconventional", CorePrinciple = "Accumulate quietly; avoid sharp grabs.", Priority = 20 },
            new Strategy { Name = "Penetrate Their Minds", Category = "Unconventional", CorePrinciple = "Communication as psychological warfare.", Priority = 20 },
            new Strategy { Name = "Destroy from Within", Category = "Unconventional", CorePrinciple = "Infiltrate and turn the inner front.", Priority = 20 },
            new Strategy { Name = "Dominate While Seeming to Submit", Category = "Unconventional", CorePrinciple = "Hidden aggression; control through compliance.", Priority = 20 },
            new Strategy { Name = "Sow Uncertainty and Panic Through Acts of Terror", Category = "Unconventional", CorePrinciple = "Trigger overreaction; keep your balance.", Priority = 20 }
        };

        foreach (var s in seeds)
        {
            s.OwnerId = userId;
            s.SourceBook = "The 33 Strategies of War";
            s.Status = "Active";
            s.CreatedAtUtc = now;
            s.UpdatedAtUtc = now;
        }

        _db.Strategies.AddRange(seeds);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
