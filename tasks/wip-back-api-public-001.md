# todo-back-api-public-001.md — API publique + OpenAPI + Webhooks

**Module** : Infra
**Dépendances** : aucune
**Priorité** : BASSE (post-MVP, scale)
**Skills à lire** : `aspnet-minimal-api`

---

## Objectif

Exposer une API publique documentée pour les intégrateurs, avec authentification par API key et webhooks.

## Implémentation

### Backend

1. **OpenAPI / Swagger**
   - Activer Scalar ou SwaggerUI sur `/api/docs`
   - Documenter tous les endpoints existants avec descriptions + exemples
   - Versionning : `/api/v1/...` (déjà en place)

2. **API Keys**
   - `ApiKey { Id, ClinicId, Key (hashed), Name, Scopes, CreatedAt, ExpiresAt, IsActive }`
   - Auth via header `X-Api-Key`
   - Scopes : read:patients, write:patients, read:invoices, write:invoices, etc.
   - Rate limiting : 1000 req/min par API key

3. **Webhooks**
   - `Webhook { Id, ClinicId, Url, Events, Secret, IsActive }`
   - Events supportés : appointment.created, appointment.updated, invoice.created, invoice.paid, patient.created
   - Delivery : POST JSON avec HMAC-SHA256 signature
   - Retry : 3 tentatives avec backoff exponentiel

### Frontend

4. **Page /[locale]/settings/api** (AdminOnly)
   - Créer/révoquer des API keys
   - Configurer des webhooks
   - Voir les logs de delivery

## Critère

```
□ OpenAPI docs sur /api/docs
□ API key auth avec scopes
□ Webhooks avec HMAC signature
□ Page settings/api
□ Rate limiting par API key
□ Tests BDD
□ Renommer en done
```
