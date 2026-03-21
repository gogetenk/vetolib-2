# todo-back-multi-clinic-001.md — Vue groupe multi-cliniques

**Module** : Auth + Dashboard
**Dépendances** : done-back-signup-selfservice-001
**Priorité** : BASSE (post-MVP, scale)
**Skills à lire** : `ardalis-result`, `multitenant-efcore`

---

## Objectif

Permettre à un owner de gérer plusieurs cliniques depuis un seul compte, avec une vue consolidée.

## Implémentation

### Backend

1. **Domain : ClinicGroup**
   - `ClinicGroup { Id, Name, OwnerUserId }`
   - `ClinicGroupMember { ClinicGroupId, ClinicId }`
   - Un Admin peut lier ses cliniques à un groupe

2. **Endpoints**
   - `POST /api/v1/clinic-groups` — créer un groupe (AdminOnly)
   - `POST /api/v1/clinic-groups/{id}/clinics` — ajouter une clinique au groupe
   - `GET /api/v1/clinic-groups/{id}/dashboard` — stats consolidées cross-cliniques
   - `GET /api/v1/clinic-groups/{id}/clinics` — liste des cliniques du groupe

3. **Switch clinic**
   - L'utilisateur peut switcher de clinique active via un sélecteur dans le header
   - Le JWT est rafraîchi avec le nouveau ClinicId

### Frontend

4. **Sélecteur de clinique dans le header** (si multi-clinic)
5. **Dashboard groupe** — stats agrégées de toutes les cliniques

## Critère

```
□ ClinicGroup entity + endpoints
□ Dashboard consolidé multi-cliniques
□ Sélecteur de clinique dans le header
□ Tests BDD
□ Renommer en done
```
