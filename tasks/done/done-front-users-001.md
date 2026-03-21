# todo-front-users-001.md — Frontend : Gestion équipe clinique

**Module** : Frontend / Users
**Dépendances** : done-front-layout-001
[MSW: oui] — développement sans backend requis
[Branchement ultérieur] : done-back-users-001 → crée automatiquement todo-wire-users-001
**Skills à lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`

---

## Périmètre exact

- `app/(dashboard)/settings/team/page.tsx` — liste de l'équipe + actions
- `components/features/users/TeamTable.tsx`
- `components/features/users/InviteUserDialog.tsx`
- `components/features/users/ChangeRoleDialog.tsx`
- `lib/api/users.ts`
- `src/mocks/handlers/users.ts`
- `e2e/users/team.spec.ts`

## UI

### Liste de l'équipe

Tableau avec colonnes : Nom, Email, Rôle (Badge coloré), Statut (Actif/Inactif), Actions

Badge couleurs :
- ADMIN → rouge
- VET → bleu
- ASSISTANT → vert
- RECEPTIONIST → orange

Actions par ligne (ADMIN uniquement) :
- "Change Role" → Dialog avec Select des rôles disponibles
- "Deactivate" → Confirmation dialog (grisé si c'est l'utilisateur courant)

Bouton "Invite Member" (haut droit, ADMIN uniquement — invisible pour les autres rôles)

### Dialog invitation

```
Email *
Nom complet *
Rôle * (Select : VET | ASSISTANT | RECEPTIONIST — pas ADMIN)

[Cancel]  [Send Invite]
```

Après succès : affiche le mot de passe temporaire dans une alerte bien visible
avec bouton "Copy" et message "Share this password securely — it won't be shown again."

## MSW handlers

```typescript
// src/mocks/handlers/users.ts
const MOCK_USERS = [
  { id: 'u-001', email: 'dr.sarah@desertpaws.ae', fullName: 'Dr. Sarah Johnson',
    role: 'VET', isActive: true },
  { id: 'u-002', email: 'reception@desertpaws.ae', fullName: 'Amira Hassan',
    role: 'RECEPTIONIST', isActive: true },
  { id: 'u-003', email: 'admin@desertpaws.ae', fullName: 'Omar Al-Rashid',
    role: 'ADMIN', isActive: true },
]
// GET /api/users, POST /api/users, PATCH /api/users/:id/role, DELETE /api/users/:id
```

## RBAC côté UI

- Page settings/team accessible uniquement si rôle ADMIN
- Sidebar : "Team" visible uniquement pour ADMIN
- Si VET/ASSISTANT/RECEPTIONIST navigue vers /settings/team → redirect /appointments

## Critère de complétion

```
□ Liste équipe avec badges rôles colorés
□ Invitation fonctionne + affichage mot de passe temporaire
□ Changement de rôle fonctionne
□ Désactivation fonctionne (pas sur soi-même)
□ Non-ADMIN ne voit pas la page
□ Tests Playwright verts
□ Renommer en done-front-users-001.md
```
