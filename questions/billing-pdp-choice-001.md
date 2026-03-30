# Question -- billing-pdp-choice-001

**Module** : Billing
**Bloquant** : Non (bloquant uniquement pour la Phase 3 -- transmission PDP)

## Probleme

La facturation electronique obligatoire en France (sept 2026-2027) impose de transmettre les factures via une PDP (Plateforme de Dematerialisation Partenaire) ou le PPF (Portail Public de Facturation). Le choix de la plateforme partenaire a un impact sur :
- Le cout d'integration technique
- Le modele economique de Vetolib (inclus ou add-on payant)
- La qualite de service (PPF gratuit = service minimum)

## Options

**Option A** : PPF (Chorus Pro) uniquement
- Gratuit pour Vetolib et les cliniques
- API documentee mais service minimum (pas d'archivage, pas de rapprochement auto)
- Risque : API pas encore finalisee pour le secteur prive
- Impact : zero cout, mais experience utilisateur basique

**Option B** : Partenariat avec une PDP privee (Pennylane, Cegid, Sage)
- Services a valeur ajoutee (archivage legal, rapprochement automatique, integration comptable)
- Cout : commission par facture ou abonnement
- Impact : meilleure UX, potentiel revenu additionnel pour Vetolib (marge sur la commission)

**Option C** : Multi-PDP (interface abstraite + connecteurs)
- Laisser chaque clinique choisir sa PDP
- Plus complexe techniquement mais plus flexible
- Impact : Vetolib comme hub neutre, pas de dependance a un fournisseur

## Recommandation

Option C a terme, avec Option A comme implementation initiale (PPF gratuit en attendant de signer un partenariat PDP). L'architecture avec `IEInvoicingGateway` supporte deja cette approche.

## Decision PO attendue

- [ ] Valider l'approche multi-PDP
- [ ] Definir si la facturation electronique est incluse ou payante
- [ ] Prioriser les PDP partenaires a contacter

## Reponse PO

**La facturation electronique francaise (PDP/PPF) est totalement hors scope MVP.** Le MVP cible UAE (Dubai) ou la facturation electronique n'est pas imposee par la FTA pour les PME veterinaires a ce stade.

Decisions pour le futur (quand la France sera ciblee) :

1. **Approche multi-PDP validee (Option C a terme).** L'architecture avec `IEInvoicingGateway` est la bonne abstraction. On ne se lie pas a un fournisseur.

2. **Phase 1 France** : PPF (Chorus Pro) gratuit comme implementation initiale. Zero cout pour Vetolib et les cliniques. Suffisant pour la conformite legale.

3. **La facturation electronique sera incluse dans le plan de base** (pas un add-on). C'est une obligation legale, pas un service a valeur ajoutee. Facturer pour la conformite reglementaire serait mal percu par les cliniques.

4. **PDP partenaires** : a evaluer 6 mois avant le lancement France. Pennylane et Sage sont les plus credibles pour les TPE/PME. Pas d'action avant Q4 2026 au plus tot.

**Cette question n'est pas bloquante.** Aucune tache en cours n'en depend.

-> Debloque : non
-> Escalade humain requise : non
