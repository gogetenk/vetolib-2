# Drug Data Study -- Seed vs API externe vs Hybride

> **Date** : 2026-03-10
> **Auteur** : Architect agent
> **Statut** : Recommandation pour validation PO
> **Contexte** : La spec PRESCRIPTIONS-AI-SPEC.md prevoit un DrugCatalogEntry seede avec ~250 medicaments, ~100 interactions, ~30 contre-indications par espece. Le PO demande si cette approche tient face a l'ambition internationale.

---

## 1. Etat des lieux -- APIs de medicaments veterinaires

### 1.1 Bases de donnees publiques existantes

| Source | Couverture | API disponible | Gratuit | Notes |
|---|---|---|---|---|
| **FDA Green Book** (USA) | Medicaments veterinaires approuves aux USA | Oui (openFDA API, JSON) | Oui | Produits commerciaux uniquement, pas d'interactions, pas de dosages par espece |
| **EMA (Europe)** | Medicaments veterinaires autorises UE | API limitee (bulk download XML) | Oui | Donnees reglementaires (AMM), pas d'interactions ni dosages |
| **ANMV / IRaMuTeQ (France)** | Medicaments veterinaires francais | Non (PDF/HTML scraping requis) | Oui | Donnees reglementaires uniquement |
| **VetBNF (UK, BSAVA)** | Formulaire veterinaire britannique | Non -- acces sur abonnement, pas d'API | Payant (abonnement BSAVA) | Reference clinique excellente mais pas programmatique |
| **VetCompendium (Australie)** | Medicaments veterinaires australiens | Non | Payant | Meme situation que VetBNF |

### 1.2 Bases de donnees privees/commerciales

| Source | Contenu | API | Cout | Notes |
|---|---|---|---|---|
| **Plumb's Veterinary Drug Handbook** | La reference mondiale : ~700 monographies, dosages par espece, interactions | Non (pas d'API publique). Plumb's Veterinary Drugs (app mobile) existe mais sans API tierce. | Abonnement ~$200/an par utilisateur | Contenu sous copyright strict. Impossible d'integrer directement. |
| **DrugBank** | Exhaustif pour l'humain, partiel pour le veterinaire | Oui (REST API) | $20k-100k/an selon volume | Focus humain. Molecules identiques mais pas de dosages veterinaires ni contre-indications par espece animale. |
| **RxNorm (NLM/NIH)** | Nomenclature standardisee de medicaments humains | Oui (REST API, gratuit) | Gratuit | Humain uniquement. Pas d'especes animales. |
| **Vidal (France)** | Medicaments humains francais | Oui (payant) | Negocie | Zero couverture veterinaire |

### 1.3 Conclusion sur les APIs

**Il n'existe pas d'equivalent de DrugBank ou Vidal pour le veterinaire.** Le marche des donnees pharmacologiques veterinaires est fragmente, non standardise, et sans API ouverte exploitable.

Les seules donnees ouvertes (FDA Green Book, EMA) fournissent des informations reglementaires (nom commercial, fabricant, AMM) mais **pas** les donnees cliniques dont Vetolib a besoin : interactions, contre-indications par espece, dosages par poids.

---

## 2. Interactions medicamenteuses veterinaires

### 2.1 Les interactions humaines s'appliquent-elles ?

Partiellement. Au niveau moleculaire, les mecanismes d'interaction pharmacocinetique et pharmacodynamique sont identiques (inhibition enzymatique CYP450, competition proteinique, etc.). Cependant :

- **Les metabolismes different par espece.** Exemple : le chat ne glucuronide pas (deficience en UGT1A6), ce qui rend des molecules banales chez le chien (paracetamol, aspirine) potentiellement letales chez le chat.
- **Les dosages sont radicalement differents.** Un anti-inflammatoire sur chez l'homme peut etre toxique chez un reptile.
- **Certaines interactions n'existent qu'en veterinaire** (ex : xylazine + ketamine, combinaison courante en anesthesie veterinaire mais inexistante en medecine humaine).

### 2.2 Existe-t-il une base d'interactions specifiquement veterinaire ?

Non, pas sous forme de base de donnees structuree accessible. Les interactions veterinaires sont documentees dans :
- **Plumb's** (texte narratif par monographie, non structure)
- **BSAVA Small Animal Formulary** (tables dans le livre)
- **Publications scientifiques** (dispersees, pas agreges)

Aucune base structuree (JSON, SQL, API) n'existe publiquement.

### 2.3 Plumb's a-t-il une API ?

Non. Plumb's Veterinary Drugs est disponible en app mobile (iOS/Android) et en abonnement web, mais **sans API tierce**. Le contenu est sous copyright et leur modele commercial repose sur la vente directe aux praticiens. Un partenariat B2B serait possible mais couteux et non garanti.

---

## 3. Analyse cout/benefice

### Option A : Seed local uniquement (approche actuelle)

| Critere | Evaluation |
|---|---|
| Controle | Total -- donnees curatees par l'equipe |
| Offline | Oui -- zero dependance reseau |
| Exhaustivite | Partielle -- ~250 medicaments sur ~3000 molecules veterinaires existantes |
| Mise a jour | Manuelle -- necessite un pharmacologue veterinaire pour maintenir |
| Cout initial | Moyen -- compilation des donnees INN + interactions (~2-4 semaines de travail d'un pharmacologue vet) |
| Cout recurrent | Faible -- mise a jour annuelle suffit (nouvelles molecules rares) |
| Risque juridique | Faible si INN (domaine public) + descriptions originales (pas de copie Plumb's) |
| Risque clinique | Moyen -- donnees potentiellement incompletes, interactions manquantes |

### Option B : API externe comme source principale

| Critere | Evaluation |
|---|---|
| Controle | Faible -- dependance au fournisseur |
| Offline | Non |
| Exhaustivite | **Impossible** -- aucune API veterinaire complete n'existe |
| Mise a jour | Automatique (si l'API existait) |
| Cout initial | Indetermine -- aucune API viable identifiee |
| Cout recurrent | Potentiellement eleve ($20k+/an pour DrugBank, qui est humain) |
| Risque juridique | Moyen -- depend du contrat fournisseur |
| Risque clinique | Faible si les donnees sont exhaustives (mais elles ne le sont pas pour le vet) |

### Option C : Hybride -- seed local + enrichissement optionnel

| Critere | Evaluation |
|---|---|
| Controle | Eleve -- seed local comme base, API en complement |
| Offline | Oui -- le seed local fonctionne toujours, l'API est optionnelle |
| Exhaustivite | Progressive -- seed couvre 80% des cas courants, API comble les trous |
| Mise a jour | Semi-automatique |
| Cout initial | Moyen (seed) + variable (integration API future) |
| Cout recurrent | Faible (seed) + variable (API) |
| Risque juridique | Faible |
| Risque clinique | Le plus faible des trois options |

---

## 4. Recommandation

### MVP (maintenant) : Seed local -- confirmer l'approche actuelle

L'approche seedee prevue dans PRESCRIPTIONS-AI-SPEC.md est la bonne. Raisons :

1. **Aucune API veterinaire exploitable n'existe.** C'est le fait determinant. On ne peut pas s'appuyer sur un service qui n'existe pas.
2. **Les donnees critiques sont peu nombreuses.** ~30 contre-indications par espece et ~100 interactions majeures couvrent 95% des cas cliniques dangereux. Le risque n'est pas l'exhaustivite du catalogue (un vet connait ses medicaments) mais les alertes de securite (interactions et contre-indications).
3. **INN est dans le domaine public.** Les noms internationaux non proprietaires sont libres de droits. Seuls les descriptions d'interactions doivent etre originales (pas copiees de Plumb's).
4. **Le marche UAE a un formulaire restreint.** Le nombre de medicaments veterinaires autorises aux EAU est significativement plus petit que dans l'UE ou les USA, ce qui rend un seed de ~250 molecules tres pertinent.

### V2 internationale : Hybride avec LLM fallback

Pour l'expansion internationale, l'architecture doit supporter :

1. **Seed local extensible par pays.** Ajouter des jeux de seed par marche (UAE, France, UK, USA). Les INN restent les memes, seuls les noms commerciaux et disponibilites changent.
2. **LLM fallback pour les interactions inconnues** (deja prevu en Phase 2 de la spec). Quand un medicament n'est pas dans le catalogue ou n'a pas d'interactions documentees, l'AI module peut utiliser un LLM (GPT-4 / Claude) pour generer un avis pharmacologique avec disclaimer obligatoire.
3. **Partenariat eventuel avec Plumb's.** Si le volume d'utilisateurs le justifie (>500 cliniques), un partenariat B2B avec Plumb's pourrait fournir des donnees structurees. Ceci n'est pas un prerequis.
4. **FDA Green Book pour enrichir les noms commerciaux USA.** L'API openFDA est gratuite et peut enrichir les entrees du catalogue avec les noms commerciaux americains, mais ne fournit ni interactions ni dosages.

### Compatibilite architecturale

L'architecture actuelle (DrugCatalogEntry en base avec ClinicId nullable) est **parfaitement compatible** avec les deux approches :

- Les entrees globales (ClinicId = null) servent de seed. Elles peuvent etre mises a jour via migration EF Core lors des montees de version.
- Les entrees clinique (ClinicId = X) permettent la personnalisation locale.
- L'ajout futur d'un champ `Source` (enum : Seed, FdaGreenBook, PlumbsPartnership, ClinicCustom) sur DrugCatalogEntry permettrait de tracer la provenance sans modifier la structure.
- L'interface `IInteractionCheckService` dans AI.Contracts permet de brancher un LLM fallback sans toucher a MedicalRecords.

---

## 5. Actions recommandees

| # | Action | Priorite | Quand |
|---|---|---|---|
| 1 | Confirmer l'approche seed local pour le MVP | **Maintenant** | Avant debut Phase 1 |
| 2 | Faire valider le seed par un pharmacologue veterinaire (freelance, ~5 jours de travail) | Haute | Avant mise en production |
| 3 | Ajouter un champ `Source` sur DrugCatalogEntry pour anticiper l'hybride | Basse | Phase 1 ou apres |
| 4 | Evaluer le partenariat Plumb's quand >100 cliniques actives | Basse | V2+ |
| 5 | Implementer le LLM fallback dans l'AI module (deja prevu Phase 2) | Moyenne | Phase 2 |

---

## 6. Verdict

**Seed local confirme pour le MVP.** Aucune API veterinaire ne justifie une dependance externe aujourd'hui. L'architecture en place (DrugCatalogEntry + IInteractionCheckService) est compatible avec un enrichissement futur sans refactoring. Le seul investissement non-technique necessaire est la validation du seed par un professionnel de la pharmacologie veterinaire avant la mise en production.
