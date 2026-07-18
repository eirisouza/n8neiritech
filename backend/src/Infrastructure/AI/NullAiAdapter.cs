using System.Text.RegularExpressions;
using n8neiritech.Application.Interfaces;

namespace n8neiritech.Infrastructure.AI;

public class NullAiAdapter : IAiProvider
{
    public Task<IntentResult> DetectIntentAsync(string message, string? context = null, CancellationToken ct = default)
    {
        var text = message.ToLowerInvariant();
        var intent = text.Contains("pedido") || text.Contains("comprar") ? "order" :
                     text.Contains("entrega") ? "delivery" :
                     text.Contains("pagamento") ? "payment" :
                     text.Contains("atendente") || text.Contains("humano") ? "handoff" : "general";
        return Task.FromResult(new IntentResult(intent, 0.55, null));
    }

    public Task<EntityExtractionResult> ExtractEntitiesAsync(string message, CancellationToken ct = default)
    {
        var quantity = Regex.Match(message, @"(?<!\d)(\d{1,3})(?!\d)");
        var priceMatches = Regex.Matches(message, @"\d+[\.,]?\d{0,2}").Select(x => decimal.TryParse(x.Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : (decimal?)null).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        return Task.FromResult(new EntityExtractionResult(
            Product: null,
            Category: null,
            Brand: null,
            Color: null,
            Size: null,
            Quantity: quantity.Success ? int.Parse(quantity.Value) : null,
            Address: message.Contains("rua", StringComparison.OrdinalIgnoreCase) ? message : null,
            Neighborhood: null,
            PaymentMethod: message.Contains("pix", StringComparison.OrdinalIgnoreCase) ? "Pix" : null,
            MinPrice: priceMatches.FirstOrDefault(),
            MaxPrice: priceMatches.Length > 1 ? priceMatches.Last() : null));
    }

    public Task<string> GenerateResponseAsync(string prompt, string context, CancellationToken ct = default)
    {
        var text = context.ToLowerInvariant();
        var response = text.Contains("horário")
            ? "Nosso horário padrão é de segunda a sábado das 8h às 20h e domingo das 9h às 13h."
            : text.Contains("entrega")
                ? "Fazemos entrega na região e também retirada na loja."
                : text.Contains("pagamento")
                    ? "Aceitamos Pix, cartão, dinheiro e pagamento na entrega."
                    : "Olá! Posso ajudar com catálogo, preços, disponibilidade, pedido e entrega.";
        return Task.FromResult(response);
    }

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
        => Task.FromResult(Array.Empty<float>());
}
