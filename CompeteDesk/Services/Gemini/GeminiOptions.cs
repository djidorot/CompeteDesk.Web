using System;

namespace CompeteDesk.Services.Gemini;

/// <summary>
/// Configuration for Gemini Developer API (Google Generative Language API).
/// </summary>
public sealed class GeminiOptions
{
    /// <summary>API key for Gemini Developer API.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API version segment. Use "v1" for current stable endpoints.
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Model id, e.g. "gemini-2.5-flash" or "gemini-1.5-flash".
    /// Do NOT include the "models/" prefix; we normalize if you do.
    /// </summary>
    public string Model { get; set; } = "gemini-2.5-flash";

    /// <summary>
    /// System instruction to keep answers concise and helpful for "search".
    /// </summary>
    public string SystemInstruction { get; set; } =
        "You are an assistant integrated into an app search bar. " +
        "Answer the user's query with concise, practical information (3-8 bullets max). " +
        "If the query is ambiguous, ask one brief clarifying question. " +
        "Avoid mentioning internal app implementation details.";

    public int MaxOutputTokens { get; set; } = 512;
    public double Temperature { get; set; } = 0.3;
}
