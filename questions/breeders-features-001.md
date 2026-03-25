# Breeders Features Analysis -- PO Report

**Date** : 2026-03-25
**Module** : MedicalRecords (primarily), Billing, Agenda
**Bloquant** : Non (prospective analysis for roadmap)

---

## 1. Analyse des besoins eleveurs

### 1.1 Typologie des eleveurs sur les marches cibles

#### UAE (MVP market)
- **Fauconniers** : marche premium, oiseaux valant 50k-500k+ AED. Suivi sanitaire strict, passeports CITES, import/export. Abu Dhabi Falcon Hospital = plus grand hopital de faucons au monde. Des milliers de fauconniers prives.
- **Chevaux arabes** : elevage prestigieux, courses et endurance. Suivi de lignees, tests genetiques, reproduction assistee (IA, transfert embryonnaire).
- **Chameaux de course** : marche massif aux UAE. Suivis par lot, vaccinations collectives, suivi de performance.
- **Eleveurs canins/felins** : marche plus petit mais en croissance (expatries + locaux).
- **Animaux exotiques** : reptiles, oiseaux ornementaux -- marche de niche.

#### France (second market)
- **Eleveurs canins/felins** : tres structure (LOF/LOOF), suivi de portees obligatoire, depistages reglementaires (dysplasie, tares oculaires, ADN).
- **Equin** : haras, ecuries de course, centres equestres. Reproduction assistee, suivi de gestation.
- **Bovin/ovin/caprin** : prophylaxie de troupeau, suivi sanitaire collectif, carnet sanitaire d'elevage. Marche volume.
- **NAC** : marche de niche.

#### Poland (third market)
- Profil similaire a la France. Elevage canin/felin + grande agriculture (bovin, porcin, avicole).

### 1.2 Specificites medicales des eleveurs

| Besoin | Pet owner classique | Eleveur | Couvert aujourd'hui ? |
|---|---|---|---|
| Fiche patient individuelle | Oui | Oui + par lot/groupe | Individuel seulement |
| Suivi de poids | Unique | Courbe de croissance par portee | Poids unique, pas d'historique |
| Reproduction | Non pertinent | Cycle, saillie, gestation, mise-bas | NON |
| Portees/lineage | Non pertinent | Arbre genealogique, portees liees | NON |
| Tests genetiques | Rare | Systematique (ADN, depistages) | NON |
| Vaccinations groupees | 1 animal | Par lot de 10-500 animaux | NON |
| Certificats sanitaires | Rare | Export/import, CITES, passeports | NON |
| Prophylaxie troupeau | Non | Protocoles collectifs | NON |
| Identification | Puce/tatouage | + bague (oiseaux), boucle (bovins), ADN | Pas de champ dedie |
| Facturation | Par consultation | Par lot, forfaits eleveur, remises volume | Par consultation |
| Multi-animaux par owner | 1-5 animaux | 10-5000 animaux | Oui mais pas optimise pour le volume |

---

## 2. GAPs identifies dans le modele actuel

### GAP 1 : Pas de concept de "Groupe/Lot" d'animaux
Le modele Patient est strictement individuel. Un eleveur bovin qui amene 200 veaux pour vaccination ne peut pas les gerer comme un lot. Il faudrait creer 200 fiches manuellement.

**Impact** : Bloquant pour eleveurs de betail et fauconniers avec voliere de 20+ oiseaux.

### GAP 2 : Pas de suivi de reproduction
Aucune entite pour :
- Cycle de chaleur
- Saillie (date, male, methode : naturelle/IA/transfert)
- Gestation (duree, echographies, complications)
- Mise-bas (date, nombre de petits, morts-nes)
- Portee (lien entre mere, pere, petits)

**Impact** : C'est LE besoin numero 1 des eleveurs canins/felins/equins.

### GAP 3 : Pas de lignee / arbre genealogique
Le Patient n'a aucun lien parent-enfant. Impossible de tracer :
- Mere / Pere
- Coefficient de consanguinite
- Historique genetique des ascendants

**Impact** : Critique pour eleveurs de race (LOF, registres).

### GAP 4 : Pas de tests genetiques / depistages
Le MedicalRecord est generaliste (Diagnosis + Treatment). Pas de structure pour :
- Resultats de depistage (dysplasie A/B/C/D/E, tares oculaires, etc.)
- Tests ADN (marqueurs, statut porteur/atteint/sain)
- Score de depistage avec grade standardise

**Impact** : Important pour eleveurs professionnels, legal en France.

### GAP 5 : Pas d'identification structuree
Le Patient n'a pas de champ pour :
- Numero de puce electronique (15 chiffres, standard ISO 11784/11785)
- Numero de tatouage
- Numero de bague (oiseaux)
- Numero de boucle auriculaire (bovins)
- Numero CITES (faune protegee)

**Impact** : Obligatoire legalement dans les 3 marches.

### GAP 6 : Pas de vaccination groupee
Le workflow actuel impose 1 MedicalRecord par patient par consultation. Vacciner 50 chiots d'un eleveur = 50 fiches medicales individuelles, pas de vision "campagne de vaccination".

### GAP 7 : Pas de certificats sanitaires
Aucun concept de document officiel genere : certificat de bonne sante, certificat d'export, passeport europeen, permis CITES.

### GAP 8 : Pas de gestion de sexe
Le Patient n'a pas de champ Sex (Male/Female/Neutered/Spayed/Unknown). C'est basique et manquant meme pour les pet owners classiques.

### GAP 9 : Pas d'historique de poids (courbe de croissance)
WeightKg est un scalaire unique. Pas d'historique. Les eleveurs suivent la croissance des portees semaine par semaine.

### GAP 10 : Facturation volume / forfait eleveur
Le module Billing n'a pas de concept de remise volume, forfait annuel eleveur, ou facturation par lot.

### GAP 11 : Enum Species incomplet
L'enum actuel : Dog, Cat, Bird, Rabbit, Horse, Exotic, Camel. Il manque :
- Falcon (UAE : distinct de Bird generique)
- Goat, Sheep, Cattle (betail)
- Reptile (distinct d'Exotic)

---

## 3. Features proposees (priorisees)

### Phase 1 -- Quick wins (peuvent aller dans le MVP ou juste apres)

| # | Feature | Effort | Justification |
|---|---|---|---|
| F1 | **Ajouter Sex au Patient** | XS | Basique, manque meme pour pet owners |
| F2 | **Ajouter MicrochipNumber au Patient** | XS | Obligatoire legalement, 1 champ |
| F3 | **Enrichir enum Species** | XS | Ajouter Falcon, Cattle, Sheep, Goat, Reptile |
| F4 | **Historique de poids** | S | Transformer WeightKg en collection WeightEntry(date, kg) |

### Phase 2 -- Module Reproduction (post-MVP, Q3 2026)

| # | Feature | Effort | Justification |
|---|---|---|---|
| F5 | **Entite Litter (portee)** | M | Lie une mere + pere + liste de petits. Champs : date de naissance, nombre nes, nombre vivants |
| F6 | **Lineage Patient** | M | Champs MotherPatientId, FatherPatientId sur Patient. Navigation ascendante/descendante |
| F7 | **Suivi de gestation** | M | Entite Pregnancy : dateOfMating, expectedDueDate, method (natural/AI/embryo), ultrasounds[], outcome |
| F8 | **Suivi de chaleurs** | S | HeatCycle : startDate, endDate, notes. Lie au Patient femelle |
| F9 | **Dashboard eleveur** | L | Vue Owner avec tous ses animaux, portees en cours, gestations, prochaines vaccinations |

### Phase 3 -- Module Elevage Avance (2027)

| # | Feature | Effort | Justification |
|---|---|---|---|
| F10 | **Gestion par lot/groupe** | L | AnimalGroup : nom, espece, nombre, traitements collectifs |
| F11 | **Vaccinations groupees** | M | 1 acte veterinaire = N patients vaccines |
| F12 | **Tests genetiques** | M | GeneticTest : type, result, grade, lab, date. Lie au Patient |
| F13 | **Certificats sanitaires** | L | Templates PDF, donnees pre-remplies, signature electronique |
| F14 | **Coefficient de consanguinite** | M | Calcul automatique a partir de l'arbre genealogique |
| F15 | **Import/export registre** | M | Import CSV d'un registre d'elevage, export pour registres officiels (LOF, SCC, etc.) |

### Phase 4 -- Betail & grands troupeaux (si marche valide)

| # | Feature | Effort |
|---|---|---|
| F16 | Prophylaxie de troupeau | L |
| F17 | Suivi de production (lait, oeufs) | L |
| F18 | Tracabilite boucles auriculaires | M |
| F19 | Integration registres nationaux (EDE France, APHIS UAE) | XL |

---

## 4. Impact sur l'architecture

### Option A : Extension du module MedicalRecords (recommandee pour Phase 1-2)
- F1-F4 : simples ajouts de champs/collections sur Patient. Pas de nouveau module.
- F5-F8 : nouvelles entites (Litter, Pregnancy, HeatCycle) dans MedicalRecords. Cela reste du dossier medical.
- F9 : frontend uniquement, consomme les APIs existantes.

**Avantage** : pas de nouveau module, pas de nouveau DbContext, pas de migration d'architecture.

### Option B : Nouveau module Vetolib.Breeding (recommandee pour Phase 3+)
Quand le suivi de reproduction + genetique + certificats + lots devient trop gros pour MedicalRecords, extraire un module dedie :
```
Modules/
  Breeding/
    Vetolib.Breeding.Contracts/    -- Litter, Pregnancy, GeneticTest DTOs
    Vetolib.Breeding/              -- Domain, Handlers, DbContext
```
Communication avec MedicalRecords via Contracts (PatientId, events).

### Option C : Nouveau module Vetolib.Livestock (Phase 4 uniquement)
Si le marche betail est valide, un module separe pour la gestion de troupeau qui a des concepts tres differents (lots, prophylaxie collective, production).

**Recommandation PO** : Phase 1 dans MedicalRecords (extensions mineures). Phase 2 dans MedicalRecords (nouvelles entites). Phase 3+ dans un module Breeding dedie. Phase 4 dans un module Livestock dedie seulement si le marche est valide.

---

## 5. Recommandation PO

### Priorite immediate (avant commercialisation UAE)
**F1 (Sex), F2 (Microchip), F3 (Species enum)** -- ces 3 manques sont embarrassants meme pour les pet owners classiques. Un veterinaire qui ne peut pas enregistrer le sexe d'un animal ou son numero de puce ne prendra pas le logiciel au serieux. Ce sont des champs basiques que tout concurrent a. **A integrer dans le MVP.**

### Priorite haute (Q3 2026, post-MVP)
**F4 (historique poids), F5 (portees), F6 (lineage)** -- ce sont les features qui differencient Vetolib pour les eleveurs. Le marche fauconniers UAE est particulierement premium et ces eleveurs attendent un suivi de lignee.

### Pricing
Je recommande un **module add-on "Breeding"** facture separement :
- Plan de base : patient, dossier medical, agenda, billing -- pour les cliniques classiques
- Add-on Breeding : reproduction, portees, lignees, tests genetiques -- pour les cliniques qui servent des eleveurs
- Add-on Livestock : gestion de troupeau -- pour les cliniques rurales

Cela permet de garder le prix de base competitif pour les cliniques urbaines (80% du marche) tout en monetisant les features avancees aupres des cliniques specialisees.

### Marche UAE specifique
Le marche faucons UAE est un **ocean bleu** : peu de logiciels veterinaires couvrent les faucons avec suivi de lignee. Les fauconniers ont un pouvoir d'achat tres eleve. L'ajout de `Falcon` dans l'enum Species et un partenariat avec un hopital de faucons pourrait etre un excellent angle de penetration du marche.

Les chevaux arabes et chameaux de course sont egalement des segments premium ou le suivi genealogique est un argument de vente massif.

---

## 6. Questions pour le fondateur

### Q1 : Priorite Sex + Microchip
Confirmes-tu que F1 (Sex) et F2 (MicrochipNumber) doivent etre integres au MVP ? Ce sont des champs basiques que tous les concurrents ont. Sans eux, le logiciel parait incomplet a n'importe quel veterinaire.

### Q2 : Marche faucons
Veux-tu positionner Vetolib comme outil de reference pour les cliniques de faucons UAE ? Cela impliquerait :
- Ajout de `Falcon` dans Species (distinct de `Bird`)
- Suivi CITES (Convention internationale sur les especes protegees)
- Partenariat avec Abu Dhabi Falcon Hospital ou similaire
- Features de lignee en priorite haute

### Q3 : Betail -- on y va ou pas ?
Le marche betail (bovin, ovin, caprin) est tres different du marche companion animals. Les workflows sont collectifs (troupeaux, lots), la facturation est differente, les reglementations sont specifiques (tracabilite alimentaire). Est-ce dans le scope de Vetolib ou on reste sur companion + equin + faucons ?

**Recommandation PO** : rester sur companion animals + equin + faucons pour le MVP et la phase 2. Le betail est un marche enorme mais tres different -- le couvrir correctement necessiterait un effort considerable et diluerait le focus.

### Q4 : Registres officiels
Faut-il prevoir une integration avec les registres d'elevage (LOF/SCC en France, ADCH aux UAE, ZKWP en Pologne) ? C'est un avantage concurrentiel fort mais un effort d'integration significatif.

### Q5 : Module add-on ou tout-en-un ?
Confirmes-tu l'approche module add-on payant pour Breeding ? Ou preferes-tu tout inclure dans un plan unique (plus simple mais prix de base plus eleve) ?

### Q6 : Timeline
Quand veux-tu commencer a developper les features eleveurs ? Options :
- **A)** F1-F3 maintenant (MVP), F4-F6 Q3 2026
- **B)** Tout en post-MVP Q3-Q4 2026
- **C)** Apres la commercialisation UAE, en fonction du feedback terrain

**Recommandation PO** : Option A -- les quick wins (Sex, Microchip, Species) sont trop basiques pour manquer au MVP.

---

## Reponse PO

Toutes les questions du fondateur ont recu reponse. Les decisions sont integrees dans la spec `docs/specs/BREEDERS-FEATURES-SPEC.md` :

- **Q1** : Oui, Sex + Microchip dans le MVP immediatement
- **Q2** : Oui, positionner pour les faucons UAE
- **Q3** : Non au betail pour l'instant, focus companion + equin + faucons
- **Q4** : Registres officiels en Phase 3 (pas urgent)
- **Q5** : Module add-on "Breeding" payant
- **Q6** : Option A -- F1-F3 maintenant, F4-F6 en Q3 2026

Deliverables produits :
- `docs/specs/BREEDERS-FEATURES-SPEC.md` -- spec complete avec DTOs, endpoints, regles metier, wireframes
- 6 fichiers `.feature` Gherkin (53 scenarios, tous en anglais, tous @wip)
- Enum Species : Cattle/Sheep/Goat EXCLUS (fondateur a dit non au betail)

-> Escalade humain requise : non

---

## Resume executif

Vetolib couvre aujourd'hui 60% des besoins d'une clinique veterinaire classique (pet owners). Pour les eleveurs, la couverture est proche de 0% -- il manque les fondamentaux (sexe, puce, reproduction, lignees).

Les quick wins (F1-F3) sont des **lacunes** du MVP, pas des features eleveurs -- tout veterinaire les attend. Le module Breeding (F5-F8) est un **differenciateur** qui ouvre le segment premium UAE (faucons, chevaux arabes). Le betail (F16-F19) est un marche distinct a valider separement.

L'architecture modulaire de Vetolib permet d'ajouter ces features sans refactoring majeur : Phase 1-2 dans MedicalRecords, Phase 3+ dans un module Breeding dedie.
