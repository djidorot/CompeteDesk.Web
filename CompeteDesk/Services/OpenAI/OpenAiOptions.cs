namespace CompeteDesk.Services.OpenAI;

public sealed class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Example: "gpt-4o-mini" or "gpt-4.1-mini".
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";

    public double Temperature { get; set; } = 0.2;
}
