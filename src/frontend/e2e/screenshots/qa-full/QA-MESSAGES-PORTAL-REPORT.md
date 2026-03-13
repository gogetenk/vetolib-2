# QA Report: Messages, Owner Portal & Landing Page

**Date**: 2026-03-13
**Agent**: QA Automated (Playwright) + Visual Inspection
**Environment**: localhost:3000 (Next.js dev + MSW)

## Summary

- **Total findings**: 7
- **HIGH**: 1
- **MEDIUM**: 5
- **LOW**: 1
- **Overall**: Good quality. All pages load, MSW data is present, RTL works correctly. One responsive issue found.

## Screenshots Taken

| # | Name | Description |
|---|------|-------------|
| 1 | 01-landing-en-hero.png | Landing page EN - hero section |
| 2 | 02-landing-en-scroll.png | Landing page EN - scrolled to pricing |
| 3 | 03-landing-ar-rtl.png | Landing page AR - RTL layout |
| 4 | 04-portal-booking.png | Owner Portal - booking landing |
| 5 | 05-messages-inbox.png | Messages inbox (logged in as Dr. Sarah Johnson) |
| 6 | 06-messages-conversation.png | Messages - same view (click did not navigate to conversation detail) |
| 7 | 07-messages-ar-rtl.png | Messages AR/RTL |
| 8 | 08-login-mobile.png | Login page - mobile (375x812) |
| 9 | 09-calendar-mobile.png | Calendar/appointments - mobile (Day view) |
| 10 | 10-messages-mobile.png | Messages - mobile (375x812) |

## Findings

### HIGH Severity

- **Messages Mobile** [Responsive]: Horizontal overflow detected on mobile viewport (375x812). The messages page content extends beyond the viewport width, causing horizontal scrolling. Likely caused by the filter tags row (status + category filters) not wrapping properly at narrow widths.

### MEDIUM Severity

- **Messages Conversation** [Navigation]: Clicking a conversation in the inbox did NOT navigate to a conversation detail view. Screenshots 05 and 06 are identical - the inbox stayed on the same view. The right panel shows "Select a conversation to view details" text but clicking a conversation did not populate it.

- **Portal Booking** [UX]: The portal booking page (`/en/portal/desert-paws/book`) shows the portal landing with "My Appointments" and "Select Your Pet" cards, not the booking wizard directly. The 4-step booking wizard (from PR #50) may require navigating through "Select Your Pet" first. The route structure is: `/portal/[clinicSlug]/book` -> landing, `/portal/[clinicSlug]/book/new` -> actual wizard.

- **Messages Inbox** [UX]: The category filter bar (All, Medical Urgency, Post-Op Follow-Up, Medical Question, Appointment, Administrative, Feedback, Other) takes significant horizontal space on desktop. On mobile it wraps to multiple lines consuming ~30% of screen height before any conversation content.

- **Landing AR** [RTL]: The dashboard preview image in the hero section still shows LTR content (English dashboard mockup). While this is a static SVG placeholder, for the AR version it would be more authentic to show an RTL dashboard preview or at minimum mirror the layout. Low priority but noticeable.

- **Messages AR/RTL** [i18n]: The messages content text (conversation previews) appears in English even on the AR locale. The Arabic message from Noura Al-Maktoum is correctly displayed in Arabic, but labels like "Medical Urgency", "Open", "In Progress", "Post-Op Follow-Up" remain in English. These category/status badges should be translated.

### LOW Severity

- **Calendar Mobile** [UX]: The cookie consent banner at the bottom partially obscures the Decline button behind the navigation circle icon (same issue on messages mobile).

## Detailed Page Assessment

### 1. Landing Page EN (Hero)
- **Status**: PASS
- Hero headline "Run Your Veterinary Clinic in Half the Time" renders correctly
- Dashboard preview mockup displays properly with appointment data
- Nav bar has Features, Pricing, FAQ links + EN/AR language switcher + Sign In + Start Free Trial CTAs
- Trust badge "Trusted by 50+ clinics across the UAE" visible
- Social proof bar (clinics, patients, invoiced, uptime) partially visible at bottom

### 2. Landing Page EN (Scroll - Pricing)
- **Status**: PASS
- Pricing section renders with 3 tiers: Starter (299 AED), Pro (599 AED), Enterprise (from 999 AED)
- Monthly/Annual toggle with "Save 17%" badge
- Feature lists properly aligned with checkmarks
- All pricing in AED (correct for UAE market)

### 3. Landing Page AR (RTL)
- **Status**: PASS with minor issues
- `dir="rtl"` and `lang="ar"` correctly set on HTML element
- Arabic headline renders correctly with proper RTL alignment
- Navigation is mirrored (CTA buttons on left, logo on right)
- Dashboard preview shows LTR content (see finding above)

### 4. Owner Portal Booking
- **Status**: PASS (landing page)
- Clean sidebar with Conversations, Appointments, My Pets, Profile
- "Desert Paws Clinic" branding shown
- Language switcher available (Arabic option visible)
- Two action cards: "My Appointments" and "Select Your Pet"

### 5. Messages Inbox
- **Status**: PASS
- 3 conversations visible with MSW mock data
- Realistic UAE names: Ahmed Al-Rashid, Fatima Hassan, Noura Al-Maktoum
- Category badges (Medical Urgency, Post-Op Follow-Up, Medical Question) rendered with color coding
- Status badges (Open, In Progress) visible
- Unread count badges (1, 2) displayed
- Search icon available
- Right panel shows "Select a conversation to view details" placeholder

### 6. Messages Conversation Detail
- **Status**: PARTIAL (click did not navigate to detail)
- Same as inbox view - the conversation click did not trigger navigation or populate the detail panel

### 7. Messages AR/RTL
- **Status**: PASS with i18n gaps
- Layout correctly mirrors to RTL
- Sidebar on right side, content area on left
- User profile (Dr. Sarah Johnson) on left side of header
- Arabic text from Noura Al-Maktoum displays correctly
- Category/status badges remain in English (translation needed)

### 8. Login Mobile (375x812)
- **Status**: PASS
- Clean centered card layout
- EN/AR language switcher visible
- Email and password fields properly sized
- "Forgot password?" link present
- "Sign In" button full-width
- "Don't have an account? Sign Up" link visible
- No horizontal overflow

### 9. Calendar Mobile (375x812)
- **Status**: PASS
- Defaults to Day view (correct mobile behavior)
- Day/Week/Month toggle available
- Date navigation (< Today >) and "New Appointment" button visible
- Time slots (07:00 - 14:00+ visible) render correctly
- "Filter by vet" option available
- Hamburger menu for sidebar navigation

### 10. Messages Mobile (375x812)
- **Status**: FAIL (horizontal overflow)
- Search bar renders properly
- Filter tags wrap to multiple lines but content still overflows
- 3 conversations visible with badges
- Cookie consent banner partially obscured

## Checklist

| Test | Result |
|------|--------|
| Landing EN loads | PASS |
| Landing EN features/pricing visible | PASS |
| Landing AR RTL direction | PASS |
| Landing AR Arabic text | PASS |
| Portal booking loads | PASS |
| Messages inbox loads with MSW data | PASS |
| Messages conversation click | PARTIAL - did not navigate |
| Messages AR/RTL layout | PASS |
| Messages AR translations | PARTIAL - badges untranslated |
| Login mobile responsive | PASS |
| Calendar mobile responsive | PASS |
| Calendar mobile defaults to Day view | PASS |
| Messages mobile responsive | FAIL - horizontal overflow |
| Console errors | None detected |

## Recommended Fixes (Priority Order)

1. **[HIGH] Messages mobile overflow** - Fix the filter tags section to properly wrap or become horizontally scrollable at 375px width. Consider collapsing filters into a dropdown on mobile.
2. **[MEDIUM] Messages conversation click** - Ensure clicking a conversation in the inbox populates the detail panel or navigates to the conversation detail page.
3. **[MEDIUM] Messages AR badge translations** - Translate status badges (Open, In Progress, Closed, Resolved) and category badges (Medical Urgency, Post-Op Follow-Up, etc.) for AR locale.
4. **[MEDIUM] Messages mobile filter density** - The status + category filters take too much vertical space on mobile. Consider a compact filter UI.
5. **[LOW] Landing AR dashboard preview** - Consider using an RTL dashboard image for the Arabic landing page.

## Notes

- Login credentials used: dr.sarah@desertpaws.ae / Secure123!
- MSW authentication works correctly - login redirects to /en/appointments
- All tests run with MSW (Mock Service Worker) intercepting API calls
- Screenshots saved in `src/frontend/e2e/screenshots/qa-full/`
- No console errors detected on any page
