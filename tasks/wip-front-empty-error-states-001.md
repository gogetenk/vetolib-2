# todo-front-empty-error-states-001.md — Empty states, loading states, error states

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `shadcn-nextjs`

---

## Objectif

Chaque page/composant doit gérer 4 états : loading, empty, error, data. Actuellement beaucoup de pages n'ont que l'état "data".

## Pages à vérifier

Pour chaque page :
1. **Loading** : skeleton loader ou spinner pendant le fetch
2. **Empty** : illustration + message + CTA quand la liste est vide
3. **Error** : message d'erreur clair + bouton retry
4. **Data** : le contenu normal

### Dashboard
- Loading : skeletons pour les KPI cards et les graphiques
- Empty : "No data yet — create your first appointment"
- Error : "Failed to load dashboard — Retry"

### Patients
- Loading : skeleton table
- Empty : illustration + "No patients yet — Add your first patient"
- Error : retry button

### Appointments
- Loading : skeleton list
- Empty : "No appointments scheduled — Book one now"
- Error : retry

### Billing
- Loading : skeleton table
- Empty : "No invoices yet — Create your first invoice"
- Error : retry

## Pattern à utiliser

```tsx
// Composant réutilisable
<EmptyState
  icon={<Users className="h-12 w-12 text-muted-foreground" />}
  title="No patients yet"
  description="Add your first patient to get started"
  action={<Button>Add Patient</Button>}
/>
```

## Critère de complétion

```
□ Composant EmptyState réutilisable créé
□ Composant ErrorState réutilisable créé
□ Skeleton loaders sur toutes les pages
□ Empty states sur toutes les listes
□ Error states avec retry sur toutes les pages
□ Renommer en done
```
