# QA Report - Billing and Stock

**Branch**: develop  
**Date**: 2026-03-12  
**Analyst**: QA Agent (claude-sonnet-4-6)

---

## Test Results

### e2e/billing/billing.spec.ts (MSW-backed)
- Passed: 18 (all tests)
- Skipped: 0
- Failed: 0

### e2e/stock/stock.spec.ts
- Passed: 0
- Skipped: ALL (global test.skip at line 5)
- Reason: Requires real Aspire backend, deferred to wire task

### e2e/recette/billing.spec.ts
- Passed: 2 (P5-BILLING-02, P5-BILLING-06)
- Skipped: 4 (P5-BILLING-01, P5-BILLING-03, P5-BILLING-04, P5-BILLING-05)
- Reason for skips: See BUG-003. Tests call page.request.post to login but never extract the JWT nor attach it as Authorization Bearer on subsequent API calls. Patient creation is unauthenticated, returns non-2xx, and the test self-skips.

---

## Coverage Analysis

### Pages covered (by e2e/billing/billing.spec.ts with MSW)
- /billing: list view, status filter (see BUG-007 for reliability caveat), grand total, AED amounts, navigation
- /billing/new: patient search, line items, VAT 5% calculation, submit and redirect
- /billing/[id]: DRAFT/SENT/PAID states, action visibility, transitions, client info, amounts

### Pages / features NOT covered by any passing automated test

1. Stock page (/stock): zero MSW-backed E2E tests exist. Entire page untested.
2. Invoice CANCELLED state: no test verifies display or button absence.
3. Delete invoice full flow: no test clicks Delete, confirms dialog, verifies redirect.
4. Download PDF click: button visibility tested in PAID state but click not tested.
5. Edit invoice route: InvoiceDetail links to /billing/[id]/edit which does not exist (BUG-001).
6. Empty invoice list: InvoiceTable renders EmptyState when list is empty. Not tested.
7. Error states: both InvoiceTable and InvoiceDetail render ErrorState on API failure. Not tested.
8. Date-range filter: supported in API client but no UI control and no test.

---

## Bugs Found

### [BUG-001] Edit invoice route does not exist
- **Severity**: High
- **File**: src/components/features/billing/InvoiceDetail.tsx line 361
- **Description**: DRAFT action panel renders Link to /billing/[id]/edit. No page file exists at src/app/[locale]/(dashboard)/billing/[id]/edit/. Clicking Edit on any DRAFT invoice leads to a Next.js 404.
- **Expected**: Either the edit page is created or the Edit button is replaced with an inline edit flow.

### [BUG-002] MSW stock seed data does not match stock.spec.ts assertions
- **Severity**: High
- **File**: src/mocks/handlers/stock.ts vs e2e/stock/stock.spec.ts
- **Description**: Multiple hard mismatches. If the global skip were removed, every assertion group would fail:
  - Test line 68 expects 8 rows; MSW has 6 items.
  - Test line 71: stock ID 001 expected Amoxicillin 250mg (isLowStock, qty 5, threshold 20); MSW has Meloxicam 1.5mg/ml at that ID. Names and IDs are swapped.
  - Test line 77: stock ID 002 expected badge-expiring; MSW isExpiringSoon false for ID 002. Expiring item is ID 003 (Ketamine).
  - Test lines 101-103: Medication filter expected 3 rows including Diphenhydramine; no such item in MSW.
  - Test line 109: low-stock filter expected 3 rows (Amoxicillin, FVRCP Vaccine, Diphenhydramine); MSW returns 2 (Meloxicam + Surgical Gloves); neither FVRCP nor Diphenhydramine exist.
  - Test lines 200-206: movement for ID 003 expected Meloxicam with qty 80; MSW ID 003 is Ketamine with qty 8. After +20 movement test expects 100; actual would be 28.
  - Test line 171: edit prefill expects Amoxicillin 250mg for ID 001; MSW ID 001 is Meloxicam 1.5mg/ml.
- **Expected**: MSW seed data and test expectations must be aligned; one is the single source of truth.

### [BUG-003] Recette billing tests self-skip due to missing Bearer token in API requests
- **Severity**: High
- **File**: e2e/recette/billing.spec.ts lines 97-119, 186-208, 264-287, 320-344
- **Description**: After page.request.post to /api/auth/login, the returned JWT is never extracted from the response body and never passed as Authorization Bearer on subsequent page.request calls. Patient creation POST is unauthenticated, patientRes.ok() is false, test self-skips.
- **Expected**: Extract token from login JSON body and pass Authorization Bearer header to all subsequent API requests.

### [BUG-004] InvoiceTable CardTitle hardcoded in English
- **Severity**: Medium
- **File**: src/components/features/billing/InvoiceTable.tsx line 85
- **Description**: CardTitle renders the literal string Invoices. Component uses useTranslations only for the empty state. Key billing.invoices exists in both en.json and ar.json.
- **Expected**: Add useTranslations call and use the billing.invoices key.

### [BUG-005] InvoiceForm uses a hardcoded static patient list instead of the API
- **Severity**: Medium
- **File**: src/components/features/billing/InvoiceForm.tsx lines 21-26
- **Description**: MOCK_PATIENTS is a static array of 4 hardcoded patients inside the component. No call to lib/api/patients. Real clinic patients never appear. Wire task will break the form. Recette tests that create patients via API will never see them in this dropdown.
- **Expected**: Replace with a debounced GET /api/v1/patients?search=... call using the existing patients MSW handler.

### [BUG-006] InvoiceDetail clinic info is an unremoved placeholder
- **Severity**: Low
- **File**: src/components/features/billing/InvoiceDetail.tsx lines 267-269
- **Description**: Clinic name Happy Paws Veterinary, city Dubai UAE, phone +971 4 000 0000 are hardcoded. Not sourced from auth session or clinic API.
- **Expected**: Source from authenticated user session or a /api/clinic endpoint.

### [BUG-007] Status filter test uses selectOption() on a Shadcn Select
- **Severity**: Medium
- **File**: e2e/billing/billing.spec.ts lines 58 and 67
- **Description**: Playwright selectOption() only works on native HTML select elements. The status-filter is a Shadcn SelectTrigger (button backed by Radix UI). selectOption() silently no-ops, making the two filter tests unreliable.
- **Expected**: Replace with click() on the trigger then click() on the appropriate SelectItem. Add data-testid to each SelectItem in InvoiceTable.

### [BUG-008] StockMovementForm OUT-quantity validation uses stale item quantity
- **Severity**: Medium
- **File**: src/components/features/stock/StockMovementForm.tsx lines 65-66
- **Description**: zodResolver(buildSchema(item?.quantity ?? 0)) evaluated once at form initialisation. If the same item is opened for a second movement, the OUT limit validates against the original quantity. The parent key only remounts on item change, not on re-open of the same item.
- **Expected**: Either validate the OUT limit inside onSubmit at call time, or force remount with a unique key including a session counter.

### [BUG-009] en.json contains corrupted em-dash characters
- **Severity**: Low
- **File**: messages/en.json
- **Description**: Em-dash stored as Windows-1252 mis-decoding of UTF-8 bytes. Affected keys: billing.invoice_count_singular, billing.invoice_count_plural, stock.movement.types.in, stock.movement.types.out. Renders as garbage characters in the browser.
- **Expected**: Re-save in UTF-8 and replace corrupted bytes with U+2014.

---

## Missing data-testid

| Component | Element | Suggested testid |
|---|---|---|
| InvoiceTable | Each status SelectItem | status-option-ALL, status-option-DRAFT, etc. |
| InvoiceTable | CardTitle heading | invoice-card-title |
| InvoiceTable | Empty state wrapper | invoice-empty-state |
| InvoiceForm | No patients found text | patient-not-found |
| InvoiceForm | Patient card section | patient-card |
| InvoiceForm | Items card section | items-card |
| InvoiceDetail | Clinic info block | invoice-clinic-info |
| InvoiceDetail | Notes card (conditional) | invoice-notes-card |

---

## i18n Issues

- I18N-001: InvoiceTable.tsx line 85 -- CardTitle Invoices hardcoded; key billing.invoices exists.
- I18N-002: InvoiceTable.tsx lines 33-38 -- STATUS_OPTIONS labels hardcoded (All statuses, Draft, Sent, Paid, Cancelled); keys exist in billing.status.* and billing.all_statuses.
- I18N-003: InvoiceForm.tsx -- No useTranslations call at all. All form labels, placeholders, button texts, error messages are hardcoded English. Many keys missing from en.json entirely.
- I18N-004: InvoiceDetail.tsx -- STATUS_LABEL map hardcoded; all button texts, section headings, error messages hardcoded. Keys exist in billing.detail.* and billing.status.* but not consumed.
- I18N-005: InvoiceTable.tsx line 68 -- setError(Failed to load invoices) hardcoded; key billing.failed_to_load exists.
- I18N-006: InvoiceForm.tsx lines 70 and 94 -- validation and submit error messages hardcoded in English.
- I18N-007: messages/en.json -- invoice_count_singular, invoice_count_plural, movement.types.in, movement.types.out contain corrupted em-dash bytes (see BUG-009).

---

## a11y Issues

- A11Y-001: InvoiceTable.tsx line 175 -- View button has no aria-label identifying which invoice it opens. Add aria-label such as View invoice INV-2026-001.
- A11Y-002: InvoiceTable.tsx lines 91-105 -- Status filter Select has no associated Label. Screen readers cannot identify the control.
- A11Y-003: InvoiceForm.tsx lines 118-141 -- Custom patient dropdown has no role=listbox / role=option, no keyboard navigation (arrow keys), no Escape-to-dismiss. Should be replaced with Shadcn Command (Combobox).

---

## Recommendations

### Blocking (must be resolved before this zone can be QA_PASS)

1. Create /billing/[id]/edit page or remove the dead Edit link (BUG-001).
2. Align MSW stock seed data with stock.spec.ts expectations (BUG-002) -- stock suite entirely unusable today.
3. Fix Bearer token propagation in recette billing tests (BUG-003) -- 4 of 6 tests cannot execute.
4. Replace MOCK_PATIENTS static array in InvoiceForm with a real API call (BUG-005).
5. Write MSW-backed E2E tests for /stock -- entire zone untested; components, handlers, data-testids are ready.

### Non-blocking (should be tracked as follow-up tasks)

6. Fix status filter test to use Shadcn/Radix click pattern instead of selectOption (BUG-007).
7. Fix StockMovementForm stale OUT-validation closure (BUG-008).
8. Add i18n to InvoiceTable, InvoiceForm, InvoiceDetail (I18N-001 through I18N-006).
9. Fix UTF-8 encoding corruption in en.json (BUG-009 / I18N-007).
10. Add aria-label to View buttons in InvoiceTable (A11Y-001).
11. Add label to status filter Select (A11Y-002).
12. Replace patient dropdown with Combobox for keyboard accessibility (A11Y-003).
13. Add tests for: CANCELLED state, delete full flow, PDF download click, empty list, error states.
14. Remove hardcoded clinic placeholder and source from auth context (BUG-006).

---

**Status**: [QA_FAIL]

**Blocking problems**:
- BUG-001: Edit route is a dead 404 link on all DRAFT invoices.
- BUG-002: Stock MSW data and test expectations out of sync; stock test suite fails if unskipped.
- BUG-003: Recette billing tests cannot authenticate for API setup; 4 of 6 tests self-skip.
- BUG-005: InvoiceForm uses a static patient list; wire task will break the form.
- No MSW-backed tests cover the /stock page at all.

**Non-blocking suggestions**: See recommendations 6-14 above.