# todo-front-whatsapp-001.md — WhatsApp admin settings UI

**Module** : Messaging (frontend)
**Dependencies** : done-back-whatsapp-001
**Priority** : medium
**MSW** : oui

## Objective
Admin page to configure WhatsApp integration.

## Scope
1. New settings sub-page: /settings/messaging/whatsapp
2. Enable/disable WhatsApp toggle
3. WABA credentials form (Business Account ID, Phone Number ID, Access Token)
4. Test connection button (sends test template to admin's phone)
5. Opt-in status overview (how many owners have opted in)
6. MSW handlers for WhatsApp config endpoints

## Completion criteria
- [ ] Settings page functional
- [ ] Test connection works
- [ ] MSW handlers
- [ ] data-testid on all elements
- [ ] Build GREEN
