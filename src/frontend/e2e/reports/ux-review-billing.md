# UX/UI Review — Billing & Stock Zone

## Summary

The Billing & Stock zone presents a functional and clean interface built on shadcn/ui conventions. The financial data presentation is solid with proper AED currency formatting and VAT breakdowns. However, the RTL (Arabic) version has significant layout issues — reversed column ordering creates confusing date displays, and text alignment needs attention. The mobile views adapt reasonably but the stock table becomes cramped. Overall, this is about 70% production-ready; the LTR English version is polished, but the RTL experience and a few interaction design gaps need addressing before launch.

---

## Screenshot Reviews

### 01-billing-list.png
**Score: 7.5/10**

- **Strengths:**
  - Clean table layout with clear column headers (# Invoice, Patient, Date, Subtotal, VAT, Total AED, Status, Actions).
  - Status badges are well-differentiated: DRAFT (neutral/outline), SENT (dark filled), PAID (green text). The visual hierarchy makes it easy to scan invoice states.
  - Financial totals are right-aligned as expected for numeric data. The "3 invoices — Total filtered: AED 2,761.50" footer summary is a valuable addition.
  - The "+ New Invoice" button is prominently placed top-right with high-contrast black fill.
  - The "ALL" dropdown filter is positioned logically above the table.
  - Left sidebar navigation is clean and well-organized with recognizable icons. The Messages badge (red "3") draws appropriate attention.

- **Issues:**
  - The "Billing" page title (h1) has no subtitle or breadcrumb context — it sits alone above the card, which feels slightly bare.
  - The status badge styling is inconsistent: DRAFT has an outline style, SENT has a dark filled badge, and PAID is plain text with no background. All three should use the same badge component with different color variants.
  - Column spacing is uneven — there is a large gap between "Patient" and "Date" columns vs. the tight packing of VAT/Total/Status/Actions on the right side.
  - The cookie/analytics consent banner at the bottom overlaps the sidebar avatar ("N" icon) and clips the "Settings" label. The banner text is partially obscured by the avatar element.
  - No search or date-range filter is visible — for a billing list that will grow over time, this is a functional gap.
  - Row hover state is not visible in the screenshot; rows should have a subtle hover highlight for scannability.

- **Recommendations:**
  - Normalize all status badges to use `<Badge variant="...">` with consistent sizing: e.g., `variant="outline"` for DRAFT, `variant="default"` (blue) for SENT, `variant="success"` (green) for PAID.
  - Add a search input next to the ALL dropdown, and consider a date-range picker for filtering invoices by period.
  - Fix the consent banner z-index/positioning so it does not collide with the sidebar footer.
  - Add a subtle gray background or left-border highlight on hover for table rows.

---

### 02-billing-list-mobile.png
**Score: 7/10**

- **Strengths:**
  - The sidebar collapses to a hamburger menu, freeing full width for content. Good responsive behavior.
  - The table adapts and remains readable despite the narrower viewport. All columns are still visible.
  - The "+ New Invoice" button remains accessible at the top of the card.
  - The total summary footer is preserved and clearly visible.
  - User avatar moves to a centered position in the top bar, which is clean.

- **Issues:**
  - The table is technically displaying all columns, but at mobile width this creates horizontal density issues. Column headers like "Subtotal (excl. VAT)" are quite long for a narrow viewport. On a real 375px device, horizontal scrolling may be needed.
  - The consent banner at the bottom takes significant vertical space and clips the "Decline" button — only "Accept" is fully visible.
  - The font size appears the same as desktop; financial figures could benefit from slightly reduced sizing on mobile to prevent line wrapping.
  - No card-based layout alternative for mobile — tables are inherently hard to read on small screens. A stacked card layout (invoice number as header, patient/amount/status below) would be more mobile-friendly.

- **Recommendations:**
  - For viewports under 768px, consider hiding columns like "Subtotal (excl. VAT)" and "VAT (5%)" and showing only Invoice#, Patient, Total, Status, and a "View" action. The VAT breakdown is secondary information.
  - Alternatively, switch to a card/list layout on mobile where each invoice is a tappable card with key details stacked vertically.
  - Fix the consent banner so both Decline and Accept buttons are fully visible.
  - Consider making the entire row tappable on mobile instead of requiring the small "View" button.

---

### 03-invoice-new.png
**Score: 7.5/10**

- **Strengths:**
  - The form is well-structured with clear sections: Patient, Invoice Items, Notes. Each section is in its own card, providing good visual separation.
  - Placeholder text is helpful: "Type patient or owner name...", "e.g. Consultation".
  - The "+ Add Item" button is well-placed in the Invoice Items card header.
  - The financial summary (Subtotal, VAT, Total) updates live and is right-aligned per financial convention.
  - The breadcrumb "Back to Billing" with left arrow provides clear navigation context.
  - Input labels (Description, Qty, Unit Price) are above the fields, which is good form UX.

- **Issues:**
  - The "Search patient" label and input feel disconnected — the label sits above the input with the "Patient" card title above that, creating a 3-level hierarchy (Patient > Search patient > input) that is one level too deep.
  - The default Qty value of "1" and Unit Price of "0" are fine, but the line-item total shows "AED 0.00" inline to the right of the Unit Price field without any label. It is unclear what that number represents without scanning the column headers above.
  - The Notes textarea is quite large for an optional field — it could be a collapsible section or a smaller initial height.
  - The Cancel and Create Invoice buttons are cut off at the bottom of the viewport (visible in screenshot 04 but not here). The page requires scrolling to reach the submit action, which is a usability friction point.
  - No visual indicator that Patient is a required field (no asterisk, no red border on empty state).

- **Recommendations:**
  - Simplify the Patient section: remove the "Search patient" sub-label and just use the card title "Patient" with the search input directly below.
  - Add required field indicators (asterisk or inline text) for Patient and at least one line item.
  - Add a line-item total column header ("Line Total" or "Amount") so the AED 0.00 to the right of Unit Price has context.
  - Consider a sticky footer with Cancel/Create Invoice buttons so they are always accessible without scrolling.

---

### 04-invoice-new-filled.png
**Score: 8/10**

- **Strengths:**
  - The patient selection shows excellent detail: patient name "Max", owner "Ahmed Al-Rashid", phone number "+971 50 123 4567", and a "Change patient" link. This is very well-designed and informative.
  - Two line items display correctly with proper calculations (200 + 150 = 350 subtotal, 17.50 VAT, 367.50 total).
  - The red "x" delete buttons on each line item are visible and appropriately placed.
  - The Cancel and Create Invoice buttons are visible at the bottom with good contrast (Cancel as outline, Create Invoice as solid black).
  - The number formatting is consistent with two decimal places throughout.
  - UAE-specific data (Arabic name, +971 phone prefix) demonstrates proper market targeting.

- **Issues:**
  - The "Change patient" link is styled as a blue underlined link at small font size — it could be more prominent, perhaps as a secondary button.
  - The first line item ("General Consultation") does not have a delete button, but the second item ("Vaccination — Rabies") does. This inconsistency is confusing. If the first item is undeletable because at least one item is required, that logic should be communicated visually.
  - The line-item row spacing is tight — the description, qty, and price fields are packed together. On the second row, the fields touch the row above, making it harder to distinguish line items.
  - The Qty field spinner arrows are visible on the second row (focused state) but not on the first. The narrow Qty field (about 60px wide) might be hard to tap on mobile.
  - There is no "Remove item" or "Delete" label next to the red X — screen readers and less tech-savvy users may not understand the icon.

- **Recommendations:**
  - Show the delete button on all line items including the first, or add a visual rule (dashed separator or alternate row shading) between items.
  - Add a slight vertical gap (8-12px) or a light divider between line-item rows.
  - Widen the Qty field slightly (80px minimum) for easier interaction.
  - Add `aria-label="Remove item"` to the delete buttons for accessibility.

---

### 05-invoice-detail.png
**Score: 8/10**

- **Strengths:**
  - Excellent invoice layout that resembles a real printed invoice. The clinic info (Happy Paws Veterinary, Dubai, UAE, phone) is right-aligned at the top, matching professional invoice conventions.
  - The invoice number "INV-2026-001" with a "Draft" badge and creation date is clearly presented.
  - Client section shows owner name, phone, and patient name — all relevant billing information.
  - The Items table is clean with Description, Qty, Unit Price, and Subtotal columns.
  - Financial summary (Subtotal, VAT 5%, Total AED) is properly formatted and right-aligned.
  - The card-based layout with clear section separators (Patient, Client, Items) creates good visual grouping.

- **Issues:**
  - The "Draft" badge next to the invoice number uses a muted style that makes it hard to see at a glance. Given that draft invoices require action, the badge should be more prominent (perhaps amber/yellow).
  - The page title says "Invoices" (plural) but this is a detail view of a single invoice. It should say "Invoice Detail" or just show the invoice number as the page title.
  - There are action buttons cut off at the bottom of the viewport (partially visible in the screenshot). Critical actions like "Mark as Sent", "Mark as Paid", "Print/Download PDF", and "Edit" should be in a sticky action bar or prominently placed.
  - The "Client" section heading could be "Owner" or "Client/Owner" to match the veterinary domain language used elsewhere in the app.
  - No back navigation is visible (no breadcrumb like "Back to Billing" as seen in the new invoice form).
  - The item description "Consultation" is in a blue/link color — if it is not clickable, it should not look like a link.

- **Recommendations:**
  - Add a breadcrumb: "Billing > INV-2026-001" at the top.
  - Make the Draft badge more prominent with an amber/warning color.
  - Add a sticky action bar at the bottom or a button group at the top with: Edit, Send, Mark as Paid, Download PDF.
  - Change the item description text color to match regular body text (black/gray) unless it is actually a clickable link.
  - Consider adding a "Print" icon button in the top-right corner for quick access.

---

### 06-stock-list.png
**Score: 8.5/10**

- **Strengths:**
  - The alert banners at the top are excellent UX — "2 item(s) low on stock" (red/amber background with warning icon) and "1 item(s) expiring within 30 days" (yellow/peach background with clock icon). These immediately draw attention to actionable issues.
  - Alert details include specific items and quantities, which is very helpful (e.g., "Meloxicam 1.5mg/ml — 5 left (min: 20)").
  - The table is well-structured: Name, Category, Quantity, Min. Threshold, Expiry Date, Status, Actions.
  - Color coding is effective: "Low Stock" badges in red, "OK" in green, "Expiring Soon" in amber/orange. The quantity values for low-stock items are also highlighted in red ("5 bottles", "12 boxes"), and expiring dates in red.
  - Two dropdown filters are available for filtering the stock list.
  - The "+ Add Item" button has a plus icon, consistent with other "add" actions in the app.
  - Action icons (edit pencil, adjustment arrows) are clear and appropriately sized.

- **Issues:**
  - The two filter dropdowns both show "all" with no labels — it is unclear what each dropdown filters (Category? Status? Both?). Labels above or inline with the dropdowns are needed.
  - The "Min. Threshold" column shows units with the number (e.g., "20 bottles", "30 tablets"), which is good, but the "Quantity" column does the same. This redundancy adds visual noise. Consider showing units only once.
  - No search functionality is visible — for clinics with large inventories, searching by name would be essential.
  - The em-dash ("—") for items without expiry dates is fine, but could be replaced with "N/A" or left blank with a tooltip for clarity.
  - Row striping or alternating backgrounds would improve scannability for this denser table.

- **Recommendations:**
  - Add labels to the filter dropdowns: "Category: all" and "Status: all".
  - Add a search input for filtering items by name.
  - Consider alternating row backgrounds (very subtle gray/white) for the 6+ row table.
  - Add a "Last Updated" or "Last Restocked" column or tooltip for inventory tracking context.

---

### 07-stock-list-mobile.png
**Score: 7/10**

- **Strengths:**
  - The alert banners adapt well to mobile width and remain fully readable.
  - All table columns are preserved, which is impressive for mobile.
  - The status badges and color coding remain visible and effective.
  - The hamburger menu and user avatar position correctly.
  - Filter dropdowns span the width nicely.

- **Issues:**
  - The table is extremely dense on mobile. Seven columns on a small screen means each column is very narrow, making the data hard to scan. Column headers are truncated or very small.
  - The action icons (edit, adjust) are small and closely spaced — they are likely difficult to tap accurately on a touch device (below the 44x44px minimum touch target recommendation).
  - There is no horizontal scroll indicator, so users may not realize they can scroll if the table overflows.
  - The "Expiry Date" column with ISO format dates (2026-08-15) takes significant horizontal space. A shorter format like "Aug 2026" would save space on mobile.
  - Alert banners take up about 40% of the visible viewport, pushing the table below the fold.

- **Recommendations:**
  - On mobile, collapse the table into a card/list layout: each item as a card with Name + Category as header, Quantity/Status as key metrics, and Expiry/Threshold as secondary detail.
  - Make alerts collapsible or show them as a compact notification bar that can be expanded.
  - If keeping the table layout, hide lower-priority columns (Min. Threshold, Category) and allow horizontal scrolling with a visual indicator.
  - Increase action icon touch targets to at least 44x44px with adequate spacing between them.

---

### 08-billing-ar.png
**Score: 5/10**

- **Strengths:**
  - The overall layout correctly mirrors to RTL: sidebar is on the right, content flows right-to-left, and the user avatar moves to the top-left.
  - Navigation items in the sidebar are right-aligned with icons on the right side, which is correct RTL behavior.
  - The "+ New Invoice" button moves to the top-left of the card (correct for RTL).
  - Status badges (DRAFT, SENT, PAID) remain readable.
  - Financial totals and the summary footer are present.

- **Issues:**
  - **Critical: Date column is garbled.** The dates display as "Mar 2026, 01:00 PM 01" instead of "01 Mar 2026, 01:00 PM". It appears the day number is being placed at the end of the string rather than the beginning. This is likely a date formatting/direction issue where the date string is being reversed by RTL text direction. This must be fixed.
  - **Column headers remain in English.** The headers show "Actions", "Status", "Total AED", "VAT (5%)", "Subtotal (excl. VAT)", "Date", "Patient", "Invoice #" — these should be translated to Arabic or at minimum use `dir="ltr"` on financial/technical columns.
  - The column order is fully reversed (Actions first on the left, Invoice # on the right). While this is technically correct RTL mirroring, for financial tables it creates a jarring reading experience — the invoice number (primary identifier) is pushed to the far right edge.
  - The "ALL" dropdown text remains in English — it should be translated.
  - The total summary text reads ":invoices — Total filtered 3" with the colon at the beginning and the number at the end — this is broken RTL text rendering.
  - The consent banner at the bottom reads right-to-left but the "Decline" button text appears clipped.

- **Recommendations:**
  - **Fix date formatting immediately.** Wrap date strings in a `<span dir="ltr">` or use a locale-aware date formatter that handles Arabic dates correctly (e.g., `Intl.DateTimeFormat('ar-AE')`).
  - Translate all UI text to Arabic: column headers, filter labels, button text, summary footer.
  - Wrap financial numbers and invoice numbers in `<span dir="ltr">` to prevent digit/text reversal in RTL context.
  - Fix the summary footer string construction to handle RTL properly: "3 invoices — Total filtered: AED 2,761.50" should render correctly in Arabic.
  - Test with native Arabic speakers — the current state would be confusing for Arabic-reading users.

---

### 09-stock-ar.png
**Score: 5.5/10**

- **Strengths:**
  - RTL layout mirroring is applied: sidebar on right, "+ Add Item" button on top-left, table columns reversed.
  - Alert banners mirror correctly — the warning/clock icons move to the right side, and the text is right-aligned.
  - Status badges ("Low Stock", "Expiring Soon", "OK") retain their color coding.
  - The color-highlighted quantities (red for low stock) still work in RTL.
  - Filter dropdowns are present and positioned correctly.

- **Issues:**
  - **Alert text is not translated to Arabic.** "item(s) low on stock 2" and "item(s) expiring within 30 days 1" are English text rendered RTL, causing the numbers to appear at the wrong end of the phrase. The number "2" should come before "item(s)" in English, but in RTL it reads as "item(s) low on stock 2" with the count appended at the end.
  - **Column headers remain in English** and are simply reversed in order: Actions, Status, Expiry Date, Min. Threshold, Quantity, Category, Name. These should be in Arabic.
  - **Quantity values are reversed.** "bottles 5" instead of "5 bottles", "tablets 120" instead of "120 tablets". Numbers and units are being reversed by RTL text direction. Each quantity cell needs `dir="ltr"` or the number/unit pattern needs to be wrapped properly.
  - **Expiry dates** appear formatted correctly (2026-08-15), but the ISO format is not localized for Arabic users.
  - The filter dropdown labels ("all") are not translated.
  - The table is missing the medication/supply name emphasis it has in LTR — in RTL, the Name column is pushed to the far right, making it the last thing the eye encounters when scanning left-to-right (which Arabic readers do not do, but the visual weight feels off).

- **Recommendations:**
  - Translate all UI strings to Arabic.
  - Wrap all numeric values with units in `<span dir="ltr">`: quantities ("5 bottles"), thresholds ("20 bottles"), and dates.
  - Use `Intl.DateTimeFormat('ar-AE')` for localized date rendering.
  - Fix alert banner string interpolation: ensure the count is placed correctly in the Arabic sentence structure, e.g., using ICU message format or proper Arabic pluralization.
  - Test the "Add Item" flow in Arabic to ensure form inputs handle RTL correctly.

---

## Priority Improvements

### HIGH Priority

1. **[HIGH] Fix RTL date/number rendering in billing table (08-billing-ar.png)** — Dates display as "Mar 2026, 01:00 PM 01" instead of "01 Mar 2026, 01:00 PM". All date strings, financial amounts, and invoice numbers in table cells must be wrapped in `<span dir="ltr">` or use locale-aware formatters. Affects: billing list components, likely in `src/frontend/src/app/[locale]/(dashboard)/billing/` page or table components.

2. **[HIGH] Fix RTL quantity/unit reversal in stock table (09-stock-ar.png)** — "bottles 5" instead of "5 bottles". All quantity+unit strings must use `<span dir="ltr">` or structured formatting. Affects: stock list components in `src/frontend/src/app/[locale]/(dashboard)/stock/`.

3. **[HIGH] Fix RTL summary footer text (08-billing-ar.png)** — ":invoices — Total filtered 3" is broken. The template string that builds this summary needs proper bidi handling. Use ICU MessageFormat or explicit `dir` attributes.

4. **[HIGH] Translate all UI strings to Arabic** — Column headers, button labels, filter dropdowns, alert text, and summary text are all still in English in the Arabic screenshots. This indicates the i18n translation files for billing and stock are either missing or incomplete. Check `src/frontend/messages/ar.json` or equivalent locale files.

### MEDIUM Priority

5. **[MEDIUM] Normalize invoice status badge styling (01-billing-list.png)** — DRAFT (outline), SENT (dark fill), PAID (plain text) use inconsistent badge variants. Standardize to a single Badge component with color variants: gray/outline for Draft, blue for Sent, green for Paid, red for Overdue.

6. **[MEDIUM] Add mobile card layout for tables (02, 07)** — Both billing and stock tables are too dense on mobile. Implement a responsive card/list view that activates below 768px breakpoint. Each item becomes a tappable card with key information stacked vertically.

7. **[MEDIUM] Add search and date filtering to billing list (01)** — A billing list with only a status filter will not scale. Add a text search (by invoice number or patient name) and a date-range picker.

8. **[MEDIUM] Label filter dropdowns on stock page (06, 09)** — The two "all" dropdowns have no visible labels. Add "Category" and "Status" labels or use placeholder text inside the dropdowns.

9. **[MEDIUM] Add breadcrumb and action buttons to invoice detail (05)** — The detail view lacks navigation back to the billing list and has no visible action buttons (Send, Mark Paid, Print, Edit). Add a breadcrumb and a sticky action bar.

10. **[MEDIUM] Fix consent banner overlap (01, 03)** — The analytics consent banner overlaps the sidebar avatar/footer on desktop. Adjust z-index and positioning so the banner sits above all content without clipping.

### LOW Priority

11. **[LOW] Add row hover states to all tables** — Tables lack visible hover state, making it harder to scan rows. Add `hover:bg-muted/50` to table rows.

12. **[LOW] Add line-item separators in invoice form (04)** — Multiple line items run together without visual separation. Add a light horizontal divider or 8px vertical gap between rows.

13. **[LOW] Improve invoice detail item text color (05)** — The "Consultation" item description appears in blue/link color. If not clickable, use standard body text color.

14. **[LOW] Add alternating row backgrounds to stock table (06)** — With 6+ rows, alternating subtle gray/white backgrounds improve scannability.

15. **[LOW] Make action icons larger on mobile (07)** — Edit and adjust icons in the stock table are below the 44x44px minimum touch target. Increase padding/size on mobile.

---

## Overall Scores Summary

| Screenshot | Score | Key Issue |
|---|---|---|
| 01 - Billing List (Desktop) | 7.5/10 | Inconsistent badge styling, no search |
| 02 - Billing List (Mobile) | 7.0/10 | Table too dense, needs card layout |
| 03 - Invoice New (Empty) | 7.5/10 | Missing required indicators, CTA below fold |
| 04 - Invoice New (Filled) | 8.0/10 | Line item separation, delete button inconsistency |
| 05 - Invoice Detail | 8.0/10 | Missing actions, no breadcrumb, link-color text |
| 06 - Stock List (Desktop) | 8.5/10 | Unlabeled filters, no search |
| 07 - Stock List (Mobile) | 7.0/10 | Table too dense for mobile |
| 08 - Billing AR (RTL) | 5.0/10 | Broken dates, untranslated text, reversed numbers |
| 09 - Stock AR (RTL) | 5.5/10 | Reversed quantities, untranslated text |

**Average: 7.1/10**

The English LTR experience is solid (averaging ~7.6/10) and close to production-ready with minor polish. The Arabic RTL experience (averaging 5.25/10) has critical bugs that would make it unusable for Arabic-speaking users and requires immediate attention before launch.
