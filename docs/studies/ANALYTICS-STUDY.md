# Etude architecturale : Analytics + PostHog

**Date** : 2026-03-10
**Auteur** : Agent architecte
**Statut** : RECOMMANDATION

---

## 1. Etat des lieux

### Tracking actuel

Le frontend dispose d'un fichier `src/lib/analytics.ts` minimal qui utilise `window.gtag` (Google Analytics 4). Il ne couvre que la landing page (CTA clicks, scroll depth, FAQ expand, language switch, pricing toggle). Aucun tracking du comportement utilisateur dans l'application SaaS elle-meme (dashboard, patients, agenda, billing).

Le backend dispose d'OpenTelemetry (metriques HTTP, traces distribuees) via .NET Aspire et d'un audit trail (`AuditDbContext` + `AuditSaveChangesInterceptor`) qui trace les modifications de donnees. Ces deux mecanismes couvrent l'observabilite technique mais pas l'analytique produit.

### Besoin identifie

| Dimension | Couvert | Manquant |
|---|---|---|
| Observabilite technique (latence, erreurs, CPU) | OUI (OpenTelemetry + Aspire Dashboard) | - |
| Audit trail (qui a modifie quoi) | OUI (AuditDbContext) | - |
| Analytics produit (feature adoption, funnels, retention) | NON | PostHog ou equivalent |
| Business metrics (KPIs par clinique) | PARTIEL (DashboardEndpoints agrege pour l'utilisateur connecte) | Metriques cross-tenant pour l'equipe Vetolib |

---

## 2. Recommandation architecture

### PostHog Cloud (pas self-hosted)

**Recommandation : PostHog Cloud (region EU).**

| Critere | PostHog Cloud | Self-hosted |
|---|---|---|
| Cout initial | 0 (free tier 1M events/mois) | Serveur ClickHouse + PostgreSQL + Redis + ingestion = 200-400 USD/mois minimum |
| Maintenance | Zero | ClickHouse ops, upgrades, backups = charge significative |
| Data residency UAE | Pas de region UAE disponible (EU ou US) | Possible si deploye aux UAE |
| RGPD/PDPA | PostHog Cloud EU conforme RGPD. Les UAE n'ont pas de PDPL equivalent strict. | N/A |
| Time to value | 1 jour | 1-2 semaines |
| Scalabilite | Automatique | A gerer manuellement |

**Decision** : PostHog Cloud region EU. Les donnees trackees sont des events comportementaux (clics, navigation), pas des donnees medicales animales. La data residency UAE n'est pas un bloquant pour ce type de donnees. Si un client demande du on-premise, c'est une feature enterprise a adresser plus tard.

### Pas de module Vetolib.Analytics backend

**Recommandation : pas de nouveau module backend.** PostHog cote frontend suffit pour le MVP analytics.

Justification :
1. Les business metrics internes (revenue total, nombre de cliniques, churn) sont un besoin de l'equipe Vetolib, pas des utilisateurs. Ils peuvent etre extraits via des requetes SQL directes ou un outil BI (Metabase) branche sur la base PostgreSQL.
2. Les events server-side (API usage, error rates) sont deja couverts par OpenTelemetry. Les exporter vers PostHog doublerait la donnee sans valeur ajoutee.
3. L'audit trail couvre deja la tracabilite des actions utilisateur cote serveur.
4. Un module Analytics backend ajouterait du couplage : il devrait ecouter les domain events de TOUS les modules, ce qui cree une dependance transversale.

**Exception** : si le besoin de metriques business cross-tenant emerge (ex: "quel % de cliniques utilisent le module Billing ?"), on pourra ajouter un endpoint `/api/admin/metrics` dans `Vetolib.Api` (pas un module) qui agregera les donnees en lecture seule. Ce n'est pas dans le scope de cette etude.

### Architecture cible

```
+-------------------+          +-------------------+
|   Next.js 15      |          |   PostHog Cloud   |
|   App Router      |  HTTPS   |   (EU region)     |
|                   +--------->+                   |
|  PostHogProvider  |          |  - Events         |
|  usePostHog()     |          |  - Session replay |
|                   |          |  - Feature flags   |
|  lib/analytics.ts |          |  - Funnels         |
|  (unified API)    |          |  - Retention       |
+-------------------+          +-------------------+
        |
        | fetch (API calls)
        v
+-------------------+          +-------------------+
|   ASP.NET Core    |          |   Aspire Dashboard |
|   Vetolib.Api     +--------->+   (OpenTelemetry)  |
|                   |  OTLP    |                   |
|   Audit trail     |          |  - Traces          |
|   (DB interne)    |          |  - Metriques HTTP  |
+-------------------+          +-------------------+
```

Pas de fleche backend -> PostHog. Pas de module Vetolib.Analytics. Separation nette :
- **PostHog** = analytics produit (comportement utilisateur, feature adoption)
- **OpenTelemetry** = observabilite technique (latence, erreurs, saturation)
- **Audit trail** = conformite (qui a fait quoi, quand)

---

## 3. Integration Next.js 15 App Router

### Provider

PostHog fournit un SDK React officiel (`posthog-js`) avec un provider compatible React Server Components. Le provider doit etre place dans le layout client, pas dans le root layout (qui est un Server Component).

```
src/
  components/
    PostHogProvider.tsx          <-- 'use client', initialise posthog-js
  app/
    [locale]/
      (dashboard)/
        layout.tsx               <-- inclut <PostHogProvider>
      (auth)/
        layout.tsx               <-- inclut <PostHogProvider> (tracking login)
    layout.tsx                   <-- PAS de PostHog ici (Server Component)
  lib/
    analytics.ts                 <-- API unifiee (remplace gtag par PostHog)
    posthog.ts                   <-- config PostHog (API key, options)
```

### Route change tracking

Next.js 15 App Router utilise le client-side navigation. PostHog `posthog-js` capture automatiquement les pageviews si `capture_pageview: true` est configure. Pour le App Router, il faut un hook `usePathname()` + `useEffect` pour envoyer un `$pageview` a chaque changement de route.

### Impact bundle size

| Package | Taille gzip | Impact |
|---|---|---|
| `posthog-js` | ~25 KB gzip | Acceptable. Charge uniquement cote client. |
| `posthog-js/react` | inclus | - |

La landing page publique ne doit PAS charger PostHog (elle utilise gtag pour le marketing). Seul le dashboard SaaS charge PostHog. Cela se fait naturellement en placant le provider dans le layout `(dashboard)`.

### Consent management

L'utilisateur doit pouvoir opt-out du tracking comportemental. PostHog supporte nativement `posthog.opt_out_capturing()` et `posthog.opt_in_capturing()`.

Implementation :
1. Au premier login, afficher un banner de consentement (composant `ConsentBanner.tsx`)
2. Stocker le choix dans `localStorage` (cle `analytics_consent`)
3. Initialiser PostHog en mode `opt_out` par defaut (`persistence: 'localStorage'`, `opt_out_capturing_by_default: true`)
4. Si l'utilisateur accepte, appeler `posthog.opt_in_capturing()`
5. Dans les settings utilisateur, ajouter un toggle pour changer le choix

Ce mecanisme est independant d'un futur module "Preference Management". Si ce module est cree, le consentement analytics sera migre dedans.

---

## 4. Events a tracker

### Frontend — events comportementaux

#### Navigation et engagement

| Event | Properties | Priorite |
|---|---|---|
| `$pageview` | route, locale | Auto (PostHog) |
| `$pageleave` | time_on_page | Auto (PostHog) |
| `session_start` | - | Auto (PostHog) |

#### Feature usage

| Event | Properties | Priorite |
|---|---|---|
| `appointment_created` | species, has_notes | P1 |
| `appointment_status_changed` | from_status, to_status | P1 |
| `patient_created` | species, has_microchip | P1 |
| `patient_searched` | has_results, query_length | P1 |
| `patient_csv_imported` | row_count, success_count, error_count | P2 |
| `medical_record_added` | has_prescription, record_type | P1 |
| `invoice_created` | item_count, total_aed | P1 |
| `invoice_status_changed` | from_status, to_status | P1 |
| `invoice_pdf_downloaded` | - | P2 |
| `user_invited` | role | P2 |
| `user_role_changed` | from_role, to_role | P2 |
| `password_changed` | - | P2 |
| `dashboard_viewed` | has_analytics_section | P1 |
| `analytics_section_viewed` | - | P2 |

#### Funnel events

| Funnel | Events sequence |
|---|---|
| Onboarding | `clinic_registered` -> `first_patient_created` -> `first_appointment_created` -> `first_invoice_created` |
| Appointment flow | `appointment_form_opened` -> `appointment_created` -> `appointment_checked_in` -> `appointment_completed` |
| Billing flow | `invoice_form_opened` -> `invoice_created` -> `invoice_sent` -> `invoice_paid` |

#### Erreurs UX

| Event | Properties | Priorite |
|---|---|---|
| `form_validation_error` | form_name, field_name, error_type | P2 |
| `api_error_displayed` | endpoint, status_code, error_code | P1 |
| `session_expired` | time_since_login | P2 |

### Backend — pas d'events PostHog

Les events backend sont couverts par :
- **OpenTelemetry** : latence par endpoint, taux d'erreur, throughput
- **Audit trail** : qui a cree/modifie/supprime quoi, quand, dans quelle clinique
- **Logs structures Serilog** : details techniques, stack traces

Pas de valeur ajoutee a dupliquer ces donnees dans PostHog.

---

## 5. Identification utilisateur

PostHog doit identifier l'utilisateur pour la retention et les funnels. Au login :

```
posthog.identify(user.id, {
  email: user.email,
  name: user.fullName,
  role: user.role,
  clinic_id: user.clinicId,
  clinic_name: user.clinicName,
})
```

Au logout :

```
posthog.reset()
```

**Multi-tenant** : les analytics PostHog sont globales (toutes les cliniques dans le meme projet PostHog). Le filtrage par clinique se fait via la property `clinic_id` sur chaque event. PostHog supporte nativement le groupement par "organization" (= clinic dans notre cas) via `posthog.group('clinic', clinicId)`.

---

## 6. Impact sur les modules existants

### Modules backend : AUCUN impact

Aucun module backend n'est modifie. Pas de nouveau handler, pas de nouveau event, pas de nouvelle dependance.

### Frontend : impact modere

| Fichier/composant | Modification | Impact |
|---|---|---|
| `package.json` | Ajout `posthog-js` | S |
| `lib/analytics.ts` | Remplacer gtag par PostHog (API unifiee) | S |
| `lib/posthog.ts` | Nouveau fichier config | S |
| `components/PostHogProvider.tsx` | Nouveau composant provider | S |
| `components/ConsentBanner.tsx` | Nouveau composant consentement | M |
| `app/[locale]/(dashboard)/layout.tsx` | Ajouter PostHogProvider | S |
| `app/[locale]/(auth)/layout.tsx` | Ajouter PostHogProvider | S |
| Landing page | Conserver gtag (marketing, Google Ads) | Aucun |
| Composants features (appointments, patients, billing) | Ajouter `trackEvent()` aux actions utilisateur | M (nombreux fichiers, mais changements mecaniques) |
| Settings page | Ajouter toggle consent analytics | S |

### Fichiers geles : AUCUN impact

Aucune modification de Shared/, AppHost, ou Vetolib.Api/Program.cs.

---

## 7. Estimation de complexite

| Composant | Complexite | Description |
|---|---|---|
| PostHog setup (provider, config, consent banner) | S | 3 fichiers, pattern bien documente |
| Refacto `lib/analytics.ts` (gtag -> PostHog) | S | Remplacement 1:1, meme API publique |
| Instrumentation des composants features | M | 10-15 fichiers a modifier, ajout de `trackEvent()` a chaque action. Mecanique mais volumineux. |
| Consent management (banner + settings toggle) | M | UI + logique localStorage + integration PostHog opt-in/opt-out |
| Tests Playwright (verifier que PostHog n'interfere pas) | S | Verifier que les tests existants passent toujours avec PostHog charge |
| PostHog dashboard setup (funnels, retention, dashboards) | M | Configuration dans PostHog Cloud, pas de code. Necessite une reflexion produit sur les KPIs. |
| **Total** | **M (Medium)** | 2-3 jours dev, dont 1 jour de config PostHog |

---

## 8. Propositions de tasks techniques

### Task 1 : Setup PostHog provider + consent

**Fichier** : `tasks/todo-front-posthog-setup-001.md`

```
Scope :
- Installer posthog-js
- Creer lib/posthog.ts (config)
- Creer components/PostHogProvider.tsx
- Creer components/ConsentBanner.tsx
- Integrer dans (dashboard)/layout.tsx et (auth)/layout.tsx
- opt_out par defaut, opt_in apres consentement
- Conserver gtag sur la landing page (pas de regression)

Complexite : S
Skills : shadcn-nextjs
```

### Task 2 : Refactoriser lib/analytics.ts

**Fichier** : `tasks/todo-front-posthog-analytics-002.md`

```
Scope :
- Remplacer l'implementation gtag par PostHog dans lib/analytics.ts
- Garder la meme API publique (trackEvent, AnalyticsEvents)
- Ajouter les nouveaux events (appointment_created, patient_created, etc.)
- La landing page continue d'utiliser gtag directement (pas de regression)

Complexite : S
Skills : shadcn-nextjs
```

### Task 3 : Instrumenter les composants features

**Fichier** : `tasks/todo-front-posthog-instrument-003.md`

```
Scope :
- Ajouter trackEvent() dans les composants :
  - AppointmentForm (appointment_created, appointment_form_opened)
  - AppointmentsTable (appointment_status_changed)
  - PatientForm (patient_created)
  - PatientSearch (patient_searched)
  - CsvImportDialog (patient_csv_imported)
  - MedicalRecordForm (medical_record_added)
  - InvoiceForm (invoice_created)
  - InvoiceDetail (invoice_status_changed, invoice_pdf_downloaded)
  - LoginForm (login_success, login_failure)
  - UserMenu (logout)
  - Team settings (user_invited, user_role_changed)
- Ajouter les events de funnel onboarding
- Ajouter api_error_displayed dans le error boundary / apiFetch catch

Complexite : M
Skills : shadcn-nextjs
Dependance : Task 2
```

### Task 4 : Settings page — toggle analytics consent

**Fichier** : `tasks/todo-front-posthog-settings-004.md`

```
Scope :
- Ajouter une section "Privacy" dans les settings utilisateur
- Toggle on/off pour le tracking comportemental
- Appeler posthog.opt_in_capturing() / posthog.opt_out_capturing()
- Persister dans localStorage
- i18n (EN + AR)

Complexite : S
Skills : shadcn-nextjs
```

### Task 5 : PostHog Cloud setup (pas de code)

**Fichier** : `tasks/todo-infra-posthog-cloud-005.md`

```
Scope :
- Creer le projet PostHog Cloud (region EU)
- Configurer les groupes (clinic = organization)
- Creer les dashboards :
  - Feature adoption (% utilisateurs par feature)
  - Onboarding funnel (clinic_registered -> first_invoice)
  - Appointment funnel
  - Retention (weekly, par clinic)
  - Errors (api_error_displayed par endpoint)
- Configurer les alertes (drop de retention > 20%)
- Ajouter NEXT_PUBLIC_POSTHOG_KEY dans .env.example et la CI

Complexite : M
Dependance : Task 1
```

---

## 9. Risques

| # | Risque | Probabilite | Impact | Mitigation |
|---|---|---|---|---|
| R1 | PostHog augmente le bundle size (+25KB gzip) | Certaine | Faible | Charge uniquement dans le dashboard, pas la landing page. 25KB est acceptable. |
| R2 | PostHog interfere avec MSW en dev | Faible | Moyen | PostHog envoie ses events vers `app.posthog.com`, pas vers l'API locale. MSW n'intercepte que les requetes vers l'API. Pas de conflit. |
| R3 | Consent banner degrade l'UX du premier login | Moyenne | Faible | Design discret (bottom banner, pas de modal bloquant). Disparait apres le choix. |
| R4 | Free tier PostHog insuffisant | Faible (MVP) | Moyen | 1M events/mois gratuits. Avec 50 cliniques x 5 users x 100 events/jour = 750K/mois. Marge suffisante pour le lancement. |
| R5 | Data residency UAE exigee par un client | Faible (court terme) | Eleve | Si bloquant : migrer vers PostHog self-hosted deploye aux UAE. Cout : L (Large). A traiter comme feature enterprise. |
| R6 | Tests Playwright ralentis par le chargement PostHog | Moyenne | Faible | Desactiver PostHog dans l'env de test (`NEXT_PUBLIC_POSTHOG_KEY` vide = PostHog ne s'initialise pas). |

---

## 10. Ce qui ne change PAS

- **Backend** : aucun module modifie, aucun nouveau module
- **OpenTelemetry** : reste l'outil d'observabilite technique
- **Audit trail** : reste l'outil de tracabilite reglementaire
- **Shared/** : GELE, pas touche
- **AppHost, Vetolib.Api** : GELES, pas touches
- **Landing page** : conserve gtag pour le marketing (Google Ads tracking)
- **MSW handlers** : pas modifies (PostHog n'utilise pas les memes endpoints)

---

## 11. Synthese

**PostHog Cloud (EU) cote frontend uniquement** est la bonne approche pour le MVP analytics.

Raisons :
1. Zero impact backend (pas de nouveau module, pas de couplage)
2. Setup rapide (2-3 jours)
3. Free tier suffisant pour le lancement
4. Consent management natif
5. Funnels, retention, session replay out-of-the-box
6. Separation nette des responsabilites : PostHog = produit, OTel = infra, Audit = compliance

Un module backend Analytics n'est pas justifie a ce stade. Si des metriques business cross-tenant deviennent necessaires, un endpoint admin dans Vetolib.Api suffira sans creer de nouveau module.

**Complexite totale** : M (Medium) -- 2-3 jours de dev pour les 4 tasks code, + 1 jour de configuration PostHog Cloud.
