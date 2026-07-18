namespace n8neiritech.Application.Interfaces;

public interface IAiProvider
{
    Task<IntentResult> DetectIntentAsync(string message, string? context = null, CancellationToken ct = default);
    Task<EntityExtractionResult> ExtractEntitiesAsync(string message, CancellationToken ct = default);
    Task<string> GenerateResponseAsync(string prompt, string context, CancellationToken ct = default);
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}

public record IntentResult(string Intent, double Confidence, string? SubIntent);
public record EntityExtractionResult(string? Product, string? Category, string? Brand, string? Color, string? Size, int? Quantity, string? Address, string? Neighborhood, string? PaymentMethod, decimal? MinPrice, decimal? MaxPrice);
