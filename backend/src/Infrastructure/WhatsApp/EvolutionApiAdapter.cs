using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using n8neiritech.Application.Interfaces;
using n8neiritech.Infrastructure.Options;

namespace n8neiritech.Infrastructure.WhatsApp;

public class EvolutionApiAdapter : IWhatsAppProvider
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<EvolutionApiAdapter> _logger;

    public EvolutionApiAdapter(HttpClient httpClient, IOptions<WhatsAppOptions> options, ILogger<EvolutionApiAdapter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
        }
    }

    public Task<SendMessageResult> SendTextAsync(string instanceName, string to, string text, string? quotedMessageId = null, CancellationToken ct = default)
        => SendAsync($"/message/sendText/{instanceName}", new { number = to, text, quoted = quotedMessageId }, ct);

    public Task<SendMessageResult> SendImageAsync(string instanceName, string to, string imageUrl, string? caption = null, CancellationToken ct = default)
        => SendAsync($"/message/sendMedia/{instanceName}", new { number = to, mediatype = "image", media = imageUrl, caption }, ct);

    public Task<SendMessageResult> SendDocumentAsync(string instanceName, string to, string documentUrl, string? fileName = null, CancellationToken ct = default)
        => SendAsync($"/message/sendMedia/{instanceName}", new { number = to, mediatype = "document", media = documentUrl, fileName }, ct);

    public Task<SendMessageResult> SendAudioAsync(string instanceName, string to, string audioUrl, CancellationToken ct = default)
        => SendAsync($"/message/sendMedia/{instanceName}", new { number = to, mediatype = "audio", media = audioUrl }, ct);

    public Task<SendMessageResult> SendButtonsAsync(string instanceName, string to, string text, IEnumerable<MessageButton> buttons, CancellationToken ct = default)
        => SendAsync($"/message/sendButtons/{instanceName}", new { number = to, text, buttons = buttons.Select(x => new { buttonId = x.Id, buttonText = x.Text }) }, ct);

    public async Task<ProviderStatus> GetStatusAsync(string instanceName, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/instance/fetchInstances", ct);
        if (!response.IsSuccessStatusCode)
        {
            return new ProviderStatus(false, null, $"HTTP {(int)response.StatusCode}");
        }

        var instances = await response.Content.ReadFromJsonAsync<List<EvolutionInstanceDto>>(cancellationToken: ct) ?? [];
        var instance = instances.FirstOrDefault(x => string.Equals(x.Name, instanceName, StringComparison.OrdinalIgnoreCase));
        return instance is null
            ? new ProviderStatus(false, null, "not_found")
            : new ProviderStatus(instance.ConnectionStatus.Equals("open", StringComparison.OrdinalIgnoreCase), instance.Number, instance.ConnectionStatus);
    }

    public async Task<QrCodeResult> GetQrCodeAsync(string instanceName, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/instance/connect/{instanceName}", ct);
        if (!response.IsSuccessStatusCode)
        {
            return new QrCodeResult(false, null, $"HTTP {(int)response.StatusCode}");
        }

        var payload = await response.Content.ReadFromJsonAsync<EvolutionQrCodeDto>(cancellationToken: ct);
        return new QrCodeResult(true, payload?.Base64 ?? payload?.Code, null);
    }

    private async Task<SendMessageResult> SendAsync(string endpoint, object payload, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                return new SendMessageResult(false, null, error);
            }

            var content = await response.Content.ReadFromJsonAsync<EvolutionSendResult>(cancellationToken: ct);
            return new SendMessageResult(true, content?.Key?.Id ?? Guid.NewGuid().ToString("N"), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message through Evolution API");
            return new SendMessageResult(false, null, ex.Message);
        }
    }

    private sealed record EvolutionSendResult(EvolutionKey? Key);
    private sealed record EvolutionKey(string? Id);
    private sealed record EvolutionInstanceDto(string Name, string ConnectionStatus, string? Number);
    private sealed record EvolutionQrCodeDto(string? Base64, string? Code);
}
