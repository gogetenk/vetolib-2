# QA Report -- Messaging and Settings

**Branch**: develop
**Timestamp**: 2026-03-12
**Status**: [QA_FAIL]

---

## Test Results

- **Passed**: 102
- **Skipped**: 34
- **Failed**: 0

---

## Coverage Analysis

### Pages with Playwright coverage

| Page | Spec file | Coverage level |
|---|---|---|
| /messages (inbox) | inbox.spec.ts | Good -- RBAC, filters, search, unread badge |
| /messages/[id] (conversation) | conversation.spec.ts | Good -- 13 scenarios covering all actions |
| /settings/team | users/team.spec.ts | Good -- invite, deactivate, change role, RBAC |
| /settings/preferences | preferences.spec.ts | Good -- 22 scenarios across all sections |
| /settings/messaging/templates | admin-settings.spec.ts | Partial -- CRUD covered, no category=none edge case |
| /settings/messaging/hours | admin-settings.spec.ts | Partial -- save success only, no load-error scenario |
| /settings/messaging/stats | admin-settings.spec.ts | Partial -- display only, no empty state |

### Pages without Playwright coverage

- MessagingSseProvider (SSE reconnect logic) -- no test
- AttachmentPreview / lightbox in MessageBubble -- no test
- DayHoursRow individual behavior (disabled state propagation) -- no test
- ConversationSummary collapse/expand -- no test

---

## Bugs Found

### BUG-001 [BLOCKING] -- TemplateFormDialog sends string value to API instead of null for empty category

**File**: src/components/features/messaging/admin/TemplateFormDialog.tsx line 84

When the user selects No category in the template form, the SelectItem has value equal to the string "none".
The submit handler uses (category as MessageCategory) || null to convert back to null.
Since the string "none" is truthy, this expression evaluates to the string "none" and sends an invalid value to the API instead of null.

Reproduction: Open Templates page > Create or edit template > leave Category as No category > Save.
Network request body will contain category: "none" instead of category: null.

Fix direction: Replace the truthy check with an explicit comparison: category === "none" ? null : (category as MessageCategory).

---

### BUG-002 [BLOCKING] -- ASSISTANT role gets access to medical message categories

**File**: src/components/features/messaging/MessagesPage.tsx lines 34-42

ROLE_CATEGORIES.ASSISTANT is set to the full list including MedicalUrgency, PostOperativeFollowUp, and MedicalQuestion.
The spec states ASSISTANT should only see non-medical conversations in read-only. This grants ASSISTANT the same
category visibility as VET/ADMIN, bypassing RBAC.

Fix direction: ASSISTANT category list should be limited to AppointmentRequest, Administrative, Feedback, Other.
The component should also enforce read-only mode (no send/reply actions) for the ASSISTANT role.

---

### BUG-003 [BLOCKING] -- Playwright team tests use wrong URL paths (missing /en/ locale prefix)

**File**: e2e/users/team.spec.ts lines 50 and 59

Two toHaveURL assertions use /appointments and /settings/team without the /en/ locale prefix.
All other spec files use /en/ prefixed URLs. These assertions will always fail in a Next.js i18n
routing context where the locale prefix is mandatory.

Line 50 checks for /appointments -- should check for /en/appointments
Line 59 checks for /settings/team -- should check for /en/settings/team

---

### BUG-004 [NON-BLOCKING] -- MessagingHoursPage silently swallows load errors

**File**: src/components/features/messaging/admin/MessagingHoursPage.tsx

The loadHours() function has an empty catch block with only a comment. If the API call fails,
the page silently shows default hours and the user has no indication that their saved configuration
was not loaded. Any edits made on a failed-load state will overwrite real saved data with defaults.

Fix direction: Add error state and display a visible warning banner when the load fails.

---

### BUG-005 [NON-BLOCKING] -- Dead UI: inline ConversationDetail placeholder in MessagesPage

**File**: src/components/features/messaging/MessagesPage.tsx

The component renders a placeholder div in the right panel but handleSelect immediately calls
router.push which navigates away before the placeholder is ever visible. The placeholder and
the selectedId state are dead code in the current routing-based navigation model.

Fix direction: Remove the inline placeholder and selectedId state, or document the intent for
a future split-pane layout.

---

## Missing data-testid

| Component | Missing element | Impact |
|---|---|---|
| MessagingHoursPage error state | No testid on error/warning banner | Cannot assert error state in tests |
| ConversationSummary collapse button | No testid on toggle trigger | Cannot test expand/collapse flow |
| AttachmentPreview lightbox | No testid on lightbox overlay or close button | No testability for attachment viewing |
| ReplyComposer add-to-record-btn | testid present but button is a no-op | Test would pass but action does nothing |

---

## i18n Issues

### i18n-001 -- TeamTable hardcoded English strings

**File**: src/components/features/users/TeamTable.tsx

All table headers, button labels (Change Role, Deactivate, Deactivating...), status badges (Active, Inactive),
and toast messages use hardcoded English strings. No useTranslations hook is used.
This blocks FR and PL market expansion.

---

### i18n-002 -- InviteUserDialog and ChangeRoleDialog hardcoded English strings

**Files**: src/components/features/users/InviteUserDialog.tsx, ChangeRoleDialog.tsx

All dialog titles, field labels, button text, and success/error messages are hardcoded English strings.
No useTranslations hook is used.

---

### i18n-003 -- ConversationListItem relative date formatting hardcoded in English

**File**: src/components/features/messaging/ConversationListItem.tsx

formatRelativeDate uses hardcoded English suffixes: m ago, h ago, d ago.
These strings will not translate to AR or FR locales.

---

### i18n-004 -- MessageBubble relative time formatting hardcoded in English

**File**: src/components/features/messaging/MessageBubble.tsx

formatRelativeTime uses hardcoded English strings: just now, minute ago, minutes ago, hour ago, hours ago.

---

### i18n-005 -- TemplatesPage date formatting without locale or timezone

**File**: src/components/features/messaging/admin/TemplatesPage.tsx

toLocaleDateString() is called without a locale argument and without timezone (Asia/Dubai).
Should use toLocaleDateString with en-AE locale and Asia/Dubai timezone option.

---

## Accessibility Issues

### a11y-001 -- ConversationActions custom menu has no keyboard navigation

**File**: src/components/features/messaging/ConversationActions.tsx

The component uses role=menu and role=menuitem but implements no keyboard event handlers.
There is no Escape key handler to close the menu, no arrow key navigation between items,
and no aria-labelledby linking the menu to its trigger button.
This fails WCAG 2.1 SC 2.1.1 (Keyboard).

---

### a11y-002 -- ConversationSummary collapsed content remains keyboard-focusable

**File**: src/components/features/messaging/ConversationSummary.tsx

The collapse animation uses max-h-0 overflow-hidden CSS with aria-hidden on the content container,
but does not add the inert attribute. Focusable elements inside the collapsed summary remain in the
tab order, allowing keyboard users to reach hidden interactive elements.
Should use the inert attribute or visibility: hidden in addition to aria-hidden.

---

## MSW Handler Gaps

| Gap | Description |
|---|---|
| No error scenario for GET /api/v1/messaging/hours | MessagingHoursPage silent-fail path (BUG-004) is untestable without an error handler variant |
| No handler for template category null | The null vs none category bug (BUG-001) has no MSW test coverage |
| No handler for ASSISTANT role conversations | RBAC category filtering for ASSISTANT is not validated by a dedicated MSW scenario |
| No handler for SSE endpoint | MessagingSseProvider has no mock, SSE reconnect behavior is untestable |

---

## Recommendations (non-blocking)

1. Extract formatRelativeDate and formatRelativeTime into a shared lib/utils/date.ts with locale and timezone support. Apply to ConversationListItem and MessageBubble.
2. Add useTranslations to TeamTable, InviteUserDialog, and ChangeRoleDialog as part of the next i18n pass.
3. Add a dedicated MSW handler override for the hours load-error scenario and write a test asserting the error banner is displayed.
4. Add inert attribute support to ConversationSummary for proper keyboard accessibility.
5. Implement keyboard navigation (arrow keys + Escape) in ConversationActions to reach WCAG 2.1 compliance.
6. Add tests for AttachmentPreview lightbox open/close behavior.
7. Remove dead selectedId state and inline placeholder from MessagesPage, or add a comment explaining the planned split-pane design.

---

## Blocking Issues Summary

| ID | File | Issue | Blocker |
|---|---|---|---|
| BUG-001 | TemplateFormDialog.tsx:84 | Sends string none to API instead of null for empty category | Yes |
| BUG-002 | MessagesPage.tsx:34-42 | ASSISTANT role gets medical categories -- RBAC violation | Yes |
| BUG-003 | team.spec.ts:50,59 | Missing /en/ locale prefix in URL assertions -- tests will fail | Yes |
| BUG-004 | MessagingHoursPage | Silent error swallowing on load -- UX data loss risk | No |
| BUG-005 | MessagesPage | Dead placeholder UI and dead selectedId state | No |
| i18n-001 | TeamTable | No i18n -- hardcoded English strings | No |
| i18n-002 | InviteUserDialog, ChangeRoleDialog | No i18n -- hardcoded English strings | No |
| i18n-003 | ConversationListItem | Hardcoded relative date suffixes | No |
| i18n-004 | MessageBubble | Hardcoded relative time strings | No |
| i18n-005 | TemplatesPage | toLocaleDateString() without locale/timezone | No |
| a11y-001 | ConversationActions | No keyboard navigation on custom menu | No |
| a11y-002 | ConversationSummary | Collapsed content keyboard-focusable via missing inert | No |

**Status: [QA_FAIL]** -- 3 blocking issues must be resolved before this zone can be marked QA_PASS.
