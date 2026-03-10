# todo-back-signup-selfservice-001.md — Inscription clinique self-service

**Module** : Auth + Infra
**Dépendances** : aucune
**Priorité** : HAUTE (post-MVP, acquisition)
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`

---

## Objectif

Permettre à un nouveau client de créer sa clinique et son compte admin via un formulaire public, sans intervention manuelle.

## Implémentation

### Backend

1. **POST /api/v1/clinics/register** (public, pas d'auth)
   - Input : `{ clinicName, email, password, phone, country: "AE" }`
   - Crée un nouveau tenant (ClinicId)
   - Crée l'utilisateur Admin pour ce tenant
   - Retourne un JWT pour connexion immédiate
   - Rate-limité : 3 créations/heure par IP

2. **Domain : Clinic entity**
   - `Clinic { Id, Name, SubscriptionPlan, TrialEndsAt, CreatedAt }`
   - Trial de 14 jours (plan Pro)
   - Stocké dans une table cross-tenant (pas de filtre ClinicId)

3. **Validation**
   - Email unique cross-tenant
   - Password policy (10 chars + special)
   - ClinicName non vide

### Frontend

4. **Page /[locale]/signup**
   - Formulaire : nom clinique, email, mot de passe, téléphone
   - Redirection vers /[locale]/dashboard après inscription
   - i18n EN + AR

## Critère

```
□ POST /api/v1/clinics/register fonctionne
□ Nouveau tenant créé avec isolation
□ Trial 14 jours plan Pro
□ Page /signup avec formulaire
□ i18n EN + AR
□ Rate limiting 3/h par IP
□ Tests BDD (Reqnroll) : inscription OK, email dupliqué, validation
□ Renommer en done
```
