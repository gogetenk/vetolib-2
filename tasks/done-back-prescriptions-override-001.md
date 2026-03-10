# todo-back-prescriptions-override-001 -- Override alertes + audit trail

**Module** : MedicalRecords
**Phase** : 2 (Interaction Checking)
**Dependances** : todo-back-prescriptions-interactions-001
**Branchement ulterieur** : todo-front-prescriptions-alerts-001

## Objectif

Implementer le mecanisme d'override des alertes critiques/moderees avec justification obligatoire et audit trail complet.

## Skills a lire

1. `skills/ardalis-result/SKILL.md`
2. `skills/cqrs-mediatr/SKILL.md`

## Spec de reference

`docs/PRESCRIPTIONS-AI-SPEC.md` -- section 2.2 (Override) et section 4.2 point 6-7

## Scope detaille

### Flow

1. Frontend envoie `AddPrescriptionRequest` avec les alertes reçues
2. Si alertes Critical : `OverrideJustification` est requis (min 10 caracteres)
3. Si alertes Moderate : `OverrideJustification` optionnel, proceed sans blocage
4. Si alertes Info : rien de special

### CQRS

- Modifier `AddPrescriptionHandler` :
  - Avant sauvegarde : appeler `CheckInteractionsQuery` via ISender
  - Si alertes Critical et pas de justification -> retourner `Result.Error("Override justification required for critical alerts")`
  - Si alertes Critical et justification fournie -> sauvegarder avec OverrideJustification + OverrideSeverity
  - Publier `PrescriptionOverriddenEvent` pour audit

### Audit

- Utiliser le systeme d'audit existant (AuditSaveChangesInterceptor dans Shared/)
- L'override doit generer une entree d'audit supplementaire avec :
  - VetId (from UserContext)
  - VetLicenseNumber
  - Timestamp
  - Severity de l'alerte overridee
  - Justification text
  - DrugCatalogEntryId
  - PatientId

### RBAC

- Override uniquement pour roles VET et ADMIN
- ASSISTANT et RECEPTIONIST ne peuvent PAS creer de prescriptions (donc pas d'override non plus)
- Verifier le role dans le handler via IUserContext

## Criteres de completion

- [ ] AddPrescriptionHandler appelle CheckInteractionsQuery avant sauvegarde
- [ ] Blocage si alerte Critical sans justification
- [ ] Sauvegarde avec override si justification fournie
- [ ] Audit trail genere pour chaque override
- [ ] RBAC : seuls VET et ADMIN peuvent override
- [ ] OverrideJustification minimum 10 caracteres
- [ ] Unit tests : override avec justification valide
- [ ] Unit tests : rejet sans justification sur alerte critique
- [ ] Unit tests : ASSISTANT ne peut pas creer de prescription
