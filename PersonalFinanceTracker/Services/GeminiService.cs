using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace PersonalFinanceTracker.Services;

public class GeminiService
{
    // ===== HERE IS YOUR API KEY =====
    // Replace the placeholder below with your Gemini API key from
    // https://aistudio.google.com/app/apikey
    private const string ApiKey = "YOUR_GEMINI_API_KEY_HERE";

    private const string Model    = "gemini-2.5-flash";
    private const string Endpoint = "https://generativelanguage.googleapis.com/v1beta/models";

    private readonly HttpClient _http = new();

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "YOUR_GEMINI_API_KEY_HERE";

    public async Task<string?> SuggestCategoryAsync(string description, IEnumerable<string> categoryNames)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(description))
            return null;

        var categories = string.Join(", ", categoryNames);
        var prompt = $"""
            You categorise personal finance transactions.
            Pick the single best matching category for the description below.
            Reply with ONLY the exact category name from the list, nothing else.

            Available categories: {categories}
            Description: {description}
            """;

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new { temperature = 0.0, maxOutputTokens = 32 }
        };

        var url = $"{Endpoint}/{Model}:generateContent?key={ApiKey}";
        using var resp = await _http.PostAsJsonAsync(url, requestBody);

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!resp.IsSuccessStatusCode)
        {
            var apiMessage = root.TryGetProperty("error", out var err) &&
                             err.TryGetProperty("message", out var msg)
                ? msg.GetString()
                : json;
            throw new InvalidOperationException($"Gemini API error: {apiMessage}");
        }

        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.GetArrayLength() == 0)
            return null;

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.GetArrayLength() == 0 ||
            !parts[0].TryGetProperty("text", out var textElement))
        {
            var reason = candidate.TryGetProperty("finishReason", out var fr) ? fr.GetString() : "unknown";
            throw new InvalidOperationException($"Gemini returned no text (finishReason: {reason}).");
        }

        return textElement.GetString()?.Trim().Trim('"', '.', ',');
    }
}
