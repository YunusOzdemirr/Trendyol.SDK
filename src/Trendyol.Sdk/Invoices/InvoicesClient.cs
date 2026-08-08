using System.Globalization;
using System.Net.Http.Headers;
using Trendyol.Sdk.Internal;

namespace Trendyol.Sdk.Invoices;

internal sealed class InvoicesClient : IInvoicesClient
{
    private readonly TrendyolClient _client;
    internal InvoicesClient(TrendyolClient client) => _client = client;

    public Task SendInvoiceLinkAsync(SendInvoiceLinkRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Positive(request.ShipmentPackageId, nameof(request.ShipmentPackageId));
        TrendyolGuard.NotEmpty(request.InvoiceLink, nameof(request.InvoiceLink));
        return _client.SendAsync("Invoices.SendInvoiceLink", HttpMethod.Post, SellerRoute("seller-invoice-links"),
            "integration/sellers/{sellerId}/seller-invoice-links", request, cancellationToken);
    }

    public Task DeleteInvoiceLinkAsync(DeleteInvoiceLinkRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Positive(request.ShipmentPackageId, nameof(request.ShipmentPackageId));
        return _client.SendAsync("Invoices.DeleteInvoiceLink", HttpMethod.Post, SellerRoute("seller-invoice-links/delete"),
            "integration/sellers/{sellerId}/seller-invoice-links/delete", request, cancellationToken);
    }

    public async Task UploadInvoiceFileAsync(UploadInvoiceFileRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Positive(request.ShipmentPackageId, nameof(request.ShipmentPackageId));
        TrendyolGuard.NotEmpty(request.FileName, nameof(request.FileName));
        if (request.Content.Length == 0)
        {
            throw new ArgumentException("Invoice content must not be empty.", nameof(request));
        }

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.ShipmentPackageId.ToString(CultureInfo.InvariantCulture)), "shipmentPackageId");
        if (request.InvoiceDateTime is not null)
        {
            content.Add(new StringContent(request.InvoiceDateTime.Value.ToString(CultureInfo.InvariantCulture)), "invoiceDateTime");
        }

        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            content.Add(new StringContent(request.InvoiceNumber), "invoiceNumber");
        }

        var file = new ByteArrayContent(request.Content);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
        content.Add(file, "file", request.FileName);
        await _client.SendContentAsync("Invoices.UploadInvoiceFile", HttpMethod.Post, SellerRoute("seller-invoice-file"),
            "integration/sellers/{sellerId}/seller-invoice-file", content, cancellationToken).ConfigureAwait(false);
    }

    private string SellerRoute(string suffix) => $"integration/sellers/{_client.SellerId}/{suffix}";
}
