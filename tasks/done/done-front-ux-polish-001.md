# todo-front-ux-polish-001.md — UX polish improvements across all zones

**Dépendances** : aucune
**Skills** : shadcn-nextjs
**[MSW: oui]**

## Objectif
Address HIGH and MEDIUM priority UX improvements from the designer reviews.

## Improvements by theme

### Cookie consent banner (all zones)
- Fix z-index so banner doesn't overlap page content
- Add bottom padding to main content when banner is visible
- Consider inline top banner instead of sticky bottom bar

### Status badges inconsistency (Dashboard, Billing)
- Standardize to shadcn Badge with color variants:
  - Scheduled → blue outline
  - Checked In → amber
  - In Progress → green
  - Completed → gray
  - Cancelled → red/destructive
  - Draft → gray outline, Sent → blue, Paid → green

### Native selects → shadcn Select (multiple zones)
- Appointments: date picker, vet/time selects → shadcn DatePicker + Select
- Portal new message: pet select, category select → shadcn Select
- Replace browser-native `<select>` and `<input type="date">` with design-system components

### Template category enum display (Messaging)
- "AppointmentRequest" → "Appointment Request"
- "PostOperativeFollowUp" → "Post-Operative Follow-Up"
- Map enum values to human-readable labels

### Mobile table → card layouts (Billing, Stock, Appointments)
- For viewports < 768px, switch table to card-based layout
- Each item as a tappable card with key info stacked vertically
- Increase action touch targets to min 44x44px

### Portal navigation
- Add persistent sidebar (desktop) or bottom tab bar (mobile)
- Links: Conversations, Appointments, My Pets, Profile
- Currently pages are disconnected with only "Back" links

### Search missing from key lists
- Appointments list: add patient/owner name search
- Stock list: add item name search
- Team list: add member search (for larger clinics)
- Billing list: add invoice # and patient search

### Form improvements
- Add required field indicators (asterisks) on appointment form
- Ensure submit buttons visible without scrolling (sticky footer)
- Add "Cancel" buttons to forms that lack them
- Breed field: autocomplete/combobox instead of free text
- Phone numbers: `<a href="tel:...">` clickable links
- Emails: `<a href="mailto:...">` clickable links

### Booking landing page (Portal)
- Increase icon sizes to 48x48px
- Improve description text contrast
- Add "Next Appointment" preview card
- Make "Book Appointment" the visual primary action

### Consent page emergency warning
- Highlight with amber background and warning icon
- Disable "Continue" button until checkbox is checked

## Critère de complétion
- [ ] Cookie banner no longer overlaps content
- [ ] Status badges use consistent color system
- [ ] Native selects replaced with shadcn components
- [ ] Template categories show human-readable names
- [ ] Mobile tables use card layouts
- [ ] Portal has persistent navigation
- [ ] Key lists have search functionality
- [ ] Forms have proper required indicators and sticky CTAs
- [ ] `npm run build` passes
