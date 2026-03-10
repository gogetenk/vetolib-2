# todo-back-change-password-001.md — Changement de mot de passe

**Module** : Auth
**Dépendances** : aucune
**Priorité** : BLOQUANT (PO review #1)
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`

---

## Contexte

Les utilisateurs invités reçoivent un mot de passe temporaire mais ne peuvent pas le changer. C'est inutilisable en clinique réelle.

## Périmètre

### Backend
- `POST /api/auth/change-password` — authentifié, prend `{ currentPassword, newPassword }`
- Validation : nouveau mot de passe 8+ chars, 1 uppercase, 1 digit (mêmes règles que création)
- Erreur si currentPassword incorrect → 400
- Après changement : invalider tous les refresh tokens existants (force re-login)

### Frontend
- Page `/settings/profile` avec formulaire changement de mot de passe
- Champs : mot de passe actuel, nouveau mot de passe, confirmation
- Validation Zod côté client
- Toast succès + redirect

## Critère
```
□ Endpoint change-password fonctionnel
□ Ancien mot de passe vérifié
□ Refresh tokens invalidés après changement
□ Page profile avec formulaire
□ Tests unitaires + Playwright
□ Renommer en done
```
