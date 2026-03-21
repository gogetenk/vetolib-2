# todo-cleanup-dead-dashboard-routes-001.md

**Module** : Frontend
**Priorité** : MOYENNE
**Skills à lire** : `skills/shadcn-nextjs/SKILL.md`

---

## Objectif

Supprimer l'ancienne arborescence `app/(dashboard)` qui est entièrement remplacée par `app/[locale]/(dashboard)`.

## Contexte

Le frontend possède deux arborescences de routes pour le dashboard :

**Ancienne (non-locale) :** `src/frontend/src/app/(dashboard)/`
- 14 pages : appointments, billing, dashboard, medical-records, patients, settings/team

**Nouvelle (locale-aware) :** `src/frontend/src/app/[locale]/(dashboard)/`
- 24 pages : toutes les pages de l'ancienne + messages, stock, settings/messaging, settings/preferences
- Ajoute error.tsx, loading.tsx

La nouvelle arborescence est la version canonique (i18n via next-intl). L'ancienne est du code mort
qui génère une confusion sur la structure du projet et peut causer des conflits de routes Next.js.

## Actions

1. Supprimer récursivement `src/frontend/src/app/(dashboard)/`.
2. Supprimer récursivement `src/frontend/src/app/(auth)/` si elle est aussi dupliquée sous `[locale]/`.
3. Vérifier que `src/frontend/src/app/[locale]/(auth)/` couvre bien login/signup.
4. Lancer `npm run build` dans `src/frontend/` → 0 erreurs TypeScript.
5. Vérifier que `npm run dev` démarre sans erreurs de route en double.

## Vérification préalable

Avant de supprimer, confirmer que `app/[locale]/(dashboard)/` contient toutes les routes
présentes dans l'ancienne arborescence :

```
□ appointments + [id] + new
□ billing + [id] + new
□ dashboard
□ medical-records
□ patients + [id] + [id]/records/new + new
□ settings/team
```

## Critères de complétion

```
□ app/(dashboard)/ supprimé
□ Pas de routes en double signalées par Next.js au démarrage
□ npm run build → 0 erreurs TypeScript
```
