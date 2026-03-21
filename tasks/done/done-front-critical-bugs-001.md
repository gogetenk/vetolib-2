# todo-front-critical-bugs-001.md — Fix critical functional bugs found by UX review

**Dépendances** : aucune
**Skills** : shadcn-nextjs
**[MSW: oui]**

## Objectif
Fix critical functional bugs that make features unusable.

## Bugs

### 1. [CRITICAL] Conversation detail page broken
- **Screenshot**: messaging/03-message-conversation.png
- **Bug**: Next.js runtime error "Missing `<html>` and `<body>` tags in the root layout" when opening a conversation
- **Fix**: Check route at `src/frontend/app/[locale]/(dashboard)/messages/[id]/page.tsx` — ensure it's nested within a layout that includes root HTML tags
- **Impact**: Entire message reading/replying flow is non-functional

### 2. [CRITICAL] Preferences page shows raw i18n keys
- **Screenshot**: messaging/06-preferences.png
- **Bug**: "preferences.title", "preferences.source.system_default" etc. displayed instead of actual text
- **Fix**: Add all translation entries under `preferences` namespace in `en.json` and `ar.json`
- **Impact**: Preferences page is completely unusable

### 3. [HIGH] Dashboard data not rendering on mobile and Arabic views
- **Screenshots**: dashboard/02-dashboard-mobile.png, dashboard/09-dashboard-ar.png
- **Bug**: Today's Schedule, Recent Activity, Analytics all show grey skeleton placeholders with no data
- **Fix**: Investigate data fetching / hydration timing — skeletons persist instead of real data
- **Impact**: Mobile and Arabic dashboard users see empty page

### 4. [HIGH] Medical Records standalone page is a stub
- **Screenshot**: patients/08-medical-records.png
- **Bug**: "Medical records management coming soon" with fake skeleton bars
- **Fix**: Either implement the page or remove from navigation
- **Impact**: Nav item leads to dead-end page, erodes trust

### 5. [HIGH] Volume per Day chart renders empty
- **Screenshot**: messaging/09-messaging-stats.png
- **Bug**: Chart shows axes but no data series/line
- **Fix**: Debug chart component data binding in Statistics tab
- **Impact**: Key analytics chart is non-functional

### 6. [HIGH] Middleware blocks public pages
- **Discovered by**: Auth screenshot agent
- **Bug**: Landing page (`/en`) and signup (`/en/signup`) redirect unauthenticated users to login
- **Fix**: Add `/` and `/signup` to `isPublicPath()` in `src/frontend/src/middleware.ts`
- **Impact**: New users cannot access landing page or sign up

## Critère de complétion
- [ ] Conversation detail page renders correctly
- [ ] Preferences page shows actual text (not i18n keys)
- [ ] Dashboard renders data on mobile and AR views
- [ ] Medical Records page is functional or removed from nav
- [ ] Volume per Day chart shows data
- [ ] Landing and signup pages accessible without auth
- [ ] `npm run build` passes
