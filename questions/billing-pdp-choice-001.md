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
