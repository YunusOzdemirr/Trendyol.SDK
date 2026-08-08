#pragma warning disable CS1591

namespace Trendyol.Sdk.Webhooks;

public sealed class WebhookRequest
{
    public string Url { get; set; } = string.Empty;
    public string AuthenticationType { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public List<string> SubscribedStatuses { get; set; } = [];
}

public sealed class Webhook
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string AuthenticationType { get; set; } = string.Empty;
    public string? Username { get; set; }
    public List<string>? SubscribedStatuses { get; set; }
    public string? Status { get; set; }
    public long? CreatedDate { get; set; }
    public long? LastModifiedDate { get; set; }
}
