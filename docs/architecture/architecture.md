# Trendyol.Sdk architecture

## Scope and goals

The initial package targets the Trendyol Türkiye Marketplace API. It is a reusable SDK, not an application framework: it has no domain/application/infrastructure layering, no service locator, and no global mutable state.

The package must keep transport details internal, provide asynchronous operations with cancellation, minimize dependencies, and treat every public type as a long-lived compatibility commitment. International Marketplace APIs are a future concern with distinct contracts; domestic code must not hard-code assumptions into reusable primitives that would prevent a separate international module or package.

## Repository structure

```text
/
├── src/
│   └── Trendyol.Sdk/
│       ├── Configuration/
│       ├── DependencyInjection/
│       ├── Exceptions/
│       └── Internal/
│           ├── Http/
│           └── Serialization/
├── tests/
│   ├── Trendyol.Sdk.UnitTests/
│   └── Trendyol.Sdk.IntegrationTests/
├── docs/
│   └── architecture/
├── .github/workflows/
├── Directory.Build.props
├── Directory.Packages.props
└── Trendyol.Sdk.slnx
```

Feature folders will be added only with real APIs. Each feature owns its public contracts and internal implementation. Likely boundaries are `Catalog`, `Products`, `Orders`, `Inventory`, `Returns`, `Questions`, `Invoices`, and `Webhooks`. A global `Models` folder and a monolithic endpoint client are deliberately avoided.

Samples are deferred until the SDK has a real supported operation.

## Public client design

`TrendyolClient` is a concrete facade. There is no `ITrendyolClient`: adding a member to a public interface breaks consumers that implement it, while adding a property to a concrete facade is additive.

When a feature becomes real, it exposes a focused interface such as `ICatalogClient` or `IOrdersClient`. The facade exposes implemented features as properties, and dependency-injection consumers can inject a feature interface directly. This keeps test doubles narrow and prevents the facade from becoming a giant method container.

The direct constructor owns one `HttpClient` for the facade lifetime and the facade implements `IDisposable`. The DI path is backed by `IHttpClientFactory`, which owns the underlying handler lifetime. No request creates and disposes its own `HttpClient`.

The foundation intentionally exposes no placeholder feature property or empty feature interface.

## Configuration and credentials

`TrendyolOptions` contains the seller ID, API key, API secret, integrator name, environment, and timeout. Production and stage are explicit environments whose hosts are resolved internally. An arbitrary base address is not public because it could forward credentials to an unintended server.

Validation occurs when a direct client is constructed and when a DI client is resolved. A validated immutable snapshot is used internally, preventing later mutation of the source options object from changing authentication or routing.

Credential-bearing properties are excluded from debugger display and never included in `ToString`. Transport errors are defensively redacted against the configured API key and secret. Credentials, authorization headers, and full request or response bodies are never logged.

## HTTP pipeline

The internal transport owns these cross-cutting responsibilities:

1. Resolve the official base host from the configured environment.
2. Build a relative endpoint URI supplied by a feature client.
3. Add Basic authentication and the documented User-Agent to each request.
4. Serialize request bodies and deserialize successful JSON responses using centralized settings.
5. Map non-success responses to SDK exceptions.
6. Emit safe structured diagnostics.

Feature clients will provide HTTP method, relative route, route template for logging, request body, and expected response type. They will not construct authentication headers or expose `HttpRequestMessage` to consumers.

No automatic retry or rate limiter is present. Future resilience must be operation-aware: read operations may be retry candidates, while writes require documented idempotency or an explicit idempotency mechanism. Returning `IHttpClientBuilder` from DI registration also lets an application add a deliberate handler pipeline without the SDK taking an unsafe default.

## Dependency injection and lifetime

`AddTrendyol(IServiceCollection, Action<TrendyolOptions>)` follows Microsoft.Extensions conventions and returns `IHttpClientBuilder`. It registers:

- validated options;
- the factory-managed `HttpClient` configured with the official host and timeout;
- the concrete `TrendyolClient` facade;
- future feature interfaces as their modules are implemented.

The facade is transient when resolved through the typed-client registration. Connection pooling remains efficient because `IHttpClientFactory` manages handlers independently of facade instances.

## Errors

`TrendyolApiException` represents an HTTP response that the Trendyol API rejected. It preserves HTTP status and parsed structured errors without exposing the raw body.

Two subclasses have distinct actionable meaning:

- `TrendyolAuthenticationException` for HTTP 401;
- `TrendyolRateLimitException` for HTTP 429, including a parsed standard `Retry-After` value when present.

HTTP 400 is not assigned a special exception until the official API demonstrates a consistent validation envelope. Network errors remain `HttpRequestException`. Caller cancellation and `HttpClient` timeout behavior retain the platform cancellation exception types.

The parser recognizes official examples containing `errors[]`, `exception`, or `message`, ignores unknown fields, and falls back to the HTTP reason phrase. It redacts credentials before constructing errors or exception messages.

## Serialization and contract conventions

The package uses `System.Text.Json` with centralized read-only options:

- camel-case property names for writes;
- case-insensitive property matching for reads;
- omission of null request properties;
- strict number handling;
- tolerance of unknown JSON properties;
- no global string-enum converter.

Public contracts will be sealed classes unless another representation has a concrete benefit. Records are avoided for credential- or customer-bearing types because their generated value equality and `ToString` are usually undesirable.

Request-only values with documented closed sets can use enums. Evolving response values remain strings or become explicit extensible value types. Public date/time values use `DateTimeOffset`, with endpoint-specific epoch conversion. Currency and monetary amounts use `decimal`.

Page, cursor, and batch abstractions will be introduced only when their actual wire contracts are implemented. Source-generated JSON metadata is deferred until the contract set and performance needs justify its maintenance cost.

## Logging

Logging is optional and uses `Microsoft.Extensions.Logging`. Internal precompiled message delegates avoid interpolation on disabled log levels. Permitted fields are operation name, route template, HTTP status, and elapsed time. Actual URLs, query values, headers, bodies, order numbers, customer data, API keys, and secrets are excluded.

## Testing strategy

Unit tests do not call Trendyol. A hand-written `HttpMessageHandler` records requests and returns controlled responses. Tests cover option validation, credential safety, host selection, authentication, User-Agent, serialization, error mapping, `Retry-After`, cancellation, DI registration, and client reuse.

Integration tests are a separate opt-in project. Once endpoints exist, they will use only the stage environment, load credentials from environment variables or an external secret store, and skip when credentials are absent.

CI restores, builds, tests, and packs in Release mode. Packing is part of every pull request so metadata, cross-target compatibility, Source Link, XML documentation, and symbol generation cannot silently regress.

## Target frameworks and dependencies

The package targets `netstandard2.0` and `net10.0`:

- `netstandard2.0` gives broad reach across modern .NET and supported .NET Framework consumers.
- `net10.0` provides a current LTS-specific asset and APIs through November 2028.
- A separate `net8.0` asset is not maintained because .NET 8 reaches end of support in November 2026; .NET 8 consumers can use the `netstandard2.0` asset.

Runtime dependencies are limited to `System.Text.Json`, `Microsoft.Extensions.Http`, and `Microsoft.Extensions.Logging.Abstractions`. Source Link is private to the build. No resilience, HTTP abstraction, JSON alternative, mocking, or assertion package is introduced.

## Versioning and compatibility

The initial package version is `0.1.0`. Semantic Versioning is used. Breaking changes before 1.0 remain explicitly documented in the changelog rather than treated as inconsequential.

Public types are added only when they provide consumer value. Implementation types, HTTP primitives, serializer settings, and URI construction remain internal. Version-specific namespaces will be used only when Trendyol exposes incompatible generations that the SDK must support simultaneously; Product V1 is not implemented.

## Architectural Decisions

### Türkiye Marketplace first

**Decision:** The initial package supports only Türkiye Marketplace.

**Reasoning:** International documentation introduces storefront headers and distinct contracts. Combining them now would enlarge configuration and public models before a use case exists. Shared infrastructure remains neutral enough to support a later module or companion package.

### Concrete facade, feature interfaces

**Decision:** Expose `TrendyolClient` but not `ITrendyolClient`; expose interfaces for real feature clients.

**Alternative:** A main client interface is convenient for constructor injection, but it becomes a breaking-change trap whenever a feature property is added.

### One package initially

**Decision:** Include Microsoft DI and logging support in `Trendyol.Sdk` for the pre-1.0 phase.

**Alternative:** A separate `Trendyol.Sdk.Extensions.DependencyInjection` package reduces dependencies for standalone consumers but adds publication and versioning overhead. Re-evaluate before 1.0 using real package-size and adoption data.

### No arbitrary base URL

**Decision:** Expose an environment enum, not a public base-address override.

**Reasoning:** Authentication is attached automatically. Redirecting it to arbitrary hosts is a security-sensitive operation. Internal injection keeps tests deterministic without expanding the public risk surface.

### No automatic resilience yet

**Decision:** Surface rate-limit and transient failures without retrying.

**Reasoning:** Trendyol limits differ by service and tier, and write idempotency is not generally documented. A generic retry policy could duplicate a business operation.

### Endpoint-specific evolution types

**Decision:** Do not publish universal pagination, batch, enum, or date abstractions in the foundation.

**Reasoning:** The official API demonstrates multiple incompatible pagination and response conventions. Types will emerge from verified operations rather than anticipated uniformity.

### First API candidate

**Decision:** After foundation review, implement brand lookup by name in a catalog feature.

**Reasoning:** It is a current V1/V2-shared GET and validates the complete foundational pipeline without depending on Product V1 or the ambiguous paged brand-list rules.
