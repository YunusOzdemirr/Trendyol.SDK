#pragma warning disable CS1591

namespace Trendyol.Sdk.Webhooks;

public interface IWebhooksClient
{
    public Task<Webhook> CreateAsync(WebhookRequest request, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Webhook>> GetAsync(CancellationToken cancellationToken = default);
    public Task UpdateAsync(string id, WebhookRequest request, CancellationToken cancellationToken = default);
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    public Task ActivateAsync(string id, CancellationToken cancellationToken = default);
    public Task DeactivateAsync(string id, CancellationToken cancellationToken = default);
}
