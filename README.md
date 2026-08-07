# Trendyol.Sdk

`Trendyol.Sdk` is an open-source .NET SDK for integrating with the Trendyol Türkiye Marketplace API. The project aims to provide a strongly typed, asynchronous, maintainable, and developer-friendly alternative to working with the raw HTTP API.

> [!IMPORTANT]
> This is an independent, community-maintained project and is not affiliated with, endorsed by, or maintained by Trendyol.

## Project status

The project is in pre-alpha development. Its package and public API are not yet available on NuGet and may change before version 1.0.0. Brand lookup by name is available experimentally; no other Trendyol API operation is currently supported.

## Goals

- A small and intentional public API
- Strongly typed request and response contracts
- Asynchronous I/O with cancellation support
- Safe credential handling and useful API-specific errors
- `HttpClient` and `IHttpClientFactory` support
- Optional Microsoft.Extensions.DependencyInjection and logging integration
- Unit-tested HTTP behavior without calls to Trendyol servers
- Endpoint contracts sourced exclusively from the official Trendyol documentation

## Installation

The package has not been published to NuGet. Installation instructions will be added with the first public prerelease. Contributors can generate the current `0.1.0-alpha.1` package locally with `dotnet pack --configuration Release`.

## Quick start

```csharp
using Trendyol.Sdk;
using Trendyol.Sdk.Configuration;

var options = new TrendyolOptions
{
    SellerId = 123456,
    ApiKey = "your-api-key",
    ApiSecret = "your-api-secret",
};

using var client = new TrendyolClient(options);

var brands = await client.Catalog.SearchBrandsByNameAsync(
    "TRENDYOLMİLLA",
    CancellationToken.None);
```

See the [brand-search documentation](docs/catalog/brands.md) for the verified wire contract, dependency-injection usage, and error behavior. Never commit credentials to source control.

## Supported APIs

| API | Status |
|---|---|
| Catalog — brand lookup by name | Experimental |
| Catalog — categories and attributes | Planned |
| Product API V2 | Planned |
| Orders / Shipment Packages V2 | Planned |
| Inventory & Price | Planned |
| Returns | Planned |
| Customer Questions | Planned |
| Invoices | Planned |
| Webhooks | Planned |

Status values used by this project are `Planned`, `In Progress`, `Supported`, `Experimental`, and `Deprecated`.

## Roadmap

1. Validate brand lookup by name and stabilize the Catalog client conventions.
2. Add category and category-attribute operations from verified official contracts.
3. Add Product V2 and inventory/price operations.
4. Add Order V2 and cursor-based shipment-package synchronization.
5. Expand into returns, questions, invoices, and webhooks.
6. Stabilize the public API and publish a 1.0 release.

The architectural baseline and documentation research are available under [`docs/architecture`](docs/architecture).

## Contributing

Contributions and design feedback are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request. Endpoint contributions must cite the applicable official Trendyol documentation and include tests.

## License

Trendyol.Sdk is licensed under the [MIT License](LICENSE).
