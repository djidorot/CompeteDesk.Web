using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CompeteDesk.Data;
using CompeteDesk.Models;
using CompeteDesk.Services.OpenAI;

namespace CompeteDesk.Services.WarRoom;

/// <summary>
/// War Room AI helpers:
/// - Intel Brief (directed telescope): compress selected intel into a usable brief.
/// - Red-Team Plan: stress-test a plan for gaps, risks, contradictions, and missing intel.
///
/// Uses the existing <see cref="OpenAiChatClient"/> which enforces JSON output.
/// </summary>
public sealed class WarRoomAiService
{
    private readonly ApplicationDbContext _db;
    private readonly OpenAiChatClient _ai;

    public WarRoomAiService(ApplicationDbContext db, OpenAiChatClient ai)
    {
        _db = db;
        _ai = ai;
    }

    public bool IsConfigured => _ai.IsConfigured;

    public async Task<JsonDocument> CreateIntelBriefAsync(string ownerId, int[] intelIds, CancellationToken ct)
    {
        intelIds ??= Array.Empty<int>();

        var items = await _db.WarIntel
            .AsNoTracking()
            .Where(x => x.OwnerId == ownerId && intelIds.Contains(x.Id))
            .OrderByDescending(x => x.ObservedAtUtc ?? x.CreatedAtUtc)
            .ToListAsync(ct);

        if (items.Count == 0)
        {
            return JsonDocument.Parse("{\"error\":\"No intel selected.\"}");
        }

        // Keep payload small but useful.
        var payload = new
        {
            intent = "intel_brief",
            nowUtc = DateTime.UtcNow,
            intel = items.Select(i => new
            {
                i.Id,
                i.Title,
                i.Subject,
                i.Signal,
                i.Source,
                i.Tags,
                i.Confidence,
                observedAtUtc = i.ObservedAtUtc,
                notes = string.IsNullOrWhiteSpace(i.Notes) ? null : (i.Notes!.Length > 800 ? i.Notes.Substring(0, 800) : i.Notes)
            })
        };

        var systemPrompt =
            "You are a strategic analyst for a business War Room. " +
            "You must output STRICT JSON only. " +
            "Goal: create a short, actionable intel brief (directed telescope). " +
            "Separate signal from noise. Highlight contradictions and what to verify next. " +
            "If confidence is low, say so and propose verification steps. " +
            "Schema:\n" +
            "{\n" +
            "  \"title\": string,\n" +
            "  \"executiveSummary\": string,\n" +
            "  \"keySignals\": [string],\n" +
            "  \"contradictionsOrConflicts\": [string],\n" +
            "  \"missingIntelQuestions\": [string],\n" +
            "  \"recommendedNextMoves\": [string],\n" +
            "  \"overallConfidence1to5\": number\n" +
            "}";

        var json = JsonSerializer.Serialize(payload);
        var result = await _ai.CreateJsonInsightsAsync(systemPrompt, json, ct);
        return JsonDocument.Parse(result);
    }

    public async Task<JsonDocument> RedTeamPlanAsync(string ownerId, int planId, CancellationToken ct)
    {
        var plan = await _db.WarPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerId == ownerId && x.Id == planId, ct);

        if (plan == null)
            return JsonDocument.Parse("{\"error\":\"Plan not found.\"}");

        // Pull recent intel in same workspace (optional) to challenge assumptions.
        var intel = await _db.WarIntel
            .AsNoTracking()
            .Where(x => x.OwnerId == ownerId && (plan.WorkspaceId == null || x.WorkspaceId == plan.WorkspaceId))
            .OrderByDescending(x => x.ObservedAtUtc ?? x.CreatedAtUtc)
            .Take(12)
            .Select(i => new { i.Title, i.Subject, i.Signal, i.Tags, i.Confidence })
            .ToListAsync(ct);

        var payload = new
        {
            intent = "red_team_plan",
            nowUtc = DateTime.UtcNow,
            plan = new
            {
                plan.Id,
                plan.Name,
                plan.Status,
                plan.Objective,
                plan.Approach,
                plan.Assumptions,
                plan.Risks,
                plan.Contingencies,
                plan.StartAtUtc,
                plan.EndAtUtc
            },
            contextIntel = intel
        };

        var systemPrompt =
            "You are a skeptical red-team strategist. Output STRICT JSON only. " +
            "Your job: stress-test the plan for: unclear objective, weak assumptions, missing counter-moves, " +
            "overconfidence, poor sequencing/tempo, and lack of contingencies. " +
            "Call out contradictions against the provided intel. " +
            "Also propose 2-3 alternative approaches (indirect moves) and 3 verification steps. " +
            "Schema:\n" +
            "{\n" +
            "  \"summary\": string,\n" +
            "  \"criticalGaps\": [string],\n" +
            "  \"assumptionRisks\": [string],\n" +
            "  \"likelyOpponentResponses\": [string],\n" +
            "  \"recommendedAdjustments\": [string],\n" +
            "  \"alternativeApproaches\": [string],\n" +
            "  \"verificationSteps\": [string],\n" +
            "  \"confidenceInPlan1to5\": number\n" +
            "}";

        var json = JsonSerializer.Serialize(payload);
        var result = await _ai.CreateJsonInsightsAsync(systemPrompt, json, ct);
        return JsonDocument.Parse(result);
    }
}
