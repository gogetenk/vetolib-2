# todo-front-layout-001.md — Frontend : Layout Dashboard + Navigation

**Module** : Frontend / Shell
**Dépendances** : front-scaffold-000
**Skills à lire** : `shadcn-nextjs`

---

## Périmètre exact

- `app/(dashboard)/layout.tsx` — layout principal avec sidebar + header
- `components/features/shell/Sidebar.tsx`
- `components/features/shell/Header.tsx`
- `components/features/shell/UserMenu.tsx`

Ce composant est partagé par toutes les pages dashboard.
À faire tôt car toutes les autres tâches frontend en dépendent visuellement.

---

## Layout

```
┌─────────────────────────────────────────────────┐
│ Header : Logo Vetolib         [UserMenu ▼]       │
├──────────────┬──────────────────────────────────┤
│              │                                  │
│   Sidebar    │   {children}                     │
│              │                                  │
│ 📅 Agenda    │                                  │
│ 🐾 Patients  │                                  │
│ 📋 Medical   │                                  │
│ 💰 Billing   │                                  │
│              │                                  │
│ (bottom)     │                                  │
│ ⚙ Settings  │                                  │
│ 👤 Profile   │                                  │
└──────────────┴──────────────────────────────────┘
```

---

## Sidebar

- Composant shadcn `Sidebar` (ou NavigationMenu vertical)
- Items avec icônes Lucide + label
- Item actif : highlighted (bg-primary/10 ou variant actif)
- Collapsible sur mobile (Sheet shadcn)
- Affichage conditionnel selon le rôle :
  - RECEPTIONIST : pas de "Medical Records"
  - ASSISTANT : Medical Records en lecture seule (badge "Read only")

---

## Header

- Logo "Vetolib" (texte + icône patte) à gauche
- À droite : UserMenu (voir ci-dessous)
- Responsive : hamburger menu sur mobile

---

## UserMenu

Dropdown shadcn DropdownMenu :
- Avatar initiales (cercle coloré)
- Nom complet + rôle
- Séparateur
- "Settings" (lien)
- "Sign Out" (appelle logout, redirect /login)

---

## Critère de complétion

```
□ Layout visible sur toutes les pages dashboard
□ Navigation active correctement highlightée
□ UserMenu fonctionne (logout)
□ Responsive mobile (sidebar collapse)
□ RBAC : RECEPTIONIST ne voit pas "Medical Records"
□ Renommer en done-front-layout-001.md
```
