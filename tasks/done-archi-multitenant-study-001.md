# todo-archi-multitenant-study-001.md — Étude : repenser le multi-tenant pour le dossier médical partagé

**Type** : Étude architecturale (pas de code)
**Priorité** : HAUTE (structurante pour toute la suite)
**Assigné à** : Agent architecte
**Dépendances** : aucune

---

## Contexte

L'architecture actuelle isole TOUTES les données par ClinicId (global query filter EF Core).
Or en vétérinaire, les données médicales concernent des animaux, pas des humains.
Il n'y a pas d'obligation légale de cloisonner les dossiers médicaux par cabinet.

## Proposition à étudier

**Dossier médical partagé animal** = innovation produit majeure :
- Un animal a UN dossier médical accessible par tout cabinet du pays
- Quand un animal change de vétérinaire ou consulte en urgence ailleurs, le nouveau véto voit tout l'historique
- Prescriptions, vaccinations, allergies, antécédents chirurgicaux = visibles partout
- "Passeport santé animal" numérique

## Questions architecturales à trancher

### 1. Quel scope de tenant pour quel module ?

Étudier module par module ce qui DOIT rester clinique-scoped vs ce qui pourrait être global :

| Module | Aujourd'hui | Proposition à évaluer |
|---|---|---|
| Auth (Users, Clinics) | ClinicId | Reste ClinicId — les users appartiennent à un cabinet |
| Agenda (Appointments) | ClinicId | Reste ClinicId — un RDV est dans un cabinet |
| Billing (Invoices) | ClinicId | Reste ClinicId — la facture est émise par un cabinet |
| MedicalRecords (Patients, Records, Prescriptions) | ClinicId | **À étudier : scope pays/global ?** |
| Stock | ClinicId | Reste ClinicId — le stock est physiquement dans un cabinet |
| Notifications | ClinicId | Reste ClinicId |
| Messaging | ClinicId | Reste ClinicId — conversation entre un owner et SON cabinet |

### 2. Impact technique

- Comment le `MultiTenantDbContext` gère-t-il un module partiellement global ?
- Faut-il un `CountryId` ou `RegionId` au lieu de `ClinicId` sur MedicalRecords ?
- Ou un modèle hybride : Patient global + MedicalRecord avec `OriginClinicId` (qui a créé) mais visible par tous ?
- Comment gérer le consentement du propriétaire au partage ?
- Comment gérer les conflits (2 cabinets modifient le même dossier) ?
- Impact sur les migrations EF Core existantes ?

### 3. Impact produit / business model

- Le dossier partagé est-il opt-in (le propriétaire choisit) ou par défaut ?
- Comment ça s'articule avec le modèle SaaS (chaque clinique paie) ?
- Avantage compétitif : aucun concurrent ne fait ça proprement
- Risque : certaines cliniques ne veulent peut-être pas partager (concurrence locale)

### 4. Multi-tenant pays vs global

- Faut-il un scope "pays" (toutes les cliniques UAE voient les dossiers UAE) ?
- Ou vraiment global (un animal vu aux UAE puis en France) ?
- Impact RGPD/PDPL selon les pays ?

## Livrable attendu

Un document `docs/MULTITENANT-STUDY.md` avec :
1. Recommandation architecturale tranchée (pas de "TBD")
2. Impact sur chaque module
3. Plan de migration si changement
4. Risques identifiés
5. Estimation de complexité (S/M/L/XL par module impacté)

## Ce qui est HORS SCOPE de cette étude

- Pas de code
- Pas de migration
- Pas de modification de Shared/ (gelé)
