# todo-front-fixes-001.md — Fixes frontend critiques (reviews)

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : CRITIQUE (Frontend review findings)

---

## Fixes critiques

### 1. router.push() sans locale prefix (8 composants)
Tous les `router.push('/appointments')` doivent devenir `router.push('/${locale}/appointments')`.

**Fichiers :**
- `AppointmentForm.tsx` (lignes 84, 284)
- `AppointmentDetail.tsx` (ligne 180)
- `InvoiceDetail.tsx` (ligne 131)
- `InvoiceForm.tsx` (lignes 104, 297)
- `PatientForm.tsx` (lignes 112, 310, 312)
- `MedicalRecordForm.tsx` (lignes 60, 258)

**Correction :** Ajouter `const locale = useLocale()` de `next-intl` et préfixer chaque path.

### 2. zodResolver as any (2 composants)
`PatientForm.tsx` ligne 66 et `MedicalRecordForm.tsx` ligne 43.

**Correction :** Aligner les versions `@hookform/resolvers`, `react-hook-form`, `zod` pour que les types matchent sans cast.

### 3. loginSchema dans le composant
`LoginForm.tsx` — le schema Zod est recréé à chaque render.

**Correction :** Déplacer la déclaration du schema hors du composant (module scope).

## Fixes importants

### 4. Classes Tailwind physiques (RTL cassé)
- `InvoiceForm.tsx:142` — `text-left` → `text-start`
- `InvoiceDetail.tsx:243` — `ml-auto` → `ms-auto`
- `TodayAppointments.tsx:129` — `ml-1` → `ms-1`

### 5. Hardcoded clinic name sur factures
`InvoiceDetail.tsx:195-198` — "Happy Paws Veterinary" hardcodé.

**Correction :** Lire `clinicName` depuis `getStoredUser()`.

### 6. Strings non traduites restantes
`AppointmentsTable.tsx`, `AppointmentForm.tsx`, `InvoiceTable.tsx`, `InvoiceDetail.tsx` ont encore des strings hardcodées malgré les clés i18n existantes.

**Correction :** Remplacer par `useTranslations()` — les clés existent déjà dans en.json et ar.json.

### 7. useRole() flash of wrong role
`use-role.ts` — initialise à 'VET' puis corrige en useEffect.

**Correction :** Lire le cookie dans le useState initializer :
```tsx
const [role] = useState<UserRole>(() => getStoredUser()?.role ?? 'VET')
```

### 8. window.confirm() non accessible
`InvoiceDetail.tsx:127` — remplacer par un Dialog shadcn comme dans AppointmentDetail.

### 9. MSWProvider blank page
`MSWProvider.tsx` — retourne null pendant l'init.

**Correction :** Retourner un skeleton/loading overlay au lieu de null.

### 10. AppointmentsTable silent error
`AppointmentsTable.tsx:71-73` — catch vide.

**Correction :** Ajouter un état error + UI avec data-testid="appointments-error".

## Critère

```
□ Tous les router.push() préfixés avec locale
□ zodResolver sans cast as any
□ loginSchema hors du composant
□ Classes Tailwind RTL-safe (text-start, ms-*, me-*)
□ Clinic name dynamique sur les factures
□ Toutes les strings restantes passent par useTranslations()
□ useRole() sans flash
□ Dialog au lieu de window.confirm()
□ MSWProvider avec fallback loading
□ AppointmentsTable avec error state
□ npm run build → 0 erreur
□ Renommer en done
```
