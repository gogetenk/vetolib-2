# todo-qa-recette-complete-001.md — Plan de recette E2E complet

**Module** : Tous
**Dépendances** : done-back-rbac-matrix-001, done-back-outputcache-001
**Priorité** : BLOQUANT (pre-release)
**Skills à lire** : `playwright-e2e`, `reqnroll-bindings`

---

## Objectif

Recette complète du système Vetolib de bout en bout. Tous les parcours utilisateurs critiques doivent être testés en E2E (Playwright) contre le vrai backend (pas MSW).

---

## Plan de tests — Parcours utilisateurs

### 1. AUTH — Authentification & Gestion d'équipe

```gherkin
# P1-AUTH-01 : Login admin
Given le backend est démarré avec les seeds
When je navigue vers /en/login
And je saisis admin@desertpaws.ae / [seed password]
Then je suis redirigé vers /en/dashboard
And je vois le dashboard avec les stats

# P1-AUTH-02 : Login vet
Given je suis sur /en/login
When je saisis dr.sarah@desertpaws.ae / [seed password]
Then je suis redirigé vers /en/dashboard

# P1-AUTH-03 : Login échoué
Given je suis sur /en/login
When je saisis un email/mot de passe invalide
Then je vois le message d'erreur "Invalid email or password"

# P1-AUTH-04 : Account lockout
Given je rate 5 tentatives de login
When je tente une 6ème connexion
Then je vois "Account locked. Try again in 15 minutes."

# P1-AUTH-05 : Logout
Given je suis connecté en admin
When je clique sur "Sign Out"
Then je suis redirigé vers /en/login
And les cookies httpOnly sont supprimés

# P1-AUTH-06 : Refresh token automatique
Given je suis connecté
And mon access token a expiré (15 min)
When je navigue vers une page protégée
Then le token est rafraîchi automatiquement
And la page se charge normalement

# P1-AUTH-07 : Inviter un utilisateur (Admin only)
Given je suis connecté en admin
When je navigue vers /en/settings/team
And je clique "Invite User"
And je remplis email + role (Receptionist)
Then l'utilisateur est créé
And un email d'invitation arrive dans MailHog

# P1-AUTH-08 : Changement de mot de passe
Given je suis connecté
When je navigue vers la page profil
And je change mon mot de passe
Then le changement est effectif
And mes sessions sur les autres appareils sont invalidées

# P1-AUTH-09 : Force change password (invited user)
Given un utilisateur a été invité avec un mot de passe temporaire
When il se connecte pour la première fois
Then il est redirigé vers /en/change-password
And il ne peut pas naviguer ailleurs tant qu'il n'a pas changé
```

### 2. AGENDA — Rendez-vous

```gherkin
# P2-AGENDA-01 : Créer un RDV (Receptionist)
Given je suis connecté en receptionist
When je navigue vers /en/appointments
And je clique "New Appointment"
And je remplis : patient "Max", species "Dog", owner "Ahmed", date demain 10h, vet "Dr. Sarah", reason "Vaccination"
Then le RDV est créé avec statut SCHEDULED

# P2-AGENDA-02 : Voir la liste des RDV du jour
Given il y a 3 RDV aujourd'hui
When je navigue vers /en/appointments
Then je vois 3 lignes dans le tableau
And les colonnes affichent date, patient, owner, vet, statut

# P2-AGENDA-03 : Workflow complet d'un RDV
Given un RDV est SCHEDULED
When la receptionist fait le check-in → CHECKED_IN
And le vet démarre la consultation → IN_PROGRESS
And le vet termine la consultation → COMPLETED
Then le statut final est COMPLETED

# P2-AGENDA-04 : Annuler un RDV
Given un RDV est SCHEDULED
When je l'annule avec un motif
Then le statut passe à CANCELLED

# P2-AGENDA-05 : Modifier un RDV (reschedule)
Given un RDV est SCHEDULED pour demain 10h
When je le modifie pour après-demain 14h
Then la nouvelle date est enregistrée

# P2-AGENDA-06 : Conflit horaire
Given un RDV existe demain 10h avec Dr. Sarah
When je crée un autre RDV demain 10h avec Dr. Sarah
Then le système refuse avec un message de conflit

# P2-AGENDA-07 : Disponibilités d'un vétérinaire
Given je suis sur le formulaire de création de RDV
When je sélectionne Dr. Sarah et une date
Then je vois les créneaux disponibles

# P2-AGENDA-08 : Assistant ne peut PAS créer de RDV
Given je suis connecté en assistant
When je tente de créer un RDV
Then je reçois 403 Forbidden
```

### 3. PATIENTS / OWNERS — Dossiers patients

```gherkin
# P3-PATIENTS-01 : Créer un patient (Vet)
Given je suis connecté en vet
When je navigue vers /en/patients
And je clique "New Patient"
And je remplis : nom "Simba", espèce "Cat", owner "Fatima Al-Rashid", phone "+971 50 123 4567"
Then le patient est créé

# P3-PATIENTS-02 : Rechercher un patient
Given il y a 10 patients
When je tape "Sim" dans la recherche
Then je vois "Simba" dans les résultats

# P3-PATIENTS-03 : Filtrer par espèce
Given il y a des chiens et des chats
When je filtre par "Cat"
Then je ne vois que les chats

# P3-PATIENTS-04 : Voir le détail d'un patient
Given un patient "Simba" existe avec 2 dossiers médicaux
When je clique sur "Simba"
Then je vois les infos du patient + l'historique médical

# P3-PATIENTS-05 : Espèces UAE (chameau)
Given je crée un patient espèce "Camel"
Then le patient est créé avec succès (espèce valide pour le marché UAE)

# P3-PATIENTS-06 : Receptionist ne peut PAS créer de patient
Given je suis connecté en receptionist
When je tente de créer un patient
Then je reçois 403 Forbidden
```

### 4. DOSSIERS MÉDICAUX

```gherkin
# P4-MEDICAL-01 : Ajouter un dossier médical (Vet)
Given je suis connecté en vet
And un patient "Simba" existe
When j'ajoute un dossier médical avec reason "Annual checkup", weight 4.5kg
Then le dossier est créé

# P4-MEDICAL-02 : Ajouter une prescription (Vet only)
Given un dossier médical existe
When j'ajoute une prescription "Amoxicillin 250mg, 2x/day, 7 days"
Then la prescription est enregistrée avec le numéro de licence vétérinaire

# P4-MEDICAL-03 : Receptionist ne peut PAS voir les dossiers médicaux
Given je suis connecté en receptionist
When je tente d'accéder aux dossiers médicaux d'un patient
Then je reçois 403 Forbidden

# P4-MEDICAL-04 : Admin ne peut PAS prescrire
Given je suis connecté en admin (pas de licence véto)
When je tente d'ajouter une prescription
Then le système refuse (pas de licence vétérinaire)
```

### 5. FACTURATION

```gherkin
# P5-BILLING-01 : Créer une facture (Receptionist)
Given je suis connecté en receptionist
And un patient "Simba" existe
When je crée une facture pour Simba
And j'ajoute un item "Consultation" qty 1, 150 AED
And j'ajoute un item "Vaccination" qty 2, 75 AED
Then le subtotal est 300 AED
And la TVA 5% est 15 AED
And le total est 315 AED

# P5-BILLING-02 : TRN sur la facture
Given une facture existe
When je génère le PDF
Then le PDF contient le TRN "100XXXXXXXXX"
And le nom de la clinique est dynamique (pas hardcodé)

# P5-BILLING-03 : Workflow facture complet
Given une facture est en DRAFT
When la receptionist l'envoie → SENT
And la receptionist marque comme payée → PAID
Then le statut final est PAID

# P5-BILLING-04 : Seul Admin peut annuler une facture
Given une facture est en SENT
When un vet tente de l'annuler
Then il reçoit 403
When un admin l'annule
Then le statut passe à CANCELLED

# P5-BILLING-05 : Montant négatif rejeté
Given je crée une facture
When j'ajoute un item avec un prix négatif
Then le système refuse

# P5-BILLING-06 : Arrondi TVA correct
Given un item à 115.33 AED
Then la TVA 5% est 5.77 AED (arrondi correct)
```

### 6. DASHBOARD

```gherkin
# P6-DASHBOARD-01 : Stats du jour
Given il y a 5 RDV aujourd'hui dont 2 en attente de check-in
And il y a 3 factures impayées totalisant 1500 AED
And il y a 25 patients au total
When je navigue vers /en/dashboard
Then je vois : 5 RDV, 2 check-ins en attente, 1500 AED impayés, 25 patients

# P6-DASHBOARD-02 : RDV du jour
Given il y a 3 RDV aujourd'hui
When je regarde la section "Today's Appointments"
Then je vois 3 RDV triés par heure
```

### 7. i18n — Internationalisation

```gherkin
# P7-I18N-01 : Switch EN → AR
Given je suis sur /en/login
When je clique sur le sélecteur de langue "عربي"
Then je suis redirigé vers /ar/login
And la page est en arabe
And le layout est RTL (dir="rtl")

# P7-I18N-02 : Contenu arabe correct
Given je suis sur /ar/dashboard
Then je vois "المواعيد" au lieu de "Appointments"
And les chiffres sont formatés correctement

# P7-I18N-03 : Persistance de la langue
Given j'ai choisi l'arabe
When je navigue entre les pages
Then toutes les pages restent en arabe
```

### 8. MULTI-TENANCY — Isolation

```gherkin
# P8-TENANT-01 : Isolation des données
Given la clinique A a un patient "Rex"
And la clinique B a un patient "Luna"
When un user de la clinique A liste les patients
Then il voit "Rex" mais PAS "Luna"

# P8-TENANT-02 : Pas d'accès cross-clinic
Given je suis connecté à la clinique A
When je tente d'accéder à un patient de la clinique B par son ID
Then je reçois 404 (pas 403 — le patient "n'existe pas" pour ce tenant)
```

### 9. SÉCURITÉ

```gherkin
# P9-SEC-01 : Rate limiting login
Given je fais 10 requêtes POST /api/auth/login en 1 minute
When je fais la 11ème
Then je reçois 429 Too Many Requests avec Retry-After header

# P9-SEC-02 : JWT httpOnly
Given je me connecte avec succès
Then le cookie access_token est httpOnly, Secure, SameSite=Strict
And aucun token n'est visible dans localStorage

# P9-SEC-03 : Pages protégées sans auth
Given je ne suis pas connecté
When je navigue vers /en/dashboard
Then je suis redirigé vers /en/login
```

### 10. PERFORMANCE — Cache

```gherkin
# P10-PERF-01 : OutputCache fonctionne
Given je fais 2 GET /api/v1/invoices identiques en 5 secondes
Then la 2ème requête est servie depuis le cache (temps de réponse < 10ms)

# P10-PERF-02 : Invalidation fonctionne
Given je fais GET /api/v1/invoices (cachée)
When je crée une nouvelle facture
And je refais GET /api/v1/invoices
Then la nouvelle facture apparaît (cache invalidé)
```

---

## Implémentation

### Structure des tests E2E

```
src/frontend/e2e/
├── recette/
│   ├── auth.spec.ts           ← P1-AUTH-01 à P1-AUTH-09
│   ├── agenda.spec.ts         ← P2-AGENDA-01 à P2-AGENDA-08
│   ├── patients.spec.ts       ← P3-PATIENTS-01 à P3-PATIENTS-06
│   ├── medical-records.spec.ts ← P4-MEDICAL-01 à P4-MEDICAL-04
│   ├── billing.spec.ts        ← P5-BILLING-01 à P5-BILLING-06
│   ├── dashboard.spec.ts      ← P6-DASHBOARD-01 à P6-DASHBOARD-02
│   ├── i18n.spec.ts           ← P7-I18N-01 à P7-I18N-03
│   ├── tenant-isolation.spec.ts ← P8-TENANT-01 à P8-TENANT-02
│   ├── security.spec.ts       ← P9-SEC-01 à P9-SEC-03
│   └── performance.spec.ts    ← P10-PERF-01 à P10-PERF-02
├── fixtures/
│   ├── auth.fixtures.ts       ← Login helpers, user creation
│   └── data.fixtures.ts       ← Patients, appointments, invoices setup
└── playwright.recette.config.ts ← Config spécifique recette (real backend)
```

### Config Playwright pour recette

```typescript
// playwright.recette.config.ts
export default defineConfig({
  testDir: './e2e/recette',
  use: {
    baseURL: 'http://localhost:3000',
  },
  webServer: [
    {
      command: 'dotnet run --project ../../backend/AppHost/Vetolib.AppHost.csproj',
      port: 5000,
      reuseExistingServer: true,
      timeout: 120000,
    },
    {
      command: 'npm run dev',
      port: 3000,
      reuseExistingServer: true,
    },
  ],
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
});
```

### Commande

```bash
# Lancer la recette complète
npx playwright test --config=playwright.recette.config.ts

# Lancer un module spécifique
npx playwright test --config=playwright.recette.config.ts e2e/recette/auth.spec.ts
```

## Critère

```
□ 45+ scénarios E2E implémentés en Playwright
□ Tous les parcours critiques couverts (auth, agenda, patients, medical, billing, dashboard)
□ Tests i18n EN + AR (RTL)
□ Tests multi-tenancy (isolation)
□ Tests sécurité (rate limiting, httpOnly, pages protégées)
□ Tests performance (cache hit/invalidation)
□ Tests RBAC (Assistant 403, Receptionist 403 medical records)
□ Config playwright.recette.config.ts contre le vrai backend
□ npm run test:recette → tous verts
□ Rapport HTML Playwright généré
□ Renommer en done
```
