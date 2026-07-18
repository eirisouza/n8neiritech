using n8neiritech.Domain.Enums;

namespace n8neiritech.Infrastructure.Options;

public class WhatsAppOptions
{
    public string Provider { get; set; } = WhatsAppProviderType.Fake.ToString();
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
