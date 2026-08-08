# Trendyol.Sdk

`Trendyol.Sdk` is an open-source .NET SDK for integrating with the Trendyol Türkiye Marketplace API. The project aims to provide a strongly typed, asynchronous, maintainable, and developer-friendly alternative to working with the raw HTTP API.

> [!IMPORTANT]
> This is an independent, community-maintained project and is not affiliated with, endorsed by, or maintained by Trendyol.

## Project status

The project is in alpha development. Version `0.2.0-alpha.1` exposes the current Türkiye Marketplace API families experimentally. Public contracts may change before version 1.0.0.

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

Install the latest prerelease from NuGet:

```shell
dotnet add package Trendyol.Sdk --prerelease
```

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

Feature clients are available through `client.Catalog`, `client.Products`, `client.Inventory`, `client.Orders`, `client.Returns`, `client.Questions`, `client.Invoices`, and `client.Webhooks`. See the [feature guide](docs/features.md) and [API coverage matrix](docs/api-coverage.md). Never commit credentials to source control.

## Supported APIs

| API | Status |
|---|---|
| Catalog — brands, categories, and attributes | Experimental |
| Product API V2 | Experimental |
| Orders / Shipment Packages | Experimental |
| Inventory & Price | Experimental |
| Returns | Experimental |
| Customer Questions | Experimental |
| Invoices | Experimental |
| Webhooks | Experimental |

Status values used by this project are `Planned`, `In Progress`, `Supported`, `Experimental`, and `Deprecated`.

## Roadmap

1. Validate the experimental clients against opt-in Stage scenarios.
2. Refine response contracts from real-world payloads while preserving forward compatibility.
3. Stabilize naming, validation, and pagination conventions.
4. Graduate verified API families from Experimental to Supported.
5. Stabilize the public API and publish a 1.0 release.

The architectural baseline and documentation research are available under [`docs/architecture`](docs/architecture).

## Contributing

Contributions and design feedback are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request. Endpoint contributions must cite the applicable official Trendyol documentation and include tests.

## License

Trendyol.Sdk is licensed under the [MIT License](LICENSE).
