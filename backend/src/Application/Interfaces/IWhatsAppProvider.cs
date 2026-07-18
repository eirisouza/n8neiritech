namespace n8neiritech.Application.Interfaces;

public interface IWhatsAppProvider
{
    Task<SendMessageResult> SendTextAsync(string instanceName, string to, string text, string? quotedMessageId = null, CancellationToken ct = default);
    Task<SendMessageResult> SendImageAsync(string instanceName, string to, string imageUrl, string? caption = null, CancellationToken ct = default);
    Task<SendMessageResult> SendDocumentAsync(string instanceName, string to, string documentUrl, string? fileName = null, CancellationToken ct = default);
    Task<SendMessageResult> SendAudioAsync(string instanceName, string to, string audioUrl, CancellationToken ct = default);
    Task<SendMessageResult> SendButtonsAsync(string instanceName, string to, string text, IEnumerable<MessageButton> buttons, CancellationToken ct = default);
    Task<ProviderStatus> GetStatusAsync(string instanceName, CancellationToken ct = default);
    Task<QrCodeResult> GetQrCodeAsync(string instanceName, CancellationToken ct = default);
}

public record SendMessageResult(bool Success, string? MessageId, string? Error);
public record ProviderStatus(bool Connected, string? Phone, string Status);
public record QrCodeResult(bool Success, string? QrCode, string? Error);
public record MessageButton(string Id, string Text);
