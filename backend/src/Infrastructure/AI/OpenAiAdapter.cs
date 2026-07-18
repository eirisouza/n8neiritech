using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using n8neiritech.Application.Interfaces;
using n8neiritech.Infrastructure.Options;

namespace n8neiritech.Infrastructure.AI;

public class OpenAiAdapter : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;
    private readonly ILogger<OpenAiAdapter> _logger;

    public OpenAiAdapter(HttpClient httpClient, IOptions<AiOptions> options, ILogger<OpenAiAdapter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<IntentResult> DetectIntentAsync(string message, string? context = null, CancellationToken ct = default)
    {
        var prompt = $"Return JSON with intent, confidence, subIntent. Context: {context ?? "none"}. Message: {message}";
        var content = await ChatAsync(prompt, ct);
        try
        {
            var result = JsonSerializer.Deserialize<IntentResult>(content, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return result ?? new IntentResult("general", 0.5, null);
        }
        catch
        {
            return new IntentResult("general", 0.5, null);
        }
    }

    public async Task<EntityExtractionResult> ExtractEntitiesAsync(string message, CancellationToken ct = default)
    {
        var prompt = $"Extract product, category, brand, color, size, quantity, address, neighborhood, paymentMethod, minPrice, maxPrice from: {message}. Return JSON.";
        var content = await ChatAsync(prompt, ct);
        try
        {
            var result = JsonSerializer.Deserialize<EntityExtractionResult>(content, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return result ?? new EntityExtractionResult(null, null, null, null, null, null, null, null, null, null, null);
        }
        catch
        {
            return new EntityExtractionResult(null, null, null, null, null, null, null, null, null, null, null);
        }
    }

    public Task<string> GenerateResponseAsync(string prompt, string context, CancellationToken ct = default)
        => ChatAsync($"{prompt}\n\nContext: {context}", ct);

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/embeddings", new { input = text, model = "text-embedding-3-small" }, ct);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var payload = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: ct);
            return payload?.Data?.FirstOrDefault()?.Embedding?.ToArray() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI embedding generation failed.");
            return [];
        }
    }

    private async Task<string> ChatAsync(string prompt, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", new
            {
                model = _options.Model,
                temperature = 0.2,
                messages = new object[]
                {
                    new { role = "system", content = "You are an assistant for a WhatsApp commerce backend. Return concise, structured responses." },
                    new { role = "user", content = prompt }
                }
            }, ct);

            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
            return payload?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI chat call failed.");
            return string.Empty;
        }
    }

    private sealed record ChatResponse(List<ChatChoice>? Choices);
    private sealed record ChatChoice(ChatMessage? Message);
    private sealed record ChatMessage(string? Content);
    private sealed record EmbeddingResponse(List<EmbeddingData>? Data);
    private sealed record EmbeddingData(List<float>? Embedding);
}
