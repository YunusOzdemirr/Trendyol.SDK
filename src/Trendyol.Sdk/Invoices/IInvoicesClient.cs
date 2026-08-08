#pragma warning disable CS1591

namespace Trendyol.Sdk.Invoices;

public interface IInvoicesClient
{
    public Task SendInvoiceLinkAsync(SendInvoiceLinkRequest request, CancellationToken cancellationToken = default);
    public Task DeleteInvoiceLinkAsync(DeleteInvoiceLinkRequest request, CancellationToken cancellationToken = default);
    public Task UploadInvoiceFileAsync(UploadInvoiceFileRequest request, CancellationToken cancellationToken = default);
}
