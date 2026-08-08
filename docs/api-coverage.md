# Türkiye Marketplace API coverage

Research snapshot: 8 August 2026. Every operation below is Experimental. Product V1, International Marketplace, finance/accounting, common-label, and Stage-only test-helper APIs are intentionally out of scope.

| Feature | SDK operation(s) | Official route |
|---|---|---|
| Catalog | `GetBrandsAsync`, `SearchBrandsByNameAsync` | `GET /integration/product/brands`, `/brands/by-name` |
| Catalog | `GetCategoryTreeAsync` | `GET /integration/product/product-categories` |
| Catalog | `GetCategoryAttributesAsync`, `GetCategoryAttributeValuesAsync` | `GET /integration/product/categories/{categoryId}/attributes` and its values route |
| Products | `CreateAsync` | `POST /integration/product/sellers/{sellerId}/v2/products` |
| Products | `UpdateUnapprovedAsync`, `UpdateContentAsync`, `UpdateVariantsAsync`, `UpdateDeliveryAsync` | `POST /integration/product/sellers/{sellerId}/products/*-bulk-update` |
| Products | `GetByBarcodeAsync`, `GetUnapprovedAsync`, `GetApprovedAsync`, `GetApprovedInventoryAndPriceAsync` | Product V2 filter routes |
| Products | `DeleteAsync`, `SetArchiveStateAsync`, `UnlockAsync` | Product delete, archive-state, and unlock routes |
| Products | `GetBuyboxInformationAsync`, `GetBatchResultAsync`, `GetUpdateAuditsAsync` | Product buybox, batch-request, and update-audit routes |
| Products | `CreateVideoAsync`, `GetVideosAsync`, `CreateBrandAsync` | `/integration/video/sellers/{sellerId}/videos`, `/integration/product/sellers/{sellerId}/brands` |
| Inventory | `UpdatePriceAndInventoryAsync` | `POST /integration/inventory/sellers/{sellerId}/products/price-and-inventory` |
| Orders | `GetShipmentPackagesAsync`, `StreamShipmentPackagesAsync` | `GET /integration/order/sellers/{sellerId}/orders[/stream]` |
| Orders | Package status, unsupplied, box, service delivery, cargo, warehouse, date, and labor methods | Shipment-package mutation routes |
| Orders | Four split methods | `POST .../split-packages`, `/split`, `/quantity-split`, `/multi-split` |
| Orders | Alternative/digital delivery and manual delivered/returned methods | Official shipment-package delivery and return routes |
| Returns | `GetClaimsAsync`, `CreateClaimAsync`, `ApproveClaimItemsAsync` | `/integration/order/sellers/{sellerId}/claims/...` |
| Returns | `CreateClaimIssueAsync`, `GetClaimIssueReasonsAsync`, `GetClaimItemAuditsAsync` | Claim issue, reason, and audit routes |
| Questions | `GetQuestionsAsync`, `GetQuestionAsync`, `AnswerQuestionAsync` | `/integration/qna/sellers/{sellerId}/questions/...` |
| Invoices | `SendInvoiceLinkAsync`, `DeleteInvoiceLinkAsync`, `UploadInvoiceFileAsync` | `/integration/sellers/{sellerId}/seller-invoice-*` |
| Webhooks | Create/list/update/delete/activate/deactivate | `/integration/sellers/{sellerId}/webhooks/{Id}/...` |

Primary sources: [Trendyol Product V2 endpoint matrix](https://developers.trendyol.com/docs/%C3%BCr%C3%BCn-v2-api-endpoint) and the linked [official API reference index](https://developers.trendyol.com/llms.txt).
