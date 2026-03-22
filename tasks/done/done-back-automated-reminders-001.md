# todo-back-automated-reminders-001.md — Automated appointment + vaccination reminders

**Module** : Notifications + Messaging
**Dependencies** : done-back-whatsapp-001
**Priority** : HIGH (quick win, 30-40% no-show reduction)

## Objective
Automated reminders sent via WhatsApp/email 24h before appointments and for overdue vaccinations.

## Scope
1. Reminder scheduler (background service, runs hourly)
2. Appointment reminder: 24h before, via WhatsApp template + email fallback
3. Vaccination reminder: when vaccination is due (based on next due date in patient record)
4. Follow-up reminder: X days after consultation (configurable per consultation type)
5. Opt-out per owner (respect WhatsApp opt-in)
6. Admin config: enable/disable each reminder type, customize timing
7. Logging: track sent reminders, delivery status

## Completion criteria
- [ ] Appointment reminders sent 24h before
- [ ] Vaccination reminders sent when due
- [ ] Opt-out respected
- [ ] Admin config page
- [ ] Unit tests
- [ ] Build GREEN
