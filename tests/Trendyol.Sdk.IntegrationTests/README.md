# Integration tests

This project contains read-only smoke tests against Trendyol Stage. They remain opt-in and skip safely by default.

Set `TRENDYOL_STAGE_SELLER_ID`, `TRENDYOL_STAGE_API_KEY`, and `TRENDYOL_STAGE_API_SECRET`, then run `dotnet test tests/Trendyol.Sdk.IntegrationTests`. Stage access may also require Trendyol-side IP authorization. Never use production credentials for these tests.
