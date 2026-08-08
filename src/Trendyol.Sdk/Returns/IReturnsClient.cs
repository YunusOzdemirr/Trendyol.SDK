#pragma warning disable CS1591

namespace Trendyol.Sdk.Returns;

public interface IReturnsClient
{
    public Task<ClaimPage> GetClaimsAsync(ClaimFilter? filter = null, CancellationToken cancellationToken = default);
    public Task CreateClaimAsync(CreateClaimRequest request, CancellationToken cancellationToken = default);
    public Task ApproveClaimItemsAsync(string claimId, ApproveClaimItemsRequest request, CancellationToken cancellationToken = default);
    public Task CreateClaimIssueAsync(string claimId, ClaimIssueRequest request, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<ClaimIssueReason>> GetClaimIssueReasonsAsync(CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<ClaimItemAudit>> GetClaimItemAuditsAsync(string claimItemId, CancellationToken cancellationToken = default);
}
