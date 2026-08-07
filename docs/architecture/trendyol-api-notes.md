# Trendyol Türkiye Marketplace API notes

Research date: 7 August 2026

These notes cover the Türkiye Marketplace documentation only. International Marketplace contracts are intentionally out of scope for the initial package. Official Trendyol Developer documentation is the source of truth; ambiguities below are not resolved through inference.

## API groups

The Türkiye Marketplace documentation currently groups operations around:

- Product integration V2 and legacy Product V1
- Orders and shipment packages
- Shipment package webhooks
- Delivery and common labels
- Returns
- Customer questions and answers
- Seller information and addresses
- Invoice integration
- Accounting and finance
- Supporting catalog, carrier, address, and health-check operations

Source: [Trendyol integration overview](https://developers.trendyol.com/docs/getting-started)

## Environments and base hosts

| Environment | Base host |
|---|---|
| Production | `https://apigw.trendyol.com` |
| Stage | `https://stageapigw.trendyol.com` |

Stage accounts and credentials differ from production credentials. The documentation says stage access requires Trendyol-side IP authorization and reports that a stage 503 can indicate missing authorization. Production does not ordinarily require IP authorization.

The SDK therefore models production and stage explicitly and does not expose an arbitrary base-address option that could forward credentials to an untrusted host.

Source: [Production and stage environment information](https://developers.trendyol.com/docs/3-canl%C4%B1-test-ortam-bilgileri)

## Authentication and required headers

Partner API requests use HTTP Basic authentication. The API key is the Basic username and the API secret is the Basic password, producing `Authorization: Basic base64(apiKey:apiSecret)`.

Requests must also contain a `User-Agent` value in one of these documented forms:

- `{SellerId} - {IntegratorCompanyName}` for an integration company
- `{SellerId} - SelfIntegration` for seller-owned software

The integrator name is documented as alphanumeric with a maximum length of 30 characters. Requests without the required User-Agent can receive HTTP 403. Invalid authentication is documented as HTTP 401 with `exception` equal to `ClientApiAuthenticationException`.

Credentials differ between stage and production. They must not be logged, included in exception messages, committed to source control, or rendered by option debugging helpers.

Sources: [Authorization](https://developers.trendyol.com/docs/2-authorization) and [support-request authentication example](https://developers.trendyol.com/docs/6-nas%C4%B1l-bildirim-olu%C5%9Fturabilirim)

## API versions and deprecations

### Product API

[Product V2](https://developers.trendyol.com/docs/%C3%BCr%C3%BCn-v2-api-endpoint) is the implementation baseline. Its endpoint matrix identifies operations exclusive to V2 and operations shared by V1 and V2.

The official documentation currently gives conflicting Product V1 retirement dates:

- The developer portal homepage says Product V1 will close on **15 September 2026**.
- The dedicated Product V1 endpoint page says Product V1 will become unavailable on **10 August 2026**.

Sources: [developer portal homepage](https://developers.trendyol.com/) and [Product V1 endpoint page](https://developers.trendyol.com/docs/%C3%BCr%C3%BCn-api-endpoint)

This conflict is unresolved. The SDK will not implement Product V1, so its behavior does not depend on either retirement date.

### Order API

Order V2 is live and becomes mandatory on **15 October 2026**. The current page-based route is:

`GET /integration/order/sellers/{sellerId}/v2/orders`

The documented V2 behavior includes zero-based pages, a maximum page size of 200, and a maximum accessible query window of 10,000 shipment packages. The endpoint is intended for bounded queries, not full historical scans.

Source: [Get shipment packages / Order V2](https://developers.trendyol.com/docs/sipari%C5%9F-paketlerini-%C3%A7ekme-getshipmentpackages)

For large scans and synchronization, Trendyol recommends:

`GET /integration/order/sellers/{sellerId}/orders/stream`

This operation uses `nextCursor`, `hasMore`, and `size`. The cursor is opaque, must not be modified, and must be continued with the same filters. Changing filters while using a cursor is documented to return HTTP 400. The stream finishes when `hasMore` is false.

Source: [Cursor-based shipment packages](https://developers.trendyol.com/v2.0/docs/getshipmentpackagesstream)

## Rate limiting

The general authorization page documents a maximum of 50 requests to the same endpoint within 10 seconds. The current service-limit page is more specific and lists different limits by service, seller product tier, and—in the future Product V2 model—shared limit group.

For example, product read, product write, and inventory/price write operations are documented as separate shared groups after 14 September 2026, while order limits vary by seller product-listing tier. Other groups such as returns and questions have their own limits.

Sources: [Authorization](https://developers.trendyol.com/docs/2-authorization) and [service limits](https://developers.trendyol.com/docs/1-servis-limitleri)

The documents do not clearly define precedence between the general rule and every service-specific rule. The initial SDK will surface HTTP 429 and any standard `Retry-After` header but will not implement automatic throttling or retry.

## Responses and errors

The common status documentation lists 200, 201, 202, 204, 400, 401, 403, 404, 405, 409, 414, 415, 429, 500, 502, 503, and 504. It does not define one universal JSON error envelope.

Observed official examples include:

- `exception`, such as `ClientApiAuthenticationException`
- a top-level `message`
- an `errors` array containing `key`, `message`, and `errorCode`
- batch-item `failureReasons`, which represent asynchronous business failures rather than necessarily an HTTP failure

Sources: [HTTP error codes](https://developers.trendyol.com/docs/hata-kodlar%C4%B1), [authorization failure](https://developers.trendyol.com/docs/2-authorization), and [Product archive error examples](https://developers.trendyol.com/docs/%C3%BCr%C3%BCn-ar%C5%9Fivleme-archiveproducts)

The SDK error parser must be tolerant, retain the known structured fields, and avoid retaining the raw response body by default.

## Pagination and batch conventions

Pagination is not uniform:

- Several list operations use zero-based `page` and `size` with content and total fields.
- Order streaming uses an opaque cursor.
- Brand listing returns a `brands` collection without documented total/page metadata. The domestic guide does not state page origin or unambiguous default and maximum sizes.

No universal public page type should be introduced until real endpoint contracts demonstrate a stable shared shape.

Product create and update operations are commonly asynchronous. Successful requests return `batchRequestId`, and results are later queried through the batch-result operation. Batch failures can occur per item. Batch types will be designed with the first actual batch operation rather than inferred globally.

Sources: [Product batch-result documentation](https://developers.trendyol.com/docs/toplu-i%CC%87%C5%9Flem-kontrol%C3%BC-getbatchrequestresult-1) and [brand listing](https://developers.trendyol.com/docs/trendyol-marka-listesi-getbrands-1)

## Dates, numbers, nullability, and evolving values

- Date fields are endpoint-specific. Documentation describes epoch milliseconds with a mixture of GMT, GMT+3, and local-time wording. Public request and response APIs should use `DateTimeOffset`; conversion rules must be verified per endpoint.
- Monetary and other decimal JSON values should use `decimal`, not binary floating point.
- Nullable annotations must follow the documented contract and examples. Unclear response fields should be nullable rather than assumed present.
- Unknown JSON properties should be ignored for forward compatibility.
- Response values that can evolve should remain strings or use an extensible value type. A global enum converter would make new server values deserialize as failures and is not appropriate.

## First endpoint candidate after foundation review

The recommended first operation is brand lookup by name:

`GET /integration/product/brands/by-name?name={brand-name}`

It is explicitly shared by Product V1 and V2, exercises authentication, a GET query, JSON deserialization, errors, cancellation, and testing, and does not depend on the ambiguous paged brand-list contract.

Source: [Trendyol brand documentation](https://developers.trendyol.com/docs/trendyol-marka-listesi-getbrands-1)
