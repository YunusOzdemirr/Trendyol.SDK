using System.Text.Json;
using Trendyol.Sdk.Internal;

namespace Trendyol.Sdk.Webhooks;

internal sealed class WebhooksClient : IWebhooksClient
{
    private readonly TrendyolClient _client;
    internal WebhooksClient(TrendyolClient client) => _client = client;

    public Task<Webhook> CreateAsync(WebhookRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        return RequiredAsync<Webhook>("Webhooks.Create", HttpMethod.Post, SellerRoute("webhooks"),
            "integration/sellers/{sellerId}/webhooks", request, cancellationToken);
    }

    public async Task<IReadOnlyList<Webhook>> GetAsync(CancellationToken cancellationToken = default)
    {
        return await RequiredAsync<List<Webhook>>("Webhooks.Get", HttpMethod.Get, SellerRoute("webhooks"),
            "integration/sellers/{sellerId}/webhooks", null, cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateAsync(string id, WebhookRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotEmpty(id, nameof(id));
        Validate(request);
        return _client.SendAsync("Webhooks.Update", HttpMethod.Put, SellerRoute($"webhooks/{Uri.EscapeDataString(id)}"),
            "integration/sellers/{sellerId}/webhooks/{Id}", request, cancellationToken);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default) => MutateAsync("Delete", id, string.Empty, HttpMethod.Delete, cancellationToken);
    public Task ActivateAsync(string id, CancellationToken cancellationToken = default) => MutateAsync("Activate", id, "activate", HttpMethod.Put, cancellationToken);
    public Task DeactivateAsync(string id, CancellationToken cancellationToken = default) => MutateAsync("Deactivate", id, "deactivate", HttpMethod.Put, cancellationToken);

    private Task MutateAsync(string operation, string id, string action, HttpMethod method, CancellationToken token)
    {
        TrendyolGuard.NotEmpty(id, nameof(id));
        var suffix = string.IsNullOrEmpty(action) ? string.Empty : $"/{action}";
        return _client.SendAsync($"Webhooks.{operation}", method, SellerRoute($"webhooks/{Uri.EscapeDataString(id)}{suffix}"),
            $"integration/sellers/{{sellerId}}/webhooks/{{Id}}{suffix}", null, token);
    }

    private static void Validate(WebhookRequest request)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.NotEmpty(request.Url, nameof(request.Url));
        TrendyolGuard.NotEmpty(request.AuthenticationType, nameof(request.AuthenticationType));
        if (request.SubscribedStatuses.Count > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "At most 100 subscribed statuses are accepted.");
        }
    }

    private string SellerRoute(string suffix) => $"integration/sellers/{_client.SellerId}/{suffix}";
    private async Task<T> RequiredAsync<T>(string operation, HttpMethod method, string uri, string template, object? request, CancellationToken token) =>
        await _client.SendAsync<T>(operation, method, uri, template, request, token).ConfigureAwait(false)
        ?? throw new JsonException($"Trendyol returned an empty {operation} response.");
}
