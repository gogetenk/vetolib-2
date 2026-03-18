# Vetolib Design Direction

> Design review and actionable recommendations for the Vetolib frontend.
> Based on QA screenshot analysis conducted 2026-03-13.

---

## 1. Current State Assessment

### 1.1 Calendar / Agenda (REFERENCE -- Good Design)

**Screenshots:** `qa-calendar/01-week-view-en.png`, `qa-calendar/02-day-view-en.png`

**What works:**
- Color-coded appointment blocks by consultation type (green, pink, yellow, cyan, purple) provide instant visual scanning
- Time grid is well-proportioned with clear hour markers
- Appointment cards show pet name, owner, and consultation type in a compact layout
- Day/Week/Month toggle and "Filter by vet" are well-placed
- Current day column has a subtle purple/lavender background highlight
- Current time red line indicator in day view is effective
- "New Appointment" CTA is prominent in top-right
- Notification toast in bottom-right is unobtrusive but visible

**What could improve:**
- Sidebar nav items have no hover background, making clickable area unclear
- The "Today" button styling is slightly thin compared to the Day/Week/Month toggle

**Verdict:** This is the quality bar. Every other screen should match this level of visual richness, information density, and use of color to communicate meaning.

---

### 1.2 Patients List

**Screenshot:** `patients/01-patients-list.png`

**What works:**
- Card-based layout is appropriate for patient browsing
- Each card shows species icon, pet name, breed, owner, phone, and visit dates
- "Add Patient" CTA is visible

**What does not work:**
- Cards are too large and sparse -- only 3 visible per row on a wide desktop, with excessive internal whitespace
- The animal icon (generic circle with paw) is identical for every card, adding no information
- No color differentiation -- every card is white-on-white. No way to visually distinguish species, urgency, or status at a glance
- "View Record" button is a ghost button that blends into the card -- low affordance
- "Last visit" and "Next appt" are in small gray text, hard to scan quickly
- Search bar is narrow (occupies ~40% of the content width) and left-aligned, leaving dead space to its right
- No sort/filter controls beyond search -- cannot filter by species, vet, upcoming appointments
- Card grid does not scale well: 3 columns on wide screens wastes horizontal space

---

### 1.3 Patient Detail

**Screenshot:** `patients/04-patient-detail.png`

**What works:**
- Clear patient header with name, species, breed, age, sex
- Tabbed interface (Medical Records / Prescriptions / Vaccinations) is appropriate
- Vital signs (Weight, Temperature, Heart Rate) in a compact row within each record
- Diagnosis and Treatment text is readable

**What does not work:**
- The entire page is a single flat column with no visual zoning -- the header blends into the records list
- Patient info section (owner, phone, email) has no background or card treatment; it floats in whitespace
- No species/breed icon or photo placeholder beyond the tiny generic circle
- Medical record cards have minimal visual hierarchy -- the record title ("Annual vaccination") looks nearly the same weight as the diagnosis text
- Date labels ("20 Nov 2025") are pushed to the far right in a small outline badge that is easy to miss
- No color coding for record types (vaccination vs. injury vs. skin issue) -- everything is gray/black
- Excessive vertical spacing between the patient header and the first record card
- No quick-action area: "Edit" and "New Medical Record" are at the very top, far from the records timeline

---

### 1.4 Medical Records (Standalone Page)

**Screenshot:** `patients/08-medical-records.png`

**What works:**
- Nothing notable. This is a placeholder page.

**What does not work:**
- Completely empty: shows "Records" title, three gray skeleton-like bars, and "Medical records management coming soon."
- The gray bars serve no purpose -- they are not loading skeletons (no animation) and contain no data
- Massive empty white void below the placeholder message
- This page either needs to be removed from navigation or populated with an actual record search/browse interface
- No empty state illustration or helpful guidance

---

### 1.5 Billing List

**Screenshot:** `billing/01-billing-list.png`

**What works:**
- Data table with clear column headers
- Status badges (DRAFT, SENT, PAID) with distinct styling (SENT is black pill, PAID is plain text, DRAFT is plain text)
- Summary footer ("3 invoices -- Total filtered: AED 2,761.50") is useful

**What does not work:**
- Status badge styling is inconsistent: SENT gets a black filled pill, DRAFT is plain uppercase text, PAID is plain uppercase text -- should all use colored badges
- Table rows have no hover state, making it unclear they are clickable
- "View" buttons are redundant if rows should be clickable (and they take up a full column)
- Only one filter (status dropdown) -- no date range filter despite date being a column
- The table is narrow relative to viewport -- content is squeezed into ~70% of available width
- No visual grouping or subtle row striping to aid scanning across wide rows
- "New Invoice" button is black, creating inconsistency with other screens' emerald CTAs
- The "ALL" dropdown filter looks like a basic browser select, not a styled shadcn Select

---

### 1.6 Invoice Detail

**Screenshot:** `billing/05-invoice-detail.png`

**What works:**
- Clear invoice number and status at top
- Clinic info (Happy Paws Veterinary, Dubai, UAE) and client info are separated into distinct cards
- Line items table with proper subtotal/VAT/total breakdown

**What does not work:**
- Three stacked cards (header, client, items) with excessive vertical spacing between them
- The invoice header card wastes space -- invoice number and clinic info could be a single row
- "Draft" status label is plain text next to the invoice number, not a colored badge
- No print/PDF button visible above the fold
- The overall layout does not resemble a professional invoice -- it looks like a database form
- Line items table is functional but has no row borders or alternating backgrounds
- Total section alignment (right-aligned) is correct but lacks visual weight/emphasis

---

### 1.7 Dashboard

**Screenshot:** `dashboard/01-dashboard.png`

**What works:**
- Welcome message with date is a nice personal touch
- Three KPI cards (Appointments Today: 8, Pending Check-in: 3 Urgent, Total Patients: 127) provide at-a-glance metrics
- Setup Checklist is helpful for onboarding
- "Today's Schedule" and "Recent Activity" side-by-side layout is space-efficient
- Recent Activity items have colored icons (orange for invoices, green for records, blue for appointments)

**What does not work:**
- The blue welcome banner at top feels out of place -- its rounded corners and blue background clash with the rest of the monochrome palette
- KPI cards are too small and cramped; the numbers ("8", "3", "127") need more visual weight
- "3 Urgent" in the Pending Check-in card uses a tiny red badge that is easy to overlook
- Setup Checklist takes up valuable above-the-fold space for a first-run feature; should be dismissible or collapsed after setup
- Today's Schedule list has no species icons or color coding -- it is a plain text list
- The left column (schedule) and right column (activity) have no visual separation -- they bleed together
- Time formatting in the schedule is cramped ("09:00 AM" wraps awkwardly)
- No revenue summary or billing stats -- a clinic manager would want to see daily revenue at a glance

---

### 1.8 Appointments List

**Screenshot:** `dashboard/03-appointments-list.png`

**What works:**
- Clean data table with species emoji icons next to patient names
- Status badges with distinct colors (Checked In = black, In Progress = black, Cancelled = red, Scheduled/Completed = gray)

**What does not work:**
- Same issues as billing table: no row hover, no row striping, "View" button column wastes space
- Status badge colors are inconsistent: "Checked In" and "In Progress" are both black pills, making them visually identical
- Date format ("12 Mar 2026 06:00") is dense and not scannable -- time should be visually separated from date
- No "consultation type" column, which the calendar shows prominently
- Only two filters (status dropdown + date picker), but the date picker shows "dd/mm/yyyy" placeholder -- not pre-filled with useful defaults

---

### 1.9 Messages Inbox

**Screenshot:** `messaging/01-messages-inbox.png`

**What works:**
- Master-detail layout (list on left, content on right) is the correct pattern for messaging
- Category badges are colorful and informative (Medical Urgency = red, Post-Op Follow-Up = orange, Appointment = orange, Administrative = gray, Feedback = red)
- Status badges (Open = green, In Progress = yellow, Resolved = green) provide clear state
- Unread indicators (black dots) and message count badges work well
- Filter tabs (All / Open / In Progress / Resolved / Closed) plus category filters are comprehensive

**What does not work:**
- The right panel shows only "Select a conversation to view details." -- this empty state could show a helpful illustration or inbox summary stats
- Message preview text in the list gets truncated; the list items are slightly too tall for the density of info they carry
- The left panel has no scroll indicator, which could confuse users with many messages
- Selected conversation highlight is very subtle (slightly lighter background)

---

### 1.10 Message Conversation

**Screenshot:** `messaging/03-message-conversation.png`

**Verdict:** This screenshot shows a Next.js runtime error ("Missing `<html>` and `<body>` tags in the root layout."), not the actual conversation UI. This is a bug, not a design issue. The conversation view cannot be assessed.

---

### 1.11 Stock List

**Screenshot:** `billing/06-stock-list.png`

**What works:**
- Alert banners at top (low stock = orange, expiring soon = pink) with specific item details are well-designed
- Status badges (Low Stock = red, OK = green, Expiring Soon = orange) use color effectively
- Quantity values use color (red for below-threshold quantities) to draw attention
- Edit and adjust icons in Actions column are appropriate

**What does not work:**
- Table shares the same generic styling issues as other tables (no row hover, no striping)
- Two filter dropdowns both show lowercase "all" -- inconsistent with other screens that use "ALL"

**Verdict:** This is actually one of the better non-calendar screens. The alert banners and color-coded status system demonstrate the kind of visual communication that should be applied everywhere.

---

### 1.12 Login / Auth

**Screenshot:** `auth/01-landing-en.png`

**What does not work:**
- Extremely minimal: centered card on a plain white background
- No branding beyond the word "Vetolib" and muted "Veterinary Management" subtitle
- The "Sign In" button is solid black, not emerald -- brand color is absent on the first screen users see
- No illustration, no pet imagery, no brand personality
- Looks like a generic template, not a professional veterinary product

---

### 1.13 Portal (Pet Owner)

**Screenshot:** `portal/01-portal-landing.png`

**What works:**
- Clean, simple layout appropriate for pet owners (non-technical users)
- "New Message" emerald CTA is consistent with the brand
- Conversation cards with status badges (Open/Resolved) are clear
- Language switcher (Arabic) in top-right is accessible

**What does not work:**
- Very sparse -- only 2 conversations visible with massive empty space below
- No pet summary, no upcoming appointments, no quick links
- "Download all my conversations" link at bottom is oddly prominent for what should be a secondary action

---

## 2. Design Principles

These seven principles should guide all future UI work on Vetolib.

### P1. Information Density Over Decoration
Clinic staff work on desktop monitors 8+ hours a day. Every pixel should earn its place. Prefer compact, data-rich layouts over spacious "marketing" layouts. Reduce padding, eliminate dead whitespace, and let the data breathe through hierarchy, not through spacing.

### P2. Color Communicates Status, Not Just Brand
The calendar proves this: pink = Surgery, green = Exotic, yellow = Grooming, purple = Dental. Apply this principle everywhere. Invoice status, patient species, appointment urgency, stock levels -- all should use a consistent semantic color palette. Reserve emerald for brand CTAs and navigation accents only.

### P3. Scannable Before Readable
A vet glancing at a screen between patients needs to find information in under 2 seconds. Design for scanning: use bold headings, colored badges, and spatial grouping. Details (phone numbers, addresses, notes) should be secondary -- visible but not competing for attention.

### P4. Consistent Component Language
Every table should look like the same table. Every badge should follow the same size/shape/color rules. Every card should have the same border-radius, shadow, and padding. The current inconsistency (black buttons here, emerald there; pills here, plain text there) undermines professionalism.

### P5. Desktop-First, Mobile-Adapted
This is a clinic management tool used primarily on desktop/tablet. Optimize for 1280px+ viewports first. Side panels, multi-column layouts, and data tables should use available horizontal space. Mobile is secondary -- forms and single-record views adapt down, but list views should favor desktop.

### P6. Subtle Warmth, Professional Restraint
The veterinary domain calls for warmth (pet care, empathy) but also clinical professionalism (medical records, prescriptions). Use warm neutrals (stone/warm-gray backgrounds instead of pure white), gentle shadows, and the occasional soft color accent. Avoid coldness (pure gray, sharp borders) and avoid playfulness (bright gradients, excessive emoji).

### P7. RTL-Native, Not RTL-Patched
Every component, spacing value, and icon position must work identically in RTL (Arabic) and LTR (English). Use logical CSS properties (`ps-4` not `pl-4`, `ms-auto` not `ml-auto`). Test every new component in both directions before shipping.

---

## 3. Design System Enhancements

### 3.1 Background Treatments

**Current:** Pure white (`bg-white`) everywhere. Flat, clinical, lifeless.

**Recommended:**
- **Page background:** `bg-stone-50` (warm off-white) instead of `bg-white` for the main content area
- **Sidebar:** `bg-white` with a `border-r border-stone-200` separator -- currently it blends into the page
- **Card backgrounds:** `bg-white` on the `bg-stone-50` page creates natural visual layering without shadows
- **Section headers:** Use `bg-stone-100/60` bands to group related content (e.g., table headers, filter bars)
- **Alert/banner areas:** Already good on Stock page (orange/pink backgrounds); replicate this pattern for dashboard alerts

### 3.2 Card Styles

**Current:** Cards use `border border-stone-200 rounded-lg` with no shadow or hover treatment.

**Recommended:**
- **Default card:** `bg-white rounded-xl border border-stone-200 shadow-sm`
- **Hoverable card** (patient cards, invoice rows): add `hover:shadow-md hover:border-stone-300 transition-shadow cursor-pointer`
- **Selected/active card:** `ring-2 ring-emerald-500/20 border-emerald-300`
- **Elevated card** (modals, popovers): `shadow-lg rounded-xl`
- **Consistent border-radius:** Use `rounded-xl` (12px) for cards, `rounded-lg` (8px) for badges and inputs, `rounded-full` for avatars and pills

### 3.3 Typography Hierarchy

**Current:** Title sizes are appropriate but body text lacks hierarchy. Everything below the page title is similarly weighted.

**Recommended:**
| Role | Class | Usage |
|---|---|---|
| Page title | `text-2xl font-bold text-stone-900` | "Patients", "Billing", "Appointments" |
| Section title | `text-lg font-semibold text-stone-800` | "Records", "Items", "Today's Schedule" |
| Card title | `text-base font-semibold text-stone-900` | Patient name, invoice number |
| Card subtitle | `text-sm text-stone-500` | Species, breed, date |
| Label | `text-xs font-medium uppercase tracking-wide text-stone-400` | "Weight", "Temperature", column headers |
| Body | `text-sm text-stone-700` | Diagnosis text, descriptions |
| Muted | `text-xs text-stone-400` | Timestamps, secondary info |

Key changes: Use `text-stone-*` instead of `text-gray-*` for warmer tones. Use `uppercase tracking-wide` for labels to create visual separation from body text.

### 3.4 Semantic Color Palette

Extend beyond emerald with a consistent status/category color system:

| Semantic Role | Tailwind Color | Hex | Usage |
|---|---|---|---|
| Brand / Primary CTA | `emerald-700` | #047857 | Buttons, active nav, links |
| Brand Light | `emerald-50` | #ecfdf5 | Selected row bg, active tab bg |
| Success / Paid / Resolved | `emerald-600` | #059669 | PAID badge, Resolved status |
| Warning / Pending | `amber-500` | #f59e0b | DRAFT badge, Pending Check-in, In Progress |
| Danger / Cancelled / Urgent | `red-500` | #ef4444 | Cancelled badge, Low Stock, Medical Urgency |
| Info / Scheduled | `blue-500` | #3b82f6 | Scheduled badge, Checked In |
| Surgery | `rose-400` | #fb7185 | Calendar surgery block, Surgery tag |
| Dental | `violet-400` | #a78bfa | Calendar dental block |
| Vaccination | `green-400` | #4ade80 | Calendar vaccination block |
| Grooming | `yellow-400` | #facc15 | Calendar grooming block |
| Exotic | `teal-400` | #2dd4bf | Calendar exotic block |
| Lab / Diagnostics | `sky-400` | #38bdf8 | Calendar lab block |
| Dog | `amber-700` | #b45309 | Species indicator dot/icon |
| Cat | `purple-600` | #9333ea | Species indicator dot/icon |
| Bird | `sky-600` | #0284c7 | Species indicator dot/icon |
| Exotic species | `teal-600` | #0d9488 | Species indicator dot/icon |

**Badge component standardization:**
- Filled badge: `px-2.5 py-0.5 rounded-full text-xs font-medium bg-{color}-100 text-{color}-700`
- All status badges must use this pattern. No more plain uppercase text or inconsistent black pills.

### 3.5 Icon Usage

**Current:** Minimal icon usage. Sidebar uses lucide icons. Patient cards use a generic animal circle.

**Recommended:**
- **Species icons:** Use distinct silhouette icons for Dog, Cat, Bird, Rabbit, Reptile, Camel (UAE-specific). Display as `w-8 h-8` with species color as background circle.
- **Record type icons:** Stethoscope (consultation), Syringe (vaccination), Scissors (surgery), Flask (lab), Sparkles (grooming)
- **Status indicator dots:** Small `w-2 h-2 rounded-full bg-{status-color}` dots next to status text for scanability
- **Empty state illustrations:** Simple line illustrations (not photos) for empty lists -- a sleeping cat for "No records", an open calendar for "No appointments today"
- **Action icons:** Use icon-only buttons for table row actions (Eye for view, Pencil for edit, Trash for delete) with tooltips, instead of text buttons

### 3.6 Data Table Improvements

**Current:** Plain tables with thin separators, no hover, no density control.

**Recommended:**
- **Row hover:** `hover:bg-stone-50` on every `<tr>` -- essential for scanability
- **Row click:** Make entire row clickable where a "View" action exists; remove the redundant "View" button column
- **Column headers:** `bg-stone-50 text-xs font-medium uppercase tracking-wide text-stone-500 py-3` -- visually separate from data
- **Striped rows (optional):** `even:bg-stone-50/50` for very wide tables (billing, stock)
- **Sticky header:** `sticky top-0` for tables that scroll vertically
- **Compact mode:** `py-2 text-sm` for tables with many rows (appointments, stock). Default padding is too generous.
- **Numeric alignment:** Right-align all currency/number columns (`text-right tabular-nums`)
- **Sortable columns:** Indicate sortable columns with a subtle up/down arrow icon on hover

### 3.7 Empty States

**Current:** "Medical records management coming soon." as plain text. No guidance, no illustration.

**Recommended pattern for empty states:**
```
Container: centered, max-w-sm, py-16
Illustration: 64x64 muted icon or simple line drawing
Title: text-lg font-semibold text-stone-700 -- "No medical records yet"
Description: text-sm text-stone-500 -- "Medical records will appear here after the first consultation."
CTA (if applicable): emerald button -- "+ New Medical Record"
```

Apply to: Medical Records standalone page, empty patient record tabs, empty invoice list with filters active, dashboard "no appointments today".

---

## 4. Screen-by-Screen Recommendations

### 4.1 Dashboard

**Priority: HIGH** -- this is the first screen after login.

**Current issues:**
- Blue welcome banner clashes with warm palette direction
- KPI cards are undersized and lack visual impact
- Setup Checklist dominates above-the-fold for returning users
- No revenue or billing metrics
- Schedule list lacks species/type color coding

**Specific changes:**
1. Replace blue welcome banner with a softer `bg-emerald-50 border border-emerald-200` treatment, or remove it entirely for returning users (show only when setup is incomplete)
2. KPI cards: increase to `min-h-[100px]`, use `text-3xl font-bold` for the number, add a subtle icon in the top-right corner (Calendar icon for appointments, Clock for pending, Users for patients). Add a `bg-emerald-50` or `bg-amber-50` tint to the card matching its semantic meaning
3. Add a 4th KPI card: "Today's Revenue" with AED amount
4. Make Setup Checklist dismissible with an "X" button; persist dismissal in localStorage
5. Today's Schedule: add species color dot before each patient name, add consultation type as a small colored badge (match calendar colors)
6. Recent Activity: add `border-l-2 border-{color}` left accent to each activity item based on type (green for medical, blue for appointments, amber for invoices)
7. Add a subtle `border-b border-stone-200` between the KPI row and the schedule/activity section

### 4.2 Patients List

**Priority: HIGH** -- used multiple times daily.

**Current issues:**
- Cards too large, data density too low
- No species differentiation
- No filtering beyond search
- "View Record" button has low affordance

**Specific changes:**
1. Switch from card grid to a **compact card list** or **enriched table** layout:
   - Option A (recommended): Horizontal card rows (`flex items-center gap-4 py-3 border-b`) showing species icon, name+breed, owner+phone, last visit, next appt, quick actions -- all in one scannable line
   - Option B: Keep cards but reduce to `max-w-[280px]` with tighter padding (`p-3` instead of current `p-5+`)
2. Add species color dot + icon to each card/row (dog = amber dot, cat = purple dot, etc.)
3. Add filter bar: Species dropdown, Vet dropdown, "Has upcoming appointment" toggle
4. Make the entire card/row clickable (navigate to patient detail); remove "View Record" button
5. Expand search bar to full width of the filter bar area
6. Add patient count: "5 patients" summary text near the top

### 4.3 Patient Detail

**Priority: HIGH** -- the core clinical workflow screen.

**Current issues:**
- No visual zoning between patient header and records timeline
- Patient info (owner, phone) floats in whitespace
- Medical records have no color coding by type
- Date badges are easy to miss
- No quick-action proximity to records

**Specific changes:**
1. Create a **patient header card**: `bg-white rounded-xl shadow-sm border p-4` containing a 2-column layout:
   - Left: Large species icon (colored circle, 48x48), Pet name (text-xl font-bold), species/breed/age/sex line
   - Right: Owner name, phone (clickable tel: link), email, weight -- in a compact `text-sm` block
2. Add a colored `border-l-4 border-{species-color}` accent to the header card
3. Medical record cards: add a left color accent (`border-l-4`) based on record type (vaccination = green, injury = amber, skin = purple, surgery = rose)
4. Move the date from a far-right outline badge to a `text-xs text-stone-400` line directly below the record title
5. Vital signs row: use `bg-stone-50 rounded-lg px-3 py-2` background to visually group them
6. Add a floating "New Medical Record" button (`fixed bottom-6 right-6` or sticky in the header) so it is always accessible when scrolling through records
7. Add record type icon (syringe, stethoscope, etc.) next to each record title

### 4.4 Medical Records (Standalone Page)

**Priority: HIGH** -- currently a broken placeholder.

**Current issues:**
- Placeholder content with no functionality
- Gray bars serve no purpose
- Massive empty space

**Specific changes:**
1. **Option A (recommended):** Transform into a cross-patient record search page. Show a search bar + date range filter, then a table of recent records across all patients (Date, Patient, Type, Vet, Diagnosis summary, Actions). This gives vets a way to find records without navigating to a specific patient first.
2. **Option B:** If the page truly has no backend, show a proper empty state: illustration of a medical folder, "Medical Records" title, "Access patient records from each patient's profile" description, and a "Go to Patients" emerald button.
3. Remove the meaningless gray bars immediately regardless of which option is chosen.

### 4.5 Billing List

**Priority: MEDIUM** -- used daily but less frequently than patient views.

**Current issues:**
- Inconsistent status badge styling
- No row hover
- Black "New Invoice" button (off-brand)
- Minimal filtering

**Specific changes:**
1. Standardize status badges using the semantic color system:
   - DRAFT: `bg-amber-100 text-amber-700`
   - SENT: `bg-blue-100 text-blue-700`
   - PAID: `bg-emerald-100 text-emerald-700`
   - OVERDUE: `bg-red-100 text-red-700`
   - CANCELLED: `bg-stone-100 text-stone-500`
2. Add `hover:bg-stone-50` to table rows; make rows clickable; remove "View" button column
3. Change "New Invoice" button to `bg-emerald-700 hover:bg-emerald-800 text-white`
4. Add date range filter next to the status dropdown
5. Right-align all currency columns with `tabular-nums`
6. Add `bg-stone-50` to the column header row
7. Total summary footer: increase font weight, add a top border for emphasis

### 4.6 Invoice Detail

**Priority: MEDIUM**

**Current issues:**
- Too much vertical spacing between sections
- Status shown as plain text
- Layout does not feel like an invoice
- No print/export button visible

**Specific changes:**
1. Consolidate invoice header: single row with invoice number (left), status badge (middle), clinic info (right)
2. Status badge: use the colored badge system (Draft = amber, Sent = blue, Paid = emerald)
3. Reduce spacing between card sections from current ~24px gap to ~12px
4. Add a "Print / Download PDF" button in the top action area
5. Total row: `text-lg font-bold` with a `border-t-2 border-stone-300` separator above it
6. Consider a 2-column layout: client info on the left, clinic info on the right, items table below full-width

### 4.7 Appointments List

**Priority: MEDIUM**

**Current issues:**
- Status badges: "Checked In" and "In Progress" are visually identical (both black)
- Date/time formatting is dense
- Missing consultation type column

**Specific changes:**
1. Differentiate status badges:
   - Scheduled: `bg-blue-100 text-blue-700`
   - Checked In: `bg-indigo-100 text-indigo-700`
   - In Progress: `bg-amber-100 text-amber-700`
   - Completed: `bg-emerald-100 text-emerald-700`
   - Cancelled: `bg-red-100 text-red-700`
2. Split date/time display: date in `text-sm`, time in `text-xs text-stone-400` below it
3. Add "Type" column showing consultation type with the calendar's color-coded badge
4. Add row hover and clickable rows (same pattern as billing)
5. Pre-fill date filter with today's date as default

### 4.8 Messages Inbox

**Priority: LOW** -- already well-designed.

**Current issues:**
- Empty right panel lacks personality
- Selected conversation highlight is too subtle

**Specific changes:**
1. Empty right panel: add a centered illustration (envelope icon) with "Select a conversation" text
2. Selected conversation in list: use `bg-emerald-50 border-l-2 border-emerald-600` for clearer selection state
3. Add `hover:bg-stone-50` to non-selected conversation items

### 4.9 Login / Auth

**Priority: LOW** -- seen once per session, but it sets first impression.

**Current issues:**
- No brand identity, no warmth
- Black button instead of emerald
- No imagery or illustration

**Specific changes:**
1. Change "Sign In" button to `bg-emerald-700 hover:bg-emerald-800 text-white`
2. Add the Vetolib logo/icon (stethoscope-paw) above the title at a larger size
3. Add a subtle background: `bg-gradient-to-br from-emerald-50 to-stone-50` covering the full page
4. Consider a left panel with a veterinary illustration (cat/dog silhouette) on desktop, hidden on mobile -- low priority, cosmetic only
5. Add "Don't have an account? Sign up" link below the button

### 4.10 Portal (Pet Owner)

**Priority: LOW** -- separate user base, fewer interactions.

**Current issues:**
- Too sparse, no pet/appointment summary
- "Download all my conversations" is oddly prominent

**Specific changes:**
1. Add a "Your Pets" summary section above conversations showing pet names and upcoming appointments
2. Move "Download all my conversations" to a secondary action (small link or three-dot menu)
3. If no conversations exist, show an empty state with "No messages yet. Contact your clinic with any questions." and the "New Message" button

---

## 5. Implementation Tasks

Each task modifies 1-3 components and can be assigned independently.

### Foundation Tasks (do these first)

| # | Task | Components | Priority | Est. |
|---|---|---|---|---|
| F1 | Global background: change main content area from `bg-white` to `bg-stone-50` | `layout.tsx` or `AppShell` | HIGH | 0.5h |
| F2 | Create a reusable `StatusBadge` component with semantic color mapping | New: `StatusBadge.tsx` | HIGH | 1h |
| F3 | Create a reusable `SpeciesIcon` component with species-to-color mapping | New: `SpeciesIcon.tsx` | HIGH | 1h |
| F4 | Standardize table component: add hover state, sticky header, header bg | `DataTable` or equivalent shared table | HIGH | 1.5h |
| F5 | Typography audit: replace all `text-gray-*` with `text-stone-*` | Global search-and-replace across components | MEDIUM | 1h |
| F6 | Button color audit: replace all black (`bg-black`/`bg-stone-900`) primary buttons with `bg-emerald-700` | All button instances | MEDIUM | 1h |
| F7 | Create `EmptyState` component (icon, title, description, optional CTA) | New: `EmptyState.tsx` | MEDIUM | 1h |

### Screen-Specific Tasks

| # | Task | Components | Priority | Est. |
|---|---|---|---|---|
| S1 | Dashboard: redesign KPI cards (larger numbers, icons, semantic tints) | `DashboardStats` or KPI card component | HIGH | 2h |
| S2 | Dashboard: add species dots and type badges to Today's Schedule | `TodaySchedule` component | HIGH | 1h |
| S3 | Dashboard: make Setup Checklist dismissible | `SetupChecklist` component | MEDIUM | 0.5h |
| S4 | Dashboard: add left border accents to Recent Activity items | `RecentActivity` component | LOW | 0.5h |
| S5 | Patients List: switch to compact row layout with species icons and filters | `PatientsList` page + `PatientCard` component | HIGH | 3h |
| S6 | Patient Detail: create zoned header card with species accent | `PatientDetail` header section | HIGH | 2h |
| S7 | Patient Detail: add color-coded left borders and type icons to medical record cards | `MedicalRecordCard` component | HIGH | 1.5h |
| S8 | Patient Detail: restyle vitals row with background grouping | `VitalsDisplay` or inline in `MedicalRecordCard` | MEDIUM | 0.5h |
| S9 | Medical Records page: replace placeholder with empty state (or record search) | `MedicalRecordsPage` | HIGH | 2h |
| S10 | Billing List: apply `StatusBadge` to all invoice statuses + row hover + clickable rows | `BillingList` page | MEDIUM | 1.5h |
| S11 | Invoice Detail: consolidate header, add status badge, add print button | `InvoiceDetail` page | MEDIUM | 2h |
| S12 | Appointments List: apply `StatusBadge`, add consultation type column, split date/time | `AppointmentsList` page | MEDIUM | 2h |
| S13 | Messages Inbox: improve empty panel state and selected conversation highlight | `MessagesInbox` layout | LOW | 1h |
| S14 | Login page: emerald button, gradient background, larger logo | `LoginPage` | LOW | 1h |
| S15 | Portal landing: add pets summary, improve empty state | `PortalLanding` page | LOW | 1.5h |

### Suggested Execution Order

**Phase 1 -- Foundation (1 day):** F1, F2, F3, F4, F5, F6, F7
These create the shared components and global styles that all screen tasks depend on.

**Phase 2 -- High-Impact Screens (2 days):** S1, S2, S5, S6, S7, S9
Dashboard and Patients are the most-used screens and will show the biggest visual improvement.

**Phase 3 -- Consistency Pass (1 day):** S3, S4, S8, S10, S11, S12
Apply the foundation components to remaining screens.

**Phase 4 -- Polish (0.5 day):** S13, S14, S15
Low-traffic screens and cosmetic improvements.

---

## Appendix: RTL Considerations

All implementation tasks must verify RTL behavior. Specific callouts:

- `border-l-4` accent bars must flip to `border-r-4` in RTL. Use `border-s-4` (logical start property) via Tailwind `rtl:` variant or `@apply border-s-4`.
- `text-right` for currency columns must become `text-left` in RTL. Use `text-end` instead.
- Species icons and status dots placed with `mr-2` must use `me-2` (margin-end).
- The calendar already handles RTL (per QA report, with known bugs). New components should follow the same logical property pattern.
- Test every changed screen in Arabic locale before marking the task complete.
