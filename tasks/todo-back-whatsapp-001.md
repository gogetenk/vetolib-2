# todo-back-whatsapp-001.md — WhatsApp Cloud API integration (Phase 1 — outbound only)

**Module** : Messaging
**Dependencies** : none
**Priority** : medium

## Objective
Integrate WhatsApp Business Cloud API for outbound notifications (appointment reminders, invoice sent, etc.)

## Scope
1. `IChannelDispatcher` interface in Messaging.Contracts
2. `WhatsAppSender` implementation using Meta Graph API
3. `WhatsAppBusinessAccount` entity (WABA ID + token per clinic, multi-tenant)
4. `WhatsAppPhoneMapping` entity (E.164 phone → OwnerId, opt-in date for PDPL)
5. `ConversationChannel` enum (Portal, WhatsApp, Sms) on Conversation entity + migration
6. Template message support (pre-approved by Meta):
   - Appointment reminder (24h before)
   - Appointment confirmation
   - Invoice sent notification
7. Admin settings page: enable/disable WhatsApp, configure WABA credentials
8. Rate limiting per clinic (respect Meta's rate limits)

## NOT in scope (Phase 2)
- Inbound messages from WhatsApp
- Bidirectional conversations
- Media messages

## Pre-requisites (non-dev)
- Meta Business Verification (2-5 days) — start 1 month before dev

## Completion criteria
- [ ] WhatsAppSender sends template messages via Graph API
- [ ] Phone mapping with opt-in tracking
- [ ] Admin can configure WABA credentials
- [ ] Appointment reminder trigger working
- [ ] Unit tests
- [ ] Build GREEN
