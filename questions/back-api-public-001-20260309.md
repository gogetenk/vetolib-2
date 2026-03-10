# Question — back-api-public-001

**Module** : Infra (nouveau module ApiKeys + Webhooks)
**Bloquant** : Oui

## Probleme

La tache `wip-back-api-public-001.md` demande d'implementer l'API publique avec API keys, webhooks et une page frontend. Deux blocages rendent l'implementation impossible sans arbitrage :

### Blocage 1 — Aucun fichier .feature Gherkin

La regle BDD-first est non-negociable (CLAUDE.md §3) : les step definitions doivent etre ecrites AVANT l'implementation et virer RED avant de passer GREEN. Il n'existe aucun fichier `.feature` pour :
- La creation/revocation de cles API
- L'authentification via `X-Api-Key`
- La configuration et delivery de webhooks
- Les scopes d'autorisation

Sans Gherkin, l'agent ne peut pas demarrer l'implementation. Un PR ouvert avec des tests rouges est automatiquement rejete.

### Blocage 2 — Program.cs est GELE

L'ajout de l'authentification par API key (nouveau scheme ASP.NET Core) et le rate limiting par cle API necessitent de modifier `Vetolib.Api/Program.cs`, qui est explicitement liste comme GELE dans CLAUDE.md (section "Fichiers GELES"). Toute modification requiert un arbitrage humain.

### Blocage 3 — Scope ambigu pour le module

La tache ne precise pas :
- Dans quel module vivent les entites `ApiKey` et `Webhook` (nouveau module `ApiKeys` ? dans `Auth` ?)
- Si `ApiKey.ClinicId` respecte le multi-tenant automatique (IMultiTenant) ou une logique differente
- Qui peut creer des API keys : Admin uniquement ? Vet aussi ?
- Les events webhook couvrent quels modules exactement (agenda, billing, patients) — les handlers doivent abonner des consumers MassTransit inter-modules

## Options

**Option A** : Debloquer la tache completement
- PO/architecte ecrit les fichiers `.feature` dans `features/api-keys/` et `features/webhooks/`
- PO autorise explicitement la modification de `Program.cs` via le flag `MODIF_SHARED: autorise` dans la tache
- PO precise le module d'appartenance et les regles RBAC
- Impact : 2-3 jours de travail supplementaire avant que l'agent puisse demarrer

**Option B** : Decouper en sous-taches plus petites avec Gherkins fournis
- Sous-tache 1 : OpenAPI/Scalar uniquement (pas de Gherkin necessaire, modification minimale de Program.cs)
- Sous-tache 2 : API Keys (Gherkin fourni, nouveau module, flag MODIF_SHARED)
- Sous-tache 3 : Webhooks (Gherkin fourni, apres sous-tache 2)
- Impact : livraison incrementale, risque reduit

**Option C** : Deprioritiser (tache marquee BASSE priorite)
- Laisser en `todo` jusqu'a ce que le MVP soit complet
- Ne pas bloquer les autres taches en cours
- Impact : zero cout immediat, fonctionnalite differee

## Recommandation

Option B ou C. La tache est de priorite BASSE et le MVP n'est pas encore complete (autres taches wip en cours). Commencer par la sous-tache OpenAPI/Scalar uniquement serait le debloqueur minimum a risque zero — il n'y a pas de logique metier, pas besoin de Gherkin, et la modification de Program.cs est minimale (`app.MapScalarApiReference()` ou equivalent).

Pour les API keys et webhooks, fournir les fichiers Gherkin et lever le gel de Program.cs sont des pre-requis non-negociables.

---

## Reponse PO

**Decision : Option C retenue — Deprioritise. La tache reste en `todo`, pas en `wip`.**

Justification :

1. **Le MVP n'est pas termine.** Des taches wip sont encore en cours (stock management, CSV import, signup self-service). L'API publique est un accelerateur d'adoption pour les integrateurs tiers, pas une fonctionnalite core pour les cliniques UAE au lancement. Aucune clinique n'a demande d'integration tierce a ce stade.

2. **Les 3 blocages identifies sont reels et non-negociables.** Ecrire les Gherkins pour API keys et webhooks represente un effort de spec significatif. Ce n'est pas le bon moment pour investir ce temps.

3. **La seule sous-tache livrable sans risque est OpenAPI/Scalar.** Si un agent a du temps libre, il peut creer une sous-tache `todo-back-openapi-scalar-001.md` limitee a :
   - Activer Scalar sur `/api/docs`
   - Documenter les endpoints existants
   - Modification minimale de Program.cs autorisee (ajout de `app.MapScalarApiReference()` uniquement)
   - Pas de Gherkin necessaire (zero logique metier)

4. **API keys et webhooks sont reportes a post-MVP.** Quand le moment viendra :
   - Le PO ecrira les fichiers `.feature` dans `features/api-keys/` et `features/webhooks/`
   - Le flag `MODIF_SHARED: autorise` sera ajoute a la tache
   - Les API keys vivront dans le module Auth (meme tenant model, meme RBAC)
   - Seul le role ADMIN peut creer/revoquer des API keys
   - Les webhooks couvriront : appointment.created, appointment.updated, invoice.created, invoice.paid, patient.created

**La tache `wip-back-api-public-001.md` doit etre renommee en `todo-back-api-public-001.md` (retour en todo).** Elle est deja en todo, donc aucune action necessaire.

→ Debloque : non (tache reste en todo)
→ Escalade humain requise : non
