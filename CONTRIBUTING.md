# Contributing to Trendyol.Sdk

Thank you for helping build a reliable community SDK.

## Before contributing

- Discuss significant public API or architectural changes before implementing them.
- Use the official [Trendyol Developer documentation](https://developers.trendyol.com/) as the source of truth.
- Do not use deprecated endpoints when a current version exists.
- Record documentation ambiguity instead of guessing a contract.
- Never commit API keys, API secrets, seller data, customer data, or production payloads.

## Development requirements

- .NET SDK 10.0.302 or a compatible patch selected by `global.json`
- Git

Run the complete local validation sequence from the repository root:

```shell
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
dotnet pack --configuration Release --no-build
```

## Endpoint contribution checklist

Every endpoint contribution must:

1. Link to the official documentation and identify the current API version.
2. Record the HTTP method, route, headers, parameters, and schemas.
3. Keep transport details internal and expose a deliberately designed typed API.
4. Support asynchronous execution and a final `CancellationToken` parameter.
5. Add unit tests for the route, query string, headers, serialization, success response, error response, and cancellation.
6. Add or update user documentation and the README support matrix.
7. Avoid relying on live Trendyol servers in unit tests.

## Code style

The repository uses nullable reference types, file-scoped namespaces, C# 14, analyzers, XML documentation for public APIs, and warnings as errors. Prefer small feature-focused types and keep implementation details internal.

## Integration tests

Integration tests are opt-in and must use the Trendyol stage environment. Credentials must come from environment variables or an external secret store. Tests must skip safely when credentials are absent.

## Changelog

Add user-visible changes to the `Unreleased` section of [CHANGELOG.md](CHANGELOG.md). Breaking pre-1.0 changes must still be called out explicitly.
Release 0.1.0-alpha
