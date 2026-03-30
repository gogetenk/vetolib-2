# Notifications & Email Templates Audit — 2026-03-29

## Executive Summary

The Vetolib notification system is structured across two modules: **Notifications** (email sending, reminders, delivery logging) and **Messaging** (WhatsApp, owner portal, response templates). The system has solid architectural foundations (MassTransit consumers, Polly resilience, Ardalis.Result) but has significant gaps in localization usage, HTML email safety, clinic branding, and several notification types are stub-only (log but never send).

---

## 1. Inventory of All Notification Types

### 1a. Fully Implemented (Email Actually Sent)

| # | Type | Consumer | Template (C#) | HTML Template | Status |
|---|------|----------|---------------|---------------|--------|
| 1 | Appointment Reminder (24h) | `AppointmentReminderConsumer` | `ReminderEmailTemplate` | `appointment-reminder.{en,ar}.html` | SENDS email |
| 2 | Invoice Sent | `InvoiceSentConsumer` | `InvoiceEmailTemplate` | `invoice-sent.{en,ar}.html` | SENDS email |
| 3 | User Invitation | `UserInvitedConsumer` | `InvitationEmailTemplate` | `user-invited.{en,ar}.html` | SENDS email |
| 4 | Magic Link (Owner Portal) | `SendMagicLinkConsumer` | `MessagingEmailTemplates.MagicLink` | NONE (C# inline only) | SENDS email |

### 1b. Stub-Only (Consumer Exists, Logs Only, Never Sends)

| # | Type | Consumer | Template (C#) | Reason |
|---|------|----------|---------------|--------|
| 5 | Owner Message Reply | `OwnerMessageReplyConsumer` | `MessagingEmailTemplates.OwnerMessageReply` | Missing owner email — needs Auth module enrichment |
| 6 | Emergency Message Received | `EmergencyMessageReceivedConsumer` | `MessagingEmailTemplates.EmergencyMessageReceived` | Missing vet email list — needs Auth module query |
| 7 | Emergency Escalation (10min) | `EmergencyEscalationConsumer` | `MessagingEmailTemplates.EmergencyEscalation` | Missing vet email list — needs Auth module query |
| 8 | Outbound Conversation Created | `OutboundConversationCreatedConsumer` | `MessagingEmailTemplates.OutboundConversation` | Missing owner email — needs Auth module enrichment |

### 1c. Defined in Enums/Config but No Consumer

| # | Type | Enum Value | Status |
|---|------|-----------|--------|
| 9 | Vaccination Due Reminder | `ReminderType.VaccinationDue` | `ReminderSchedulerService` has placeholder logic only — no vaccination tracking entity exists yet |
| 10 | Follow-Up Reminder | `ReminderType.FollowUp` | Defined in `ReminderType` enum, config toggle exists in `ReminderConfig`, but zero implementation |

### 1d. Events Published with No Notification Consumer

| # | Event | Module | Should Notify? |
|---|-------|--------|----------------|
| 11 | `StockLowEvent` | Stock | YES — clinic staff should be alerted when inventory is low |
| 12 | `StockExpiringEvent` | Stock | YES — clinic staff should be alerted before drugs expire |
| 13 | `StockInsufficientForPrescriptionEvent` | Stock | YES — prescribing vet should know immediately |
| 14 | `UrgentMessageClassifiedEvent` | Messaging | Consumed by Messaging module itself, not Notifications |

### 1e. WhatsApp (Messaging Module)

| Item | Status |
|------|--------|
| WhatsApp Business API integration | Implemented via `WhatsAppSender` (Meta Graph API v21.0) |
| WhatsApp template sending | Implemented — uses Meta pre-approved templates by name |
| WhatsApp config endpoints | GET/PUT config, POST test — Admin-only |
| WhatsApp message templates (content) | NOT stored in code — relies on templates registered in Meta Business Manager |
| WhatsApp used for notifications? | NO — only used for Messaging module conversations, not for appointment reminders or invoices |

---

## 2. Quality Assessment Per Notification

### 2.1 Appointment Reminder

**C# template (`ReminderEmailTemplate`):**
- Content is professional and clear
- ISSUE: English-only — no Arabic variant in C# code
- ISSUE: Does not use the HTML template files at all — the consumer calls `ReminderEmailTemplate.HtmlBody()` which returns inline HTML fragments (not the full `appointment-reminder.en.html`)
- ISSUE: No clinic logo/branding placeholder
- ISSUE: No CTA button (e.g., "Reschedule" link)
- ISSUE: Hardcoded "tomorrow" — does not adapt if lead time is changed from 24h

**HTML template (`appointment-reminder.{en,ar}.html`):**
- Well-structured, table-based layout (email-safe)
- Has AR version with `dir="rtl"` (good)
- Uses `{{mustache}}` placeholders but NO code loads these files — they appear to be dead/unused
- No clinic logo placeholder
- No unsubscribe link
- No preheader text

### 2.2 Invoice Sent

**C# template (`InvoiceEmailTemplate`):**
- Professional content, includes amount and currency
- ISSUE: English-only in C# code
- ISSUE: Says "Please find attached your invoice" but `EmailMessage` record has no attachment support — misleading copy
- ISSUE: No payment link or CTA
- ISSUE: No clinic logo

**HTML template (`invoice-sent.{en,ar}.html`):**
- Table-based, email-safe
- AR version with RTL support
- Same dead-template issue — not loaded by any code

### 2.3 User Invitation

**C# template (`InvitationEmailTemplate`):**
- SECURITY CONCERN: Sends temporary password in plaintext email — industry best practice is a one-time setup link
- ISSUE: English-only in C# code
- Content is otherwise clear and professional
- No CTA button ("Log In Now")
- No clinic logo

**HTML template (`user-invited.{en,ar}.html`):**
- Same security concern — password in plaintext
- Table-based, email-safe
- AR version present
- Dead template — not loaded by code

### 2.4 Magic Link

**C# template (`MessagingEmailTemplates.MagicLink`):**
- Good security practices: mentions single-use, limited time
- Clear CTA with link
- ISSUE: No HTML template file — only inline C# HTML fragments (no `<table>` wrapper, no full HTML doc)
- ISSUE: English-only
- ISSUE: No clinic name or branding — completely generic
- ISSUE: Link is a raw `<a>` tag, not a styled button

### 2.5 Owner Message Reply

**C# template (`MessagingEmailTemplates.OwnerMessageReply`):**
- Content is appropriate
- ISSUE: NEVER SENT — consumer is stub-only
- ISSUE: English-only
- ISSUE: No owner name personalization ("Hello," instead of "Hello {Name},")
- ISSUE: No clinic name in body

### 2.6 Emergency Message Received / Escalation

**C# templates:**
- Appropriate urgency tone, clear escalation language
- ISSUE: NEVER SENT — both consumers are stub-only
- ISSUE: English-only
- ISSUE: Exposes raw `Guid` conversationId to recipient — not user-friendly
- ISSUE: No direct link to the conversation in the clinic dashboard

### 2.7 Outbound Conversation Created

**C# template (`MessagingEmailTemplates.OutboundConversation`):**
- Good content with message preview and portal link
- ISSUE: NEVER SENT — consumer is stub-only
- ISSUE: English-only
- ISSUE: No owner name personalization

---

## 3. Cross-Cutting Issues

### 3.1 Localization

| Issue | Severity | Details |
|-------|----------|---------|
| HTML templates exist in EN+AR but are DEAD CODE | HIGH | All 6 `.html` files in `Templates/Html/` are never loaded. Consumers use C# string interpolation templates instead. |
| C# templates are English-only | HIGH | All C# template classes produce English text only. The `PreferredLanguage` field exists on events (`AppointmentReminderEvent`, `InvoiceSentEvent`, `UserInvitedEvent`) but is NEVER READ by any consumer. |
| No language selection logic | HIGH | Zero code path selects AR vs EN based on user/owner preference. |

### 3.2 Clinic Branding

| Issue | Severity | Details |
|-------|----------|---------|
| No clinic logo in any email | MEDIUM | All emails are plain text + basic HTML. No `<img>` tag for clinic logo. |
| Clinic name is included | OK | `{{ClinicName}}` is present in most templates. |
| No clinic colors/theme | LOW | All emails use hardcoded `#2c7a4b` green. No per-clinic theming. |
| No clinic contact info | MEDIUM | No phone number, address, or website in email footer. |

### 3.3 HTML Email Safety

| Issue | Severity | Details |
|-------|----------|---------|
| HTML template files use table layout | OK | The `.html` files are email-safe (no CSS grid, no flexbox). |
| C# inline templates are FRAGMENTS | HIGH | C# templates return `<p>` fragments without `<html>`, `<body>`, `<table>` wrapper. These will render inconsistently across email clients. |
| No `<!DOCTYPE>` in C# templates | HIGH | Only the dead HTML files have proper DOCTYPE. |
| No responsive meta viewport in C# output | MEDIUM | C# templates lack mobile-friendly viewport. |
| No MSO conditionals for Outlook | LOW | No Outlook-specific CSS. Acceptable for UAE market if Outlook usage is low. |

### 3.4 CTAs and Links

| Issue | Severity | Details |
|-------|----------|---------|
| Magic Link has clickable link | OK | `<a href>` present |
| Appointment Reminder has no CTA | MEDIUM | No "Reschedule" or "View Appointment" link |
| Invoice has no payment link | MEDIUM | No "Pay Now" or "View Invoice" link |
| User Invitation has no "Log In" button | MEDIUM | Just text, no styled CTA button |

### 3.5 Email Infrastructure

| Component | Status | Notes |
|-----------|--------|-------|
| `IEmailSender` interface | OK | Clean Ardalis.Result-based contract |
| `SmtpEmailSender` (Shared) | OK | Basic System.Net.Mail implementation |
| `MailKitEmailSender` (Notifications) | GOOD | MailKit with Polly retry (2 attempts) + circuit breaker (3 failures / 30s). Production-ready. |
| `ConsoleEmailSender` | OK | Dev-friendly fallback |
| `EmailServiceExtensions` | ISSUE | Registers `SmtpEmailSender` from Shared, but `MailKitEmailSender` is registered separately in Notifications module. Two competing SMTP senders in DI? Last registration wins. |
| From address consistency | ISSUE | `SmtpEmailOptions` defaults to `noreply@vetolib.ae`, `MailKitSmtpOptions` defaults to `noreply@desertpaws.ae`. Inconsistent defaults. |
| PlainText alternative | OK | All templates provide both HTML and plaintext body |
| `EmailMessage` record | ISSUE | No attachment support — but invoice template says "find attached" |

### 3.6 Delivery Tracking

| Component | Status | Notes |
|-----------|--------|-------|
| `ReminderLog` entity | OK | Tracks type, channel, status, recipient |
| `DeliveryStatus` enum | OK | Pending/Sent/Failed |
| Logging in AppointmentReminderConsumer | OK | Creates `ReminderLog` entry |
| Logging in other consumers | MISSING | Invoice, Invitation, MagicLink consumers do not create `ReminderLog` entries |

---

## 4. Missing Notifications That Should Exist

| # | Notification Type | Priority | Recipient | Notes |
|---|-------------------|----------|-----------|-------|
| 1 | **Booking Confirmation** | HIGH | Pet Owner | When an appointment is created — owner should receive confirmation with date, time, vet name |
| 2 | **Appointment Cancellation** | HIGH | Pet Owner | When clinic or owner cancels — confirmation email |
| 3 | **Appointment Rescheduled** | MEDIUM | Pet Owner | When date/time changes |
| 4 | **Password Reset** | HIGH | Staff User | Standard auth flow — currently only temporary password on invite |
| 5 | **Stock Low Alert** | MEDIUM | Clinic Admin | `StockLowEvent` exists but has no notification consumer |
| 6 | **Stock Expiring Alert** | MEDIUM | Clinic Admin | `StockExpiringEvent` exists but has no notification consumer |
| 7 | **Prescription Stock Warning** | LOW | Prescribing Vet | `StockInsufficientForPrescriptionEvent` exists but no notification |
| 8 | **New Patient Registration Confirmation** | LOW | Pet Owner | Welcome email when a pet is registered |
| 9 | **Payment Receipt** | MEDIUM | Pet Owner | After payment is processed (distinct from invoice) |
| 10 | **Vaccination Due Reminder** | HIGH | Pet Owner | `ReminderType.VaccinationDue` defined but unimplemented |
| 11 | **Follow-Up Reminder** | MEDIUM | Pet Owner | `ReminderType.FollowUp` defined but unimplemented |
| 12 | **WhatsApp Appointment Reminder** | HIGH | Pet Owner | WhatsApp integration exists but is not used for reminders |

---

## 5. Summary of Findings

### Critical Issues (Must Fix)
1. **HTML template files are dead code** — 6 localized HTML templates exist but are never loaded. C# templates are used instead.
2. **Zero localization in practice** — `PreferredLanguage` field on events is never read. All emails are English-only despite Arabic HTML templates existing.
3. **C# templates produce HTML fragments, not complete emails** — Will render broken in many email clients.
4. **4 of 8 consumers are stubs** — Emergency, reply, and outbound notifications log only and never send. Auth module integration is the blocker.
5. **Inconsistent from-address defaults** — `noreply@vetolib.ae` vs `noreply@desertpaws.ae` between two SMTP sender implementations.

### Medium Issues
6. No clinic logo or footer in any email.
7. No CTA buttons in reminder, invoice, or invitation emails.
8. Invoice template says "find attached" but `EmailMessage` has no attachment support.
9. User invitation sends temporary password in plaintext — should use a setup link.
10. No delivery tracking for invoice, invitation, or magic link emails.
11. Stock module events have no notification consumers.
12. Vaccination and follow-up reminder types are defined but have zero implementation.

### What Works Well
- Architectural pattern is solid: MassTransit consumers + Ardalis.Result + Polly resilience.
- `MailKitEmailSender` has production-grade retry + circuit breaker.
- PlainText alternatives exist for all templates.
- `ReminderConfig` entity allows per-clinic toggle of reminder types.
- WhatsApp Business API integration is functional for the Messaging module.
- `ResponseTemplate` domain entity enforces EN+AR content for messaging templates.

---

## Files Examined

- `src/backend/Modules/Notifications/Vetolib.Notifications/Templates/Html/*.html` (6 files)
- `src/backend/Modules/Notifications/Vetolib.Notifications/Templates/*.cs` (4 files)
- `src/backend/Modules/Notifications/Vetolib.Notifications/Consumers/*.cs` (8 files)
- `src/backend/Modules/Notifications/Vetolib.Notifications/Infrastructure/MailKitEmailSender.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications/Infrastructure/ReminderSchedulerService.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications/NotificationsModuleServiceRegistrar.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications/Domain/ReminderConfig.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications.Contracts/Enums/*.cs`
- `src/backend/Modules/Notifications/Vetolib.Notifications.Contracts/Events/*.cs`
- `src/backend/Shared/Vetolib.Shared.Kernel/IEmailSender.cs`
- `src/backend/Shared/Vetolib.Shared.Infrastructure/Email/*.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Infrastructure/WhatsAppSender.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Api/WhatsAppEndpoints.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/ResponseTemplate.cs`
- `src/backend/Modules/Stock/Vetolib.Stock.Contracts/Stock*Event.cs`
- `src/backend/Vetolib.Api/appsettings.Development.json`
