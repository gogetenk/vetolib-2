# todo-back-subscription-plans-001.md — Subscription plans + usage check middleware

**Module** : Auth
**Dependencies** : none
**Priority** : high

## Objective
Define subscription plans with feature limits and enforce them via middleware on every paid action.

## Context
Clinic entity already has `SubscriptionPlan` (string) and `TrialEndsAt`. This task enriches the model and adds enforcement.

## Scope
1. Define plan tiers in a `SubscriptionPlan` value object or enum:
   - **Free** : 1 vet, 50 patients, no WhatsApp, no AI triage, 1GB storage
   - **Starter** : 3 vets, 500 patients, basic WhatsApp (50 msgs/month), AI triage, 10GB storage
   - **Pro** : unlimited vets, unlimited patients, full WhatsApp, AI triage + suggestions, 50GB storage
   - **Enterprise** : everything + multi-clinic, API access, priority support, unlimited storage

2. `PlanLimits` record: MaxVets, MaxPatients, MaxWhatsAppMessages, MaxStorageGB, features flags (HasAiTriage, HasWhatsApp, HasMultiClinic, HasApi)

3. `ISubscriptionChecker` interface in Auth.Contracts:
   - `CheckLimit(clinicId, LimitType)` → Result (success or error with upgrade message)
   - `GetCurrentUsage(clinicId)` → UsageDto

4. `SubscriptionCheckMiddleware` (or endpoint filter):
   - Reads `[RequiresPlan("Starter")]` or `[CheckLimit(LimitType.Patients)]` attributes on endpoints
   - Checks clinic's plan against the required tier/limit
   - Returns 403 with upgrade message if exceeded

5. Enrich `Clinic` entity: add `PlanLimits` navigation, usage tracking fields
6. Migration for plan data

## PO decisions
- Trial = 14 days Pro (already in place)
- After trial expires without payment → downgrade to Free
- Soft limits: warn at 80%, block at 100%
- WhatsApp messages counted per calendar month

## Completion criteria
- [ ] Plan tiers defined with limits
- [ ] ISubscriptionChecker in Contracts
- [ ] Middleware/filter enforces limits on endpoints
- [ ] Unit tests for limit checks
- [ ] Build GREEN
