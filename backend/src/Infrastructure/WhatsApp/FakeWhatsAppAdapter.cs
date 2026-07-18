using Microsoft.Extensions.Logging;
using n8neiritech.Application.Interfaces;

namespace n8neiritech.Infrastructure.WhatsApp;

public class FakeWhatsAppAdapter : IWhatsAppProvider
{
    private readonly ILogger<FakeWhatsAppAdapter> _logger;

    public FakeWhatsAppAdapter(ILogger<FakeWhatsAppAdapter> logger)
    {
        _logger = logger;
    }

    public Task<SendMessageResult> SendTextAsync(string instanceName, string to, string text, string? quotedMessageId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("[FAKE-WHATSAPP] Text to {To} on {Instance}: {Text}", Sanitize(to), Sanitize(instanceName), Sanitize(text));
        return Task.FromResult(new SendMessageResult(true, Guid.NewGuid().ToString("N"), null));
    }

    public Task<SendMessageResult> SendImageAsync(string instanceName, string to, string imageUrl, string? caption = null, CancellationToken ct = default)
    {
        _logger.LogInformation("[FAKE-WHATSAPP] Image to {To} on {Instance}: {Url} {Caption}", Sanitize(to), Sanitize(instanceName), Sanitize(imageUrl), Sanitize(caption));
        return Task.FromResult(new SendMessageResult(true, Guid.NewGuid().ToString("N"), null));
    }

    public Task<SendMessageResult> SendDocumentAsync(string instanceName, string to, string documentUrl, string? fileName = null, CancellationToken ct = default)
    {
        _logger.LogInformation("[FAKE-WHATSAPP] Document to {To} on {Instance}: {Url} {FileName}", Sanitize(to), Sanitize(instanceName), Sanitize(documentUrl), Sanitize(fileName));
        return Task.FromResult(new SendMessageResult(true, Guid.NewGuid().ToString("N"), null));
    }

    public Task<SendMessageResult> SendAudioAsync(string instanceName, string to, string audioUrl, CancellationToken ct = default)
    {
        _logger.LogInformation("[FAKE-WHATSAPP] Audio to {To} on {Instance}: {Url}", Sanitize(to), Sanitize(instanceName), Sanitize(audioUrl));
        return Task.FromResult(new SendMessageResult(true, Guid.NewGuid().ToString("N"), null));
    }

    public Task<SendMessageResult> SendButtonsAsync(string instanceName, string to, string text, IEnumerable<MessageButton> buttons, CancellationToken ct = default)
    {
        _logger.LogInformation("[FAKE-WHATSAPP] Buttons to {To} on {Instance}: {Text} ({Count} buttons)", Sanitize(to), Sanitize(instanceName), Sanitize(text), buttons.Count());
        return Task.FromResult(new SendMessageResult(true, Guid.NewGuid().ToString("N"), null));
    }

    public Task<ProviderStatus> GetStatusAsync(string instanceName, CancellationToken ct = default)
        => Task.FromResult(new ProviderStatus(true, "+5511999999999", "connected"));

    public Task<QrCodeResult> GetQrCodeAsync(string instanceName, CancellationToken ct = default)
        => Task.FromResult(new QrCodeResult(true, Convert.ToBase64String(Guid.NewGuid().ToByteArray()), null));

    private static string? Sanitize(string? value) => value?.Replace("\r", string.Empty).Replace("\n", string.Empty);
}
