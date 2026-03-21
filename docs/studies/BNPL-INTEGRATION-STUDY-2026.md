# BNPL Integration Study -- Tabby & Tamara for Vetolib UAE

**Date**: 2026-03-21
**Author**: Product Research Agent
**Status**: Research complete -- pending PO validation

---

## 1. Executive Summary

No veterinary software in the UAE currently offers integrated BNPL (Buy Now Pay Later). The UAE BNPL market is valued at USD 1.17 billion in 2025, growing to USD 3.92 billion by 2031 (CAGR 21.6%). BNPL is actively expanding beyond retail into **healthcare, automotive, and services** -- veterinary care fits squarely in this trend.

Integrating Tabby and/or Tamara into Vetolib's Billing module would give clinics a competitive advantage: pet owners can split large invoices (surgeries, dental work, imaging) into interest-free installments, reducing payment friction and increasing average ticket size.

**Recommendation**: Start with **Tabby** (larger UAE footprint, healthcare already supported, better docs), add **Tamara** as a second provider later. Both use similar REST APIs so the abstraction cost is low.

---

## 2. Tabby -- API & Integration Analysis

### 2.1 Overview

- **Website**: https://tabby.ai/en-AE
- **API Docs**: https://docs.tabby.ai/api-reference/overview
- **OpenAPI Spec**: Available (openapi.yaml downloadable)
- **15M+ users**, 40,000+ merchants, $10B+ annual transaction volume
- $700M debt financing from JPMorgan (2025)
- Expanding into healthcare and automotive sectors

### 2.2 Integration Model

Tabby offers three integration methods:

| Method | Description | Best for |
|---|---|---|
| **API-only (Direct)** | Server-to-server REST API | Custom backends like Vetolib |
| **Checkout.com plugin** | Via Checkout.com payment gateway | Merchants already on Checkout.com |
| **E-commerce plugins** | Magento, WooCommerce, Shopify | Off-the-shelf stores |

**For Vetolib: API-only (Direct)** is the right choice.

### 2.3 API Endpoints

| Operation | Method | Endpoint |
|---|---|---|
| Create checkout session | POST | `/v2/checkout` |
| Retrieve payment | GET | `/v2/payments/{id}` |
| Capture payment | POST | `/v1/payments/{id}/captures` |
| Refund payment | POST | `/v1/payments/{id}/refunds` |
| Close payment | POST | `/v1/payments/{id}/close` |

**Base URLs**: Region-specific domains (UAE vs KSA). All payloads identical across regions.

**Authentication**: API key in header. Separate keys for sandbox vs production (auto-detected by Tabby).

### 2.4 Payment Flow

```
1. Vet creates invoice in Vetolib
2. Owner clicks "Pay with Tabby" (or receives payment link)
3. Vetolib backend calls POST /v2/checkout with:
   - order amount, currency (AED), items
   - buyer info (name, email, phone)
   - success/cancel/failure redirect URLs
   - webhook URL for status updates
4. Tabby returns checkout_url + payment.id
5. Owner is redirected to Tabby checkout page
6. Owner completes KYC + selects plan (Pay in 4, Pay Later)
7. Tabby sends webhook to Vetolib:
   - AUTHORIZED → mark invoice as paid (clinic gets full amount)
   - REJECTED → owner must use another payment method
   - EXPIRED → session timed out
8. Vetolib calls POST /v1/payments/{id}/captures to capture funds
```

### 2.5 Webhook Statuses

| Status | Action |
|---|---|
| `CREATED` | Pending, no action needed |
| `AUTHORIZED` | Payment approved, proceed with capture |
| `CLOSED` | Payment completed successfully |
| `REJECTED` | Customer denied, ask for alternative payment |
| `EXPIRED` | Session expired, generate new link |

### 2.6 Pricing (Merchant Fees)

- **Variable commission**: percentage per transaction (industry-dependent, negotiated per merchant)
- **Fixed fee**: AED 1 per successful transaction
- **Typical range**: 4-8% (estimated for services sector, not publicly disclosed)
- No hidden fees, no setup fees
- Pricing based on industry and annual sale volumes

### 2.7 Order Limits

| Parameter | Value |
|---|---|
| Minimum (Pay Later) | AED 50 |
| Minimum (Installments) | AED 50 |
| Maximum (standard) | AED 5,000 - 7,500 |
| Maximum (high-trust merchants) | Up to AED 25,000 |

These limits are configurable per merchant during onboarding. Veterinary surgeries (AED 2,000-10,000) fit within typical limits.

### 2.8 Merchant Onboarding

1. Apply via https://tabby.ai/en-AE/business or contact account manager
2. KYB (Know Your Business) review: trade license, bank details
3. Technical integration (sandbox → production)
4. Go-live in "a few days" per Tabby's documentation
5. No .NET SDK published; use raw REST API (OpenAPI spec available for codegen)

---

## 3. Tamara -- API & Integration Analysis

### 3.1 Overview

- **Website**: https://tamara.co/en-ae
- **API Docs**: https://docs.tamara.co/
- **Quick Start**: https://docs.tamara.co/docs/direct-quick-start-guide
- Saudi-based, expanding aggressively in UAE
- First consumer finance license from SAMA (Saudi, March 2025)
- **.NET SDK available**: `Tamara.SDK` v1.0.3 on NuGet (.NET Standard 2.1)

### 3.2 Integration Model

| Method | Description |
|---|---|
| **Direct API** | REST API, server-to-server |
| **Checkout.com plugin** | Via Checkout.com gateway |
| **E-commerce plugins** | WooCommerce, Magento, OpenCart |
| **POS Integration** | In-store checkout sessions |
| **.NET SDK** | NuGet package `Tamara.SDK` |

**For Vetolib**: Direct API or .NET SDK. The NuGet package is a wrapper around the REST API.

### 3.3 API Endpoints

| Operation | Method | Endpoint |
|---|---|---|
| Check payment options | GET | `/checkout/payment-types` |
| Create checkout session | POST | `/checkout` |
| Authorize order | POST | `/orders/{order_id}/authorise` |
| Capture order | POST | `/payments/capture` |
| Refund | POST | `/payments/refund` |

**Authentication**: Bearer token in `Authorization` header. Tokens provided during merchant onboarding.

### 3.4 Payment Flow

```
1. Vet creates invoice in Vetolib
2. Owner clicks "Pay with Tamara"
3. Vetolib backend calls POST /checkout with:
   - total_amount, currency (AED), items
   - consumer info (first_name, last_name, email, phone)
   - merchant_url (success, cancel, failure, notification)
   - order_reference_id (Vetolib invoice ID)
4. Tamara returns: order_id, checkout_id, checkout_url
5. Store order_id in Vetolib DB
6. Owner is redirected to checkout_url (Tamara-hosted page)
7. Owner completes verification + selects installment plan
8. Tamara sends webhook (approved/declined)
9. On "approved": Vetolib calls POST /orders/{order_id}/authorise
10. On shipment/service delivery: Vetolib calls POST /payments/capture
```

### 3.5 Pricing (Merchant Fees)

- **Commission**: 2.5% - 6% per transaction (varies by merchant category)
- Settlements: weekly
- Late fees charged to consumers: AED 25 per missed payment, capped at AED 150 or 25% of order value

### 3.6 Order Limits

| Parameter | Value |
|---|---|
| Minimum | AED 99 |
| Maximum | Up to AED 3,000 (varies by customer credit profile) |

Lower maximum than Tabby -- may be limiting for expensive surgical procedures.

### 3.7 Merchant Onboarding

1. Apply via https://partners.tamara.co/onboarding
2. KYB due diligence + AML/CFT screening
3. Documentation: trade license, ownership structure, bank account
4. Sandbox credentials provided for integration testing
5. Compliance-by-design approach (more rigorous than Tabby)

### 3.8 .NET SDK Details

```xml
<PackageReference Include="Tamara.SDK" Version="1.0.3" />
```

Configuration in `appsettings.json`:
```json
{
  "TamaraPayment": {
    "BaseUrl": "https://api-sandbox.tamara.co",
    "ApiToken": "...",
    "NotificationPrivateKey": "..."
  }
}
```

---

## 4. Comparative Analysis: Tabby vs Tamara

| Criterion | Tabby | Tamara | Winner |
|---|---|---|---|
| **UAE market share** | #1, 15M+ users | #2, strong in KSA | Tabby |
| **Merchant fees** | ~4-8% + AED 1/tx | 2.5-6% | Tamara (lower) |
| **Max order (standard)** | AED 5,000-7,500 | AED 3,000 | Tabby |
| **Min order** | AED 50 | AED 99 | Tabby |
| **API documentation** | Excellent (OpenAPI spec) | Good (REST docs) | Tabby |
| **.NET SDK** | None (use REST) | Yes (NuGet 1.0.3) | Tamara |
| **Healthcare sector** | Expanding into healthcare | Primarily retail | Tabby |
| **Onboarding speed** | "Few days" | More rigorous KYB | Tabby |
| **Webhook model** | Status-based callbacks | Event-based callbacks | Tie |
| **POS support** | Yes | Yes (dedicated API) | Tie |
| **Consumer UX** | Superior (more users = familiarity) | Good | Tabby |

### Recommendation

**Phase 1: Integrate Tabby first**
- Larger UAE footprint = more pet owners already have Tabby accounts
- Higher order limits = covers expensive surgeries
- Healthcare sector expansion = easier merchant approval
- Better API docs with OpenAPI spec for code generation

**Phase 2: Add Tamara as alternative**
- Lower merchant fees = clinics can choose based on cost preference
- .NET SDK available = faster integration
- Same abstraction layer supports both providers

---

## 5. Architecture -- Integration into Vetolib

### 5.1 Module Placement

BNPL belongs in the **Billing module** (`Modules/Billing/`). No new module needed -- this is a payment method extension of existing invoice functionality.

### 5.2 New Components

```
Modules/Billing/
  Vetolib.Billing.Contracts/
    BnplProvider.cs              ← enum { Tabby, Tamara }
    BnplPaymentStatus.cs         ← enum { Pending, Authorized, Captured, Rejected, Expired, Refunded }
    CreateBnplSessionRequest.cs  ← request DTO
    BnplSessionDto.cs            ← response DTO (checkoutUrl, sessionId, provider)
    BnplWebhookEvent.cs          ← webhook payload
    InvoiceStatus.cs             ← ADD: BnplPending, BnplAuthorized (new enum values)

  Vetolib.Billing/
    Domain/
      BnplPayment.cs             ← entity: InvoiceId, Provider, ExternalId, Status, Amount, CreatedAt
    Application/
      CreateBnplSession/
        CreateBnplSessionCommand.cs
        CreateBnplSessionHandler.cs
        CreateBnplSessionValidator.cs
      HandleBnplWebhook/
        HandleBnplWebhookCommand.cs
        HandleBnplWebhookHandler.cs
      CaptureBnplPayment/
        CaptureBnplPaymentCommand.cs
        CaptureBnplPaymentHandler.cs
    Infrastructure/
      BnplProviders/
        IBnplProviderClient.cs   ← interface (strategy pattern)
        TabbyClient.cs           ← Tabby REST API implementation
        TamaraClient.cs          ← Tamara REST API (or SDK wrapper)
        BnplProviderFactory.cs   ← resolves provider by enum
    Api/
      BnplEndpoints.cs           ← Minimal API endpoints
```

### 5.3 New Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/v1/billing/invoices/{invoiceId}/bnpl` | Create BNPL checkout session for an invoice |
| POST | `/api/v1/billing/bnpl/webhooks/tabby` | Tabby webhook receiver (no auth, signature validation) |
| POST | `/api/v1/billing/bnpl/webhooks/tamara` | Tamara webhook receiver |
| POST | `/api/v1/billing/invoices/{invoiceId}/bnpl/capture` | Manually capture authorized BNPL payment |
| GET  | `/api/v1/billing/invoices/{invoiceId}/bnpl/status` | Check BNPL payment status |

### 5.4 Invoice Status Changes

Current statuses: `Draft`, `Sent`, `Paid`, `Cancelled`

New statuses to add:

```csharp
public enum InvoiceStatus
{
    Draft,
    Sent,
    BnplPending,      // BNPL session created, awaiting customer action
    BnplAuthorized,    // BNPL approved, awaiting capture
    Paid,              // Includes both direct and BNPL-captured payments
    Cancelled,
    BnplRejected       // BNPL denied by provider
}
```

### 5.5 Payment Flow (Full Sequence)

```
┌─────────┐     ┌──────────┐     ┌──────────┐     ┌──────────────┐
│   Vet   │     │ Vetolib  │     │  Tabby/  │     │  Pet Owner   │
│  Staff  │     │ Backend  │     │  Tamara  │     │   (Client)   │
└────┬────┘     └────┬─────┘     └────┬─────┘     └──────┬───────┘
     │               │               │                    │
     │ Create Invoice │               │                    │
     │──────────────>│               │                    │
     │               │               │                    │
     │ Click "Send   │               │                    │
     │  BNPL Link"   │               │                    │
     │──────────────>│               │                    │
     │               │ POST /checkout │                    │
     │               │──────────────>│                    │
     │               │  checkout_url  │                    │
     │               │<──────────────│                    │
     │               │               │                    │
     │               │  SMS/Email with payment link        │
     │               │────────────────────────────────────>│
     │               │               │                    │
     │               │               │   Open link,       │
     │               │               │   choose plan      │
     │               │               │<───────────────────│
     │               │               │                    │
     │               │   Webhook:    │                    │
     │               │   AUTHORIZED  │                    │
     │               │<──────────────│                    │
     │               │               │                    │
     │               │ Auto-capture  │                    │
     │               │──────────────>│                    │
     │               │               │                    │
     │  Invoice: Paid │               │                    │
     │<──────────────│               │                    │
     │               │               │   Confirmation     │
     │               │               │──────────────────>│
```

### 5.6 Database Changes

New table: `BnplPayments`

| Column | Type | Description |
|---|---|---|
| Id | UUID | PK |
| InvoiceId | UUID | FK to Invoices |
| Provider | varchar(20) | "Tabby" or "Tamara" |
| ExternalSessionId | varchar(100) | Tabby payment.id or Tamara order_id |
| ExternalCheckoutUrl | text | URL sent to pet owner |
| Status | varchar(20) | BnplPaymentStatus enum |
| Amount | decimal(18,2) | Payment amount in AED |
| CreatedAt | timestamptz | Session creation time |
| AuthorizedAt | timestamptz | When provider authorized |
| CapturedAt | timestamptz | When funds captured |
| ClinicId | UUID | Multi-tenant filter |

### 5.7 Configuration

```json
// appsettings.json
{
  "Bnpl": {
    "Tabby": {
      "BaseUrl": "https://api.tabby.ai",
      "SecretKey": "sk_...",
      "PublicKey": "pk_...",
      "MerchantCode": "...",
      "WebhookSecret": "..."
    },
    "Tamara": {
      "BaseUrl": "https://api.tamara.co",
      "ApiToken": "...",
      "NotificationPrivateKey": "..."
    },
    "DefaultProvider": "Tabby"
  }
}
```

### 5.8 Frontend Changes

| Component | Location | Change |
|---|---|---|
| Invoice detail page | `src/frontend/app/[locale]/(app)/billing/` | Add "Send BNPL Payment Link" button |
| BNPL status badge | Same page | Show BnplPending/BnplAuthorized status |
| Settings page | `src/frontend/app/[locale]/(app)/settings/` | BNPL provider configuration (API keys) |
| Payment link page | Public route (no auth) | Owner-facing page with Tabby/Tamara redirect |

---

## 6. Risk Analysis

| Risk | Severity | Mitigation |
|---|---|---|
| Merchant category rejection | Medium | Pre-validate with Tabby/Tamara that veterinary services are eligible. Healthcare BNPL is expanding in UAE -- strong signal. |
| Order limits too low for surgeries | Low | Tabby supports up to AED 25,000 for trusted merchants. Negotiate during onboarding. |
| Webhook reliability | Medium | Implement retry logic + polling fallback (GET payment status every 5 min for pending sessions). |
| Multi-tenant data isolation | Low | BnplPayments table includes ClinicId, covered by existing global query filter. |
| Provider API changes | Low | Strategy pattern (IBnplProviderClient) isolates provider-specific code. |
| Regulatory changes | Low | Both Tabby and Tamara are UAE-regulated. Monitor CBUAE guidelines. |
| Fee impact on clinic margins | Medium | Clearly display merchant fee % in settings. Let clinics decide to absorb or pass to client. |

---

## 7. Effort Estimation

### Phase 1 -- Tabby Integration (MVP)

| Task | Type | Estimated Points | Description |
|---|---|---|---|
| 1 | Backend | 3 | Domain: BnplPayment entity, BnplProvider/BnplPaymentStatus enums, InvoiceStatus extension |
| 2 | Backend | 5 | Infrastructure: TabbyClient (REST), IBnplProviderClient interface, BnplProviderFactory |
| 3 | Backend | 3 | Application: CreateBnplSessionCommand/Handler + validator |
| 4 | Backend | 3 | Application: HandleBnplWebhookCommand/Handler (Tabby webhook processing) |
| 5 | Backend | 2 | Application: CaptureBnplPaymentCommand/Handler |
| 6 | Backend | 2 | API: BnplEndpoints (5 endpoints) |
| 7 | Backend | 2 | EF Migration: BnplPayments table + InvoiceStatus enum update |
| 8 | Backend | 3 | Tests: TU (domain + handlers) + TI (endpoint contracts) + TF (Gherkin scenarios) |
| 9 | Frontend | 3 | Invoice detail: "Send BNPL Link" button + status badges |
| 10 | Frontend | 2 | Settings: BNPL provider configuration page |
| 11 | Frontend | 2 | Public payment link page (owner-facing redirect) |
| 12 | Frontend | 1 | MSW handlers for BNPL endpoints |
| 13 | Wire | 1 | Wire frontend to real backend, Playwright E2E |

**Phase 1 Total: ~32 story points / 13 tasks**

### Phase 2 -- Tamara Addition

| Task | Type | Estimated Points | Description |
|---|---|---|---|
| 14 | Backend | 3 | Infrastructure: TamaraClient (NuGet SDK or REST) |
| 15 | Backend | 2 | Application: HandleTamaraWebhookCommand/Handler |
| 16 | Backend | 2 | Tests: TU + TI for Tamara-specific flows |
| 17 | Frontend | 1 | Settings: Tamara config fields + provider selector |
| 18 | Wire | 1 | E2E for Tamara flow |

**Phase 2 Total: ~9 story points / 5 tasks**

### Grand Total: ~41 story points / 18 tasks

At current velocity (~8 points/sprint), this is approximately **5 sprints** for both phases, or **3 sprints** for Tabby-only MVP.

---

## 8. Gherkin Scenarios (Draft)

```gherkin
Feature: BNPL Payment for Veterinary Invoices

  Scenario: Vet sends BNPL payment link to pet owner
    Given an invoice of 2500 AED for "Max" the dog's dental surgery
    And the invoice status is "Sent"
    When the vet clicks "Send BNPL Payment Link"
    Then a payment link is generated
    And the invoice status changes to "BNPL Pending"
    And the pet owner receives the link via SMS

  Scenario: Pet owner completes BNPL payment with Tabby
    Given a BNPL payment link for invoice INV-2026-0042
    When the pet owner opens the link
    And selects "Pay in 4 installments"
    And completes Tabby verification
    Then the invoice status changes to "Paid"
    And the clinic receives the full amount from Tabby

  Scenario: Pet owner is rejected by BNPL provider
    Given a BNPL payment link for invoice INV-2026-0043
    When the pet owner opens the link
    And Tabby rejects the payment request
    Then the invoice status changes to "BNPL Rejected"
    And the pet owner is informed to use an alternative payment method

  Scenario: BNPL session expires without action
    Given a BNPL payment link was sent 48 hours ago
    And the pet owner has not opened the link
    When the session expires
    Then the invoice status reverts to "Sent"
    And the vet can resend a new BNPL link

  Scenario: Clinic configures BNPL provider in settings
    Given the clinic admin is on the settings page
    When they enter Tabby API credentials
    And save the configuration
    Then BNPL payment option becomes available on invoices
```

---

## 9. Competitive Advantage Assessment

| Factor | Impact |
|---|---|
| **First mover** | No UAE vet software offers BNPL. First to market = strong differentiator in sales pitch. |
| **Higher ticket acceptance** | Owners more likely to approve expensive treatments (surgery, imaging) when they can split payments. |
| **Reduced bad debt** | Clinic gets paid immediately by Tabby/Tamara. Provider assumes installment risk. |
| **Modern UX signal** | Shows clinics that Vetolib is tech-forward, aligned with UAE consumer expectations. |
| **Marketing angle** | "Let your clients pay in installments" -- compelling for clinic acquisition campaigns. |

---

## 10. Sources

- [Tabby API Reference](https://docs.tabby.ai/api-reference/overview)
- [Tabby Business](https://tabby.ai/en-AE/business)
- [Tabby Pricing](https://tabby.ai/en-AE/help-business/about-tabby/pricing)
- [Tamara Documentation Hub](https://docs.tamara.co/)
- [Tamara Create Checkout Session API](https://docs.tamara.co/reference/createcheckoutsession)
- [Tamara Quick Start Guide](https://docs.tamara.co/docs/direct-quick-start-guide)
- [Tamara .NET SDK (NuGet)](https://www.nuget.org/packages/Tamara.SDK/)
- [Tamara .NET SDK (GitHub)](https://github.com/tamara-solution/dotnet-sdk)
- [UAE BNPL Market Report 2025 -- $4.82B by 2030](https://www.businesswire.com/news/home/20251127330476/en/UAE-Buy-Now-Pay-Later-Business-Report-2025)
- [UAE BNPL Market Expands Beyond Retail to Healthcare & Automotive](https://www.globenewswire.com/news-release/2025/02/17/3027281/28124/en/United-Arab-Emirates-Buy-Now-Pay-Later-Market-Report-2025)
- [UAE BNPL Report 2026 -- $3.92B by 2031](https://www.globenewswire.com/news-release/2026/02/03/3230793/28124/en/United-Arab-Emirates-Buy-Now-Pay-Later-Business-Report-2026)
- [Tabby Success Story](https://uaestartupstory.com/tabby-success-story/)
- [Tamara vs Tabby Comparison](https://paymentproviders.io/compare/tamara-vs-tabby)
- [Tabby via Checkout.com](https://www.checkout.com/docs/payments/add-payment-methods/tabby/api-only)
- [Tamara via Checkout.com](https://www.checkout.com/docs/payments/add-payment-methods/tamara/api-only)
