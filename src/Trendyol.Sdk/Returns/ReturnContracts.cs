#pragma warning disable CS1591

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trendyol.Sdk.Returns;

public sealed class ClaimFilter
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? OrderByField { get; set; }
    public string? OrderByDirection { get; set; }
    public IReadOnlyCollection<long>? ShipmentPackageIds { get; set; }
    public IReadOnlyCollection<string>? ClaimIds { get; set; }
    public string? ClaimItemStatus { get; set; }
}

public sealed class ClaimPage
{
    public long TotalElements { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public List<Claim> Content { get; set; } = [];
}

public sealed class Claim
{
    public string Id { get; set; } = string.Empty;
    public long? OrderNumber { get; set; }
    public long? ShipmentPackageId { get; set; }
    public List<ClaimItem> Items { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class ClaimItem
{
    public string Id { get; set; } = string.Empty;
    public long? OrderLineId { get; set; }
    public string? Status { get; set; }
    public string? CustomerClaimItemReason { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class CreateClaimRequest
{
    public List<CreateClaimItem> Items { get; set; } = [];
}

public sealed class CreateClaimItem
{
    public long ShipmentPackageId { get; set; }
    public long OrderLineId { get; set; }
    public int Quantity { get; set; }
    public long ClaimReasonId { get; set; }
}

public sealed class ApproveClaimItemsRequest
{
    public List<string> ClaimLineItemIds { get; set; } = [];
}

public sealed class ClaimIssueRequest
{
    public long ClaimIssueReasonId { get; set; }
    public List<string> ClaimItemIds { get; set; } = [];
    public string? Description { get; set; }
    public List<ClaimIssueAttachment> Attachments { get; set; } = [];
}

public sealed class ClaimIssueAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = "application/octet-stream";
}

public sealed class ClaimIssueReason
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class ClaimItemAudit
{
    public string? Status { get; set; }
    public long? Date { get; set; }
    public string? UserName { get; set; }
    public string? Note { get; set; }
}
