#pragma warning disable CS1591

namespace Trendyol.Sdk.Invoices;

public sealed class SendInvoiceLinkRequest
{
    public long ShipmentPackageId { get; set; }
    public string InvoiceLink { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public long? InvoiceDateTime { get; set; }
}

public sealed class DeleteInvoiceLinkRequest
{
    public long ShipmentPackageId { get; set; }
}

public sealed class UploadInvoiceFileRequest
{
    public long ShipmentPackageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = "application/pdf";
    public long? InvoiceDateTime { get; set; }
    public string? InvoiceNumber { get; set; }
}
