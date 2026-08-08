# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0-alpha.1] - 2026-08-08

### Added

- Experimental clients and strongly typed contracts for Catalog, Product API V2, Inventory & Price, Orders / Shipment Packages, Returns, Customer Questions, Invoices, and Webhooks.
- Page, cursor, batch-result, multipart claim attachment, and invoice-file upload support.
- Dependency-injection registrations for every feature interface.
- Opt-in, read-only Trendyol Stage smoke test and comprehensive request-contract unit tests.

### Changed

- Successful API responses with an empty body are now handled without attempting JSON deserialization.
- Package version advanced to `0.2.0-alpha.1`.

Pre-1.0 breaking changes will be documented here even when Semantic Versioning permits them in a minor release.

[0.2.0-alpha.1]: https://github.com/YunusOzdemirr/Trendyol.SDK/compare/v0.1.0-alpha.1...v0.2.0-alpha.1
