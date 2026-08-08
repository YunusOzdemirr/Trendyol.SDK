# Feature clients

All feature clients in `0.2.0-alpha.1` are experimental. They share the configured authentication, environment, cancellation, error mapping, and safe logging behavior of `TrendyolClient`.

| Property | Interface | Responsibility |
|---|---|---|
| `Catalog` | `ICatalogClient` | Brands, category tree, category attributes, and values |
| `Products` | `IProductsClient` | Product V2 create/update/read/archive/delete, batch results, buybox, videos, and brand requests |
| `Inventory` | `IInventoryClient` | Price and inventory batch updates |
| `Orders` | `IOrdersClient` | Shipment-package page/stream reads and package lifecycle mutations |
| `Returns` | `IReturnsClient` | Claims, approvals, rejection issues with attachments, reasons, and audits |
| `Questions` | `IQuestionsClient` | Question filtering, details, and answers |
| `Invoices` | `IInvoicesClient` | Invoice links and multipart invoice files |
| `Webhooks` | `IWebhooksClient` | Create, list, update, delete, activate, and deactivate |

Feature interfaces can be injected directly after calling `services.AddTrendyol(...)`. Write operations are not retried automatically because the official API does not provide one universal idempotency guarantee. Batch operations return a `batchRequestId`; query the Product batch-result operation to inspect item-level outcomes.

Dates accepted by filter contracts use `DateTimeOffset` and are serialized to epoch milliseconds. Cursor values are treated as opaque and URL-encoded. Multipart operations accept byte arrays so the SDK does not assume ownership of caller streams.

See [API coverage](api-coverage.md) for the exact route-to-method mapping and the official documentation source.
