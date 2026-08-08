using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Trendyol.Sdk.Internal;
using Trendyol.Sdk.Internal.Http;

namespace Trendyol.Sdk.Returns;

internal sealed class ReturnsClient : IReturnsClient
{
    private readonly TrendyolClient _client;
    internal ReturnsClient(TrendyolClient client) => _client = client;

    public Task<ClaimPage> GetClaimsAsync(ClaimFilter? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new ClaimFilter();
        if (filter.Page < 0 || filter.Size is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(filter));
        }

        var uri = new TrendyolQuery(SellerRoute("claims"))
            .AddEpochMilliseconds("startDate", filter.StartDate).AddEpochMilliseconds("endDate", filter.EndDate)
            .Add("page", filter.Page).Add("size", filter.Size).Add("orderByField", filter.OrderByField)
            .Add("orderByDirection", filter.OrderByDirection).AddCsv("shipmentPackageIds", filter.ShipmentPackageIds)
            .AddCsv("claimIds", filter.ClaimIds).Add("claimItemStatus", filter.ClaimItemStatus).ToString();
        return RequiredAsync<ClaimPage>("Returns.GetClaims", HttpMethod.Get, uri, "integration/order/sellers/{sellerId}/claims", null, cancellationToken);
    }

    public Task CreateClaimAsync(CreateClaimRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Count(request.Items, nameof(request.Items), 1000);
        return _client.SendAsync("Returns.CreateClaim", HttpMethod.Post, SellerRoute("claims/create"),
            "integration/order/sellers/{sellerId}/claims/create", request, cancellationToken);
    }

    public Task ApproveClaimItemsAsync(string claimId, ApproveClaimItemsRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotEmpty(claimId, nameof(claimId));
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Count(request.ClaimLineItemIds, nameof(request.ClaimLineItemIds), 1000);
        return _client.SendAsync("Returns.ApproveClaimItems", HttpMethod.Put,
            SellerRoute($"claims/{Uri.EscapeDataString(claimId)}/items/approve"),
            "integration/order/sellers/{sellerId}/claims/{claimId}/items/approve", request, cancellationToken);
    }

    public async Task CreateClaimIssueAsync(string claimId, ClaimIssueRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotEmpty(claimId, nameof(claimId));
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Count(request.ClaimItemIds, nameof(request.ClaimItemIds), 1000);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.ClaimIssueReasonId.ToString(CultureInfo.InvariantCulture)), "claimIssueReasonId");
        var description = TrendyolGuard.NotEmpty(request.Description, nameof(request.Description));
        content.Add(new StringContent(string.Join(",", request.ClaimItemIds)), "claimItemIdList");
        content.Add(new StringContent(description), "description");

        foreach (var attachment in request.Attachments)
        {
            TrendyolGuard.NotEmpty(attachment.FileName, nameof(attachment.FileName));
            var file = new ByteArrayContent(attachment.Content);
            file.Headers.ContentType = MediaTypeHeaderValue.Parse(attachment.ContentType);
            content.Add(file, "files", attachment.FileName);
        }

        await _client.SendContentAsync("Returns.CreateClaimIssue", HttpMethod.Post,
            SellerRoute($"claims/{Uri.EscapeDataString(claimId)}/issue"),
            "integration/order/sellers/{sellerId}/claims/{claimId}/issue", content, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ClaimIssueReason>> GetClaimIssueReasonsAsync(CancellationToken cancellationToken = default) =>
        await RequiredAsync<List<ClaimIssueReason>>("Returns.GetClaimIssueReasons", HttpMethod.Get,
            "integration/order/claim-issue-reasons", "integration/order/claim-issue-reasons", null, cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<ClaimItemAudit>> GetClaimItemAuditsAsync(string claimItemId, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotEmpty(claimItemId, nameof(claimItemId));
        return await RequiredAsync<List<ClaimItemAudit>>("Returns.GetClaimItemAudits", HttpMethod.Get,
            SellerRoute($"claims/items/{Uri.EscapeDataString(claimItemId)}/audit"),
            "integration/order/sellers/{sellerId}/claims/items/{claimItemsId}/audit", null, cancellationToken).ConfigureAwait(false);
    }

    private string SellerRoute(string suffix) => $"integration/order/sellers/{_client.SellerId}/{suffix}";
    private async Task<T> RequiredAsync<T>(string operation, HttpMethod method, string uri, string template, object? request, CancellationToken token) =>
        await _client.SendAsync<T>(operation, method, uri, template, request, token).ConfigureAwait(false)
        ?? throw new JsonException($"Trendyol returned an empty {operation} response.");
}
