# todo-back-messaging-templates-001.md — CRUD templates de reponse rapide

**Module** : Messaging
**Dependances** : todo-back-messaging-domain-001
**Priorite** : MOYENNE
**Skills a lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`

---

## Objectif

Implementer le CRUD pour les templates de reponse rapide (quick response templates) en EN + AR.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 7 (Staff Endpoints: templates), Appendix A (RBAC)

## Implementation

### Queries

1. **ListTemplatesQuery** + Handler
   - Retourne tous les templates de la clinique
   - Filtre optionnel par categorie
   - Policy : ClinicStaff (tous les staff peuvent lire)

### Commands

2. **CreateTemplateCommand** + Handler + Validator
   - Cree un template avec Name, ContentEn, ContentAr, Category?
   - Policy : AdminOnly

3. **UpdateTemplateCommand** + Handler + Validator
   - Met a jour un template existant
   - Policy : AdminOnly

4. **DeleteTemplateCommand** + Handler
   - Supprime un template
   - Policy : AdminOnly

### Endpoints

```
GET    /api/v1/messaging/templates       → ListTemplatesQuery
POST   /api/v1/messaging/templates       → CreateTemplateCommand
PUT    /api/v1/messaging/templates/{id}  → UpdateTemplateCommand
DELETE /api/v1/messaging/templates/{id}  → DeleteTemplateCommand
```

## Critere

```
[] 1 query + 3 commands implementees
[] 4 endpoints Minimal API
[] RBAC : seul Admin peut creer/modifier/supprimer
[] Templates bilingues EN + AR
[] dotnet build passe
[] Renommer en done
```
