# todo-front-ar-translations-001.md — Complete Arabic translations for all zones

**Dépendances** : aucune
**Skills** : shadcn-nextjs
**[MSW: oui]**

## Objectif
All 6 UX zones report that Arabic locale shows English text with RTL layout — translations are missing. Complete the `ar.json` file with all UI strings.

## Zones needing AR translations

### 1. Auth & Landing
- "Email", "Password", "Sign In", "Sign Up", "Forgot password?", "Veterinary Management"
- Signup form: all field labels, validation messages, strength indicators
- Landing page: hero text, feature descriptions, CTAs

### 2. Dashboard & Appointments
- Sidebar nav items: "Agenda", "Patients", "Billing", "Messages", "Stock", "Team", "Settings"
- Dashboard: "Today's Schedule", "Recent Activity", "Setup Checklist", KPI labels
- Appointments: column headers ("Date/Time", "Patient", "Owner", "Veterinarian", "Status"), button labels ("New Appointment", "View"), filter labels
- Status badges: "Scheduled", "Checked In", "In Progress", "Completed", "Cancelled"

### 3. Patients & Medical Records
- Page heading, search placeholder, button labels ("Add Patient", "Import CSV", "View Record")
- Card labels: "Owner:", "Last visit:", "Next appt:"
- Patient detail: tab labels, field labels, vitals labels
- New patient form: all field labels and placeholders

### 4. Billing & Stock
- Column headers for both billing and stock tables
- Button labels: "New Invoice", "Add Item"
- Status badges: "DRAFT", "SENT", "PAID"
- Alert banners: low stock / expiring warnings
- Filter labels

### 5. Portal & Booking
- "Your Conversations", "New Message", "Download all my conversations"
- Booking: "My Appointments", "Select Your Pet", "Book an Appointment"
- Consent page terms
- Export page text

### 6. Messaging & Settings
- Filter labels (status + category), sidebar nav items
- Team page: column headers, button labels
- Templates: column headers, category display names
- Business hours: day names (already may be locale-aware)
- Statistics: KPI labels, chart labels

## Approach
- Edit `src/frontend/messages/ar.json`
- Use professional Arabic translations (not Google Translate)
- Key namespaces to populate: `auth`, `dashboard`, `appointments`, `patients`, `billing`, `stock`, `portal`, `messaging`, `team`, `settings`, `common`

## Critère de complétion
- [ ] All UI chrome text displays in Arabic when locale is `ar`
- [ ] No raw English text visible in AR screenshots (except user-generated content)
- [ ] `npm run build` passes
