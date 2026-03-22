# Question -- billing-french-vat-rates-001

**Module** : Billing
**Bloquant** : Oui (bloquant pour `todo-back-billing-multi-tax-001`)

## Probleme

Les taux de TVA applicables aux actes veterinaires en France ne sont pas triviaux. Il existe plusieurs regimes possibles :

1. **Actes veterinaires (consultations, chirurgies)** : 20% (taux normal) -- semble clair
2. **Medicaments veterinaires delivres sur ordonnance** : 10% ou 20% ? Le taux reduit de 10% s'applique-t-il aux medicaments veterinaires comme pour les medicaments humains non rembourses ?
3. **Alimentation animale (croquettes, complements)** : 5.5% (alimentation) ou 20% (produit non alimentaire) ?
4. **Produits d'hygiene animale (shampoing, antiparasitaires OTC)** : 20% ?
5. **Pension / garde d'animaux** : 10% (hebergement) ou 20% ?

Un mauvais taux de TVA = redressement fiscal pour la clinique. Vetolib ne peut pas se permettre une erreur ici.

## Options

**Option A** : Demander a un expert-comptable specialise veterinaire
- Fiable, source autorisee
- Impact : delai de quelques jours

**Option B** : Se baser sur le BOFiP (Bulletin Officiel des Finances Publiques)
- Source officielle mais interpretation complexe
- Risque d'erreur d'interpretation

## Recommandation

Option A. Contacter un cabinet comptable specialise veterinaire (beaucoup existent en France, ex: VetoCompta, ComptaVet) pour obtenir une grille de taux validee.

En attendant, implementer le systeme multi-taux avec des taux configurables par la clinique (le veterinaire ou son comptable saisit les taux). Cela debloque le dev sans risque fiscal.

## Decision PO attendue

- [ ] Valider l'approche "taux configurables par la clinique" comme solution initiale
- [ ] Planifier la consultation d'un expert-comptable veterinaire
