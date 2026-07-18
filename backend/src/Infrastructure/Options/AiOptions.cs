namespace n8neiritech.Infrastructure.Options;

public class AiOptions
{
    public string Provider { get; set; } = "None";
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
}
