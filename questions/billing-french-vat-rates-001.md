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

## Reponse PO

**La France est hors scope MVP.** Le marche cible est UAE (Dubai), ou le regime fiscal est TVA 5% uniforme (FTA). Il n'y a pas de taux multiples a gerer pour le MVP.

Concernant la France (second marche, post-MVP) :

1. **L'approche "taux configurables par la clinique" est validee.** C'est la bonne solution architecturale : chaque clinique saisit ses taux (ou son comptable le fait). Vetolib ne doit pas etre un conseil fiscal -- on fournit l'outil, pas l'expertise comptable.

2. **Pas de taux pre-remplis pour la France au MVP.** Quand la France sera ciblee, on fournira des taux par defaut (20% actes, 5.5% alimentation, 10% pension) avec un avertissement "verifiez avec votre comptable". La consultation d'un expert-comptable veterinaire sera planifiee a ce moment-la.

3. **Pour le MVP UAE** : un seul taux de TVA par defaut (5%), configurable par clinique. Certaines prestations veterinaires peuvent etre exonerees selon les reglements FTA -- c'est la responsabilite de la clinique de configurer correctement.

**Cette question n'est pas bloquante pour le MVP.** La tache `todo-back-billing-multi-tax-001` n'est pas prioritaire.

-> Debloque : non (tache non prioritaire, France hors scope MVP)
-> Escalade humain requise : non
