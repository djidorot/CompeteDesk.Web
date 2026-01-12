using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CompeteDesk.Services.Ai;
using CompeteDesk.Services.Gemini;

namespace CompeteDesk.Controllers;

[Authorize]
[ApiController]
public sealed class AiSearchController : ControllerBase
{
    private readonly GeminiClient _gemini;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly DecisionTraceService _trace;

    public AiSearchController(
        GeminiClient gemini,
        UserManager<IdentityUser> userManager,
        DecisionTraceService trace)
    {
        _gemini = gemini;
        _userManager = userManager;
        _trace = trace;
    }

    // GET /api/ai-search?q=...
    [HttpGet("/api/ai-search")]
    public async Task<IActionResult> Get([FromQuery] string? q, CancellationToken ct)
    {
        var query = (q ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Query is required." });

        if (query.Length > 400)
            return BadRequest(new { error = "Query is too long (max 400 characters)." });

        var sw = Stopwatch.StartNew();
        string answer;

        try
        {
            answer = await _gemini.GenerateForSearchAsync(query, ct);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        finally
        {
            sw.Stop();
        }

        // Optional: log to AI History (DecisionTraces)
        try
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var outputJson = JsonSerializer.Serialize(new { answer }, new JsonSerializerOptions { WriteIndented = false });
                await _trace.LogAsync(
                    ownerId: userId,
                    workspaceId: null,
                    feature: "TopbarAiSearch",
                    input: new { query },
                    outputJson: outputJson,
                    aiProvider: "Gemini",
                    model: null,
                    temperature: null,
                    ct: ct);
            }
        }
        catch
        {
            // Don't block UX if trace logging fails.
        }

        return Ok(new
        {
            answer,
            elapsedMs = sw.ElapsedMilliseconds
        });
    }
}
