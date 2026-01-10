using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CompeteDesk.Services.OpenAI;

/// <summary>
/// Minimal OpenAI Chat Completions client (no external SDK).
/// Uses: POST https://api.openai.com/v1/chat/completions
/// </summary>
public sealed class OpenAiChatClient
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _opt;

    public OpenAiChatClient(HttpClient http, IOptions<OpenAiOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_opt.ApiKey);

    public async Task<string> CreateJsonInsightsAsync(string systemPrompt, string userJsonPayload, CancellationToken ct)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("OpenAI is not configured. Set OpenAI:ApiKey in appsettings or user-secrets.");

        var req = new
        {
            model = _opt.Model,
            temperature = _opt.Temperature,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userJsonPayload }
            }
        };

        using var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey);
        msg.Content = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(msg, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"OpenAI call failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {body}");

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? "{}";
    }
}
