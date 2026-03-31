# Dossier Medical Veterinaire Partage — Etude d'opportunite

> Date: 2026-03-31 | Auteur: Claude (etude approfondie) | Destinataire: Fondateur Vetolib

---

## Executive Summary

Le dossier medical veterinaire partage est un **espace blanc mondial**. Aucun acteur n'a reussi a creer un equivalent du DMP/Pro Sante Connect pour les animaux. Les standards existent (VetXML, FHIR Patient-Animal extension) mais l'adoption est quasi-nulle. Le microchip ISO 11784/11785 offre un identifiant universel deja implante sur des centaines de millions d'animaux. Le pain point est reel (referals, urgences, erreurs medicales par manque d'historique). Le timing est bon: pet insurance en croissance de 11-15%/an, digitalisation veterinaire en acceleration post-COVID, zero standard dominant.

**Recommandation**: Construire le premier reseau d'echange de dossiers veterinaires, en commencant par les referals (cliniques de reference / urgences) aux UAE, puis France. Modele: freemium, export gratuit, import/consultation de l'historique payant. Utiliser le microchip comme cle primaire universelle.

---

## 1. Existant mondial — Qui fait quoi ?

### Standards existants

| Standard | Statut | Adoption | Pertinence Vetolib |
|---|---|---|---|
| **VetXML** (UK, 2006) | Actif mais niche | Schemas pour eClaims assurance, microchip registration, resultats labo. Utilise par quelques PMS au UK | Bon point de depart pour le format d'echange labo/assurance |
| **FHIR R4 Patient-Animal Extension** | Officiel HL7 | Extension `patient-animal` avec species/breed/genderStatus. Minimaliste, prevu pour etre etendu | **Meilleur choix technique** — standard vivant, ecosysteme FHIR massif, extensible |
| **ISO 11784/11785** | Standard mondial | Micropuce 15 digits, FDX-B 134.2 kHz. Adopte par quasi tous les pays sauf quelques fabricants US (AVID) | **Identifiant universel deja en place** — la cle primaire du dossier |

### Initiatives nationales de collecte de donnees

| Pays/Region | Initiative | Type | Limites |
|---|---|---|---|
| **UK** | SAVSNET (Univ. Liverpool, 2008) | Surveillance epidemiologique, donnees de-identifiees, ~10% des cliniques UK | Pas un dossier partage — recherche/surveillance uniquement, volontaire |
| **Australie** | VetCompass Australia (7 universites) | Big data collecte automatique de records de-identifies depuis les PMS | Meme modele que SAVSNET — recherche, pas partage clinique |
| **Nordiques** (SE, DK, NO, FI) | Bases nationales obligatoires | Enregistrement obligatoire des maladies bovines/porcines depuis les 1970s | **Livestock uniquement**, pas companion animals |
| **USA** | CAVSNET (Univ. Minnesota) | Companion Animal Veterinary Surveillance Network | Emergent, recherche academique |
| **France** | I-CAD | Fichier national d'identification (42M+ animaux) | **Identification seulement** — zero donnee medicale |

### Plateformes industrielles

| Plateforme | Ce que c'est reellement | Dossier partage ? |
|---|---|---|
| **IDEXX VetConnect Plus** | Visualisation de resultats diagnostiques IDEXX (labo + analyseurs in-clinic). Partage de resultats avec referals/clients en 1 clic. AI DecisionIQ. | **Resultats de labo IDEXX uniquement** — pas un dossier medical complet. Walled garden IDEXX. |
| **Zoetis Vetscan Hub** | Plateforme de connectivite diagnostique Zoetis. Sync bidirectionnelle avec PIMS. AI Imagyst (fecal, urine, dermato, cytologie). | **Diagnostics Zoetis uniquement** — meme modele walled garden que IDEXX |
| **Covetrus Pulse** | PMS cloud avec 250+ integrations, AI SOAP notes, treatment boards. | PMS classique, pas d'interoperabilite inter-cliniques |
| **Digitail** | PMS cloud avec SOAP templates, API REST, connections labo | Pas de partage inter-cliniques |
| **ezyVet** (IDEXX) | PMS cloud flagship IDEXX | Integration profonde IDEXX mais pas de partage standard |
| **Shepherd + PetDesk** | PMS + communication client, sync bidirectionnelle | Communication client, pas partage inter-cliniques |
| **Instinct (Shareville)** | EMR veterinaire specialite/urgences | **Seul acteur avec partage**: portail unidirectionnel de records vers referals via lien securise |

### Conclusion axe 1

**PERSONNE ne fait de dossier medical veterinaire partage bidirectionnel et standardise.** IDEXX et Zoetis partagent uniquement leurs propres resultats diagnostiques. Instinct (Shareville) est le seul a offrir un partage de records, mais unidirectionnel et limite a leur ecosysteme. C'est un espace blanc massif.

---

## 2. Reglementaire

### UAE (marche primaire Vetolib)

| Aspect | Regle | Impact Vetolib |
|---|---|---|
| **Microchip obligatoire** | ISO 11784/11785 requis pour import/export. Rabies vaccine doit etre APRES le microchipping | Microchip = cle primaire fiable |
| **MOCCAE** | Health certificates obligatoires pour import/export, incluant microchip + vaccinations + historique sanitaire | Opportunite: generer automatiquement le certificat MOCCAE depuis le dossier partage |
| **Data residency** | UAE Personal Data Protection Law (PDPL, 2022) — donnees personnelles des residents UAE a heberger dans les UAE ou pays approuves | Heberger les donnees UAE sur AWS/Azure region UAE (disponible) |
| **Pas de HDS equivalent** | Pas de certification specifique pour donnees de sante animale | Moins de contraintes qu'en France |

### France (marche secondaire)

| Aspect | Regle | Impact Vetolib |
|---|---|---|
| **Code rural + Code de deontologie** | Dossier medical = document medico-legal. Conservation minimum 5 ans (pratique). Lacunes = responsabilite du praticien | Le dossier partage doit etre un complement, pas un remplacement du dossier local |
| **Registre medicaments** | Conservation 5 ans, accessible en cas de controle (DGAL) | Opportunite: tracer les prescriptions dans le dossier partage |
| **I-CAD** | 42M+ animaux identifies, delegation de service public depuis 2013. Microchip + tatouage | **Partenariat strategique potentiel** — I-CAD a l'ID, Vetolib a le dossier medical |
| **Ordre des veterinaires** | Controle disciplinaire, peut sanctionner dossiers incomplets | Le dossier partage aide a la conformite |
| **HDS** | Certification obligatoire pour hebergement de donnees de sante personnelles. V2.0 impose stockage EEA. | **Question cle**: les donnees veterinaires sont-elles des "donnees de sante" au sens HDS ? Probablement NON (sante animale =/= sante humaine) mais les donnees du proprietaire (nom, adresse) sont des donnees personnelles GDPR classiques |

### EU

| Aspect | Regle | Impact Vetolib |
|---|---|---|
| **Animal Health Law (Reg. 2016/429, applique 2021)** | Certificats sanitaires harmonises, systeme TRACES pour mouvements transfrontaliers | Opportunite: integration avec TRACES pour les exports |
| **Passeport europeen** | Microchip + rabies + infos sanitaires, delivre par veto autorise | Le dossier partage pourrait etre le backbone digital du passeport |
| **GDPR** | **L'animal n'est PAS une personne** — ses donnees medicales ne sont pas des "donnees de sante" GDPR. MAIS le proprietaire est une personne — ses donnees (nom, adresse, telephone) sont protegees | Consentement du proprietaire obligatoire pour le partage. Pas de special category data pour l'animal lui-meme |

### Recommandation reglementaire

1. **UAE first**: moins de contraintes reglementaires, microchip deja obligatoire, pas de HDS
2. **France second**: eviter HDS en classant les donnees comme "donnees veterinaires" (pas "donnees de sante humaine"), GDPR classique pour les donnees proprietaire
3. **Consentement**: opt-in explicite du proprietaire pour chaque partage de dossier, audit trail complet

---

## 3. Technique

### Format de donnees: FHIR R4 + extensions veterinaires

**Recommandation: FHIR R4 avec l'extension `patient-animal`** pour les raisons suivantes:
- Standard vivant avec communaute massive (vs VetXML moribond)
- Extension officielle HL7 pour les animaux (species, breed, genderStatus)
- Ecosysteme d'outils (parsers, validators, serveurs FHIR) deja mature
- Facilite les partenariats avec les labos/assureurs qui parlent deja FHIR
- One Health: passerelle future vers les systemes de sante humaine (zoonoses)

### Modele de donnees du dossier partage

```
Animal (Patient FHIR + extension animal)
├── Identifiers: microchip ISO (primary), tattoo, passport number
├── Species, Breed, DateOfBirth, Sex, NeuterStatus, Weight[]
├── Owner (RelatedPerson FHIR) → lien GDPR-protected
├── Encounters[] (consultations)
│   ├── Date, Clinic, Practitioner
│   ├── Reason, Diagnosis (SNOMED-CT ou VeNom codes)
│   ├── Observations[] (poids, temperature, etc.)
│   ├── Procedures[] (chirurgies, actes)
│   ├── MedicationRequests[] (prescriptions)
│   └── DiagnosticReports[] (labo, imagerie)
├── Immunizations[] (vaccinations + rappels)
├── Allergies/Intolerances[]
├── Conditions[] (pathologies chroniques)
└── Documents[] (PDF, images, rapports externes)
```

### Identite unique de l'animal

| Methode | Fiabilite | Couverture |
|---|---|---|
| **Microchip ISO 15 digits** | Tres haute (implante, quasi-impossible a falsifier) | Tres haute dans UAE/EU (obligatoire). Plus faible USA |
| Tatouage | Moyenne (peut s'effacer) | En declin, encore present en France |
| Passport number | Haute | EU uniquement |
| Composite (microchip + owner) | Maximale | Gere les cas sans microchip |

**Decision**: microchip ISO comme identifiant primaire. Fallback sur composite (nom animal + DOB + owner) si pas de microchip.

### Architecture d'echange

```
Clinique A (Vetolib)          Hub Vetolib              Clinique B (Vetolib ou tiers)
     │                            │                            │
     ├── POST /records ──────────>│                            │
     │   (FHIR Bundle)           │                            │
     │                            │<── GET /records?chip=XXX ──┤
     │                            │    (avec consentement)     │
     │                            ├── FHIR Bundle ────────────>│
     │                            │                            │
     │<── Webhook notification ───┤                            │
     │   (nouveau record dispo)  │                            │
```

- **API REST FHIR R4** pour l'echange (pas de fichiers XML a transferer manuellement)
- **Consentement**: le proprietaire autorise le partage via l'app Vetolib (QR code ou lien)
- **Granularite**: partage par encounter, par categorie (vaccinations only, full record) ou par periode
- **Audit trail**: chaque acces est logue (qui, quand, quoi, pourquoi)

### Hebergement et securite

| Marche | Hebergement | Justification |
|---|---|---|
| UAE | AWS/Azure UAE region | Data residency PDPL |
| France/EU | AWS/Azure EU region (Paris/Francfort) | GDPR, pas besoin de HDS pour donnees veterinaires |

- **Chiffrement**: AES-256 at rest, TLS 1.3 in transit
- **Controle d'acces**: RBAC par clinique + consentement proprietaire
- **Zero-knowledge optional**: chiffrement cote client pour les cliniques qui le demandent

---

## 4. Business model

### Qui paie ?

| Modele | Avantages | Inconvenients | Verdict |
|---|---|---|---|
| **Clinique emettrice paie (export)** | Incite a exporter | Frein a l'adoption — "pourquoi payer pour donner mes donnees ?" | Non |
| **Clinique receptrice paie (import)** | Valeur claire — acces a l'historique complet | Le referal/urgence a le pain point, il paie volontiers | **Oui — modele principal** |
| **Proprietaire paie** | Empower le proprietaire | Faible willingness-to-pay, frein a l'adoption massive | Non pour le lancement |
| **Freemium** | Export gratuit + consultation basique gratuite. Import complet/historique/API = payant | Maximise l'adoption (export gratuit = zero friction pour alimenter le reseau) | **Oui — combine avec receptrice paie** |

### Modele recommande

```
Tier Gratuit (toutes les cliniques Vetolib)
├── Export automatique des records vers le hub (zero effort)
├── Consultation du dernier encounter d'un animal
├── Vaccinations et allergies toujours visibles
└── Notification "un dossier existe pour cet animal"

Tier Pro (abonnement mensuel par clinique)
├── Historique complet multi-cliniques
├── Resultats de labo detailles
├── Timeline chronologique interactive
├── API d'integration PIMS
├── Export FHIR pour referals
└── Rapports analytiques (population, pathologies)

Tier Enterprise (cliniques de reference, CHV, urgences)
├── Tout Pro +
├── Reception automatique des dossiers pre-referal
├── Integration bidirectionnelle PIMS
├── Dashboard multi-sites
└── SLA + support dedie
```

### Pricing indicatif

| Tier | Prix UAE | Prix France |
|---|---|---|
| Gratuit | 0 AED | 0 EUR |
| Pro | 200-400 AED/mois (~55-110 EUR) | 50-100 EUR/mois |
| Enterprise | 800-1500 AED/mois (~220-410 EUR) | 150-300 EUR/mois |

### Comparaison DMP public vs Doctolib prive

| Aspect | DMP/Pro Sante Connect (public) | Doctolib (prive) | Vetolib (recommande) |
|---|---|---|---|
| Financement | Assurance maladie | SaaS B2B (clinique paie ~129 EUR/mois) | SaaS B2B freemium |
| Adoption | Lente (10+ ans, adoption forcee) | Rapide (network effects, valeur immediate) | **Modele Doctolib**: valeur immediate, pas attendre un mandat gouvernemental |
| Interoperabilite | Imposee par decret | Proprietaire (lock-in) | **Standard ouvert FHIR** — differenciateur vs Doctolib-like |
| Identite | Carte Vitale / INS | Email/telephone | **Microchip** (plus fiable que tout identifiant humain) |

### Partenariats strategiques

| Partenaire | Valeur | Priorite |
|---|---|---|
| **Assureurs animaux** (SantéVet, PetPlan, Trupanion) | eClaims automatises depuis le dossier, reduction fraude | Haute — pet insurance croit 11-15%/an |
| **IDEXX** | Integration VetConnect Plus → enrichir le dossier avec resultats labo | Haute — mais IDEXX pourrait vouloir controler |
| **Zoetis** | Integration Vetscan Hub → resultats diagnostiques | Haute |
| **I-CAD** (France) | Lier identification + dossier medical | Tres haute — partenariat public/prive unique |
| **Registres de race** (LOF, KC) | Donnees genetiques, historique de sante de la lignee | Moyenne |
| **Cliniques universitaires** | Early adopters naturels, credibilite, recherche | Tres haute pour le lancement |

---

## 5. Marketing & Sales

### Pain points documentes

| Pain point | Qui souffre | Severite |
|---|---|---|
| **Historique manquant en referal/urgence** | Veto referent, urgentiste | **Critique** — decisions medicales sans contexte, risque d'erreur, re-examens inutiles |
| **Transfert de records par fax/email/telephone** | CSR (reception), veto referent | Haute — workflow archaique, perte de temps (printing PDFs, scanning, faxing, calling to confirm) |
| **Re-saisie manuelle des donnees** | Veto receveur | Haute — 15-30 min par dossier transfere |
| **Vaccinations inconnues** | Veto, proprietaire qui change de clinique | Moyenne — risque de sur-vaccination ou sous-vaccination |
| **Continuite de soins animaux chroniques** | Veto, proprietaire | Haute — diabete, insuffisance renale, epilepsie necessitent un suivi longitudinal |
| **Certificat MOCCAE/export incomplet** | Proprietaire, veto certificateur | Moyenne — retards de voyage, stress |

### Pitch aux veterinaires

**Pour les cliniques de reference / urgences (early adopters):**
> "Recevez l'historique complet de vos patients referes AVANT qu'ils arrivent. Plus de fax, plus de coups de fil, plus de decisions a l'aveugle. Le dossier arrive avec l'animal."

**Pour les cliniques generalistes:**
> "Vos records suivent vos patients meme quand ils changent de clinique ou vont en urgence. Votre travail n'est jamais perdu. Et c'est gratuit a exporter."

**Pour les proprietaires:**
> "L'historique medical complet de votre animal, accessible partout, a tout moment. Changez de veterinaire sans perdre 10 ans de suivi."

### Strategie d'adoption (resoudre le chicken-and-egg)

**Phase 1 — Supply side first (les cliniques qui exportent)**
- Export gratuit et automatique pour toutes les cliniques Vetolib existantes
- Zero friction: le dossier est partage des qu'une consultation est enregistree
- Les cliniques Vetolib alimentent le reseau sans effort supplementaire

**Phase 2 — Demand side (les cliniques qui importent)**
- Cibler les referals: CHV, urgences, specialistes (ophtalmologie, dermatologie, oncologie)
- Leur montrer le volume de dossiers deja disponibles ("150 dossiers de vos patients referes vous attendent")
- Ils paient pour acceder a l'historique complet

**Phase 3 — Network effects**
- Les referals demandent aux cliniques generalistes de "partager via Vetolib"
- Les proprietaires decouvrent le service quand leur referal leur dit "j'ai deja votre dossier"
- Les assureurs integrent pour l'eClaims automatise

### Early adopters prioritaires

1. **Cliniques universitaires veterinaires** (CHV Maisons-Alfort, RVC London, cliniques UAE) — recoivent massivement des referals, souffrent le plus du manque d'historique
2. **Cliniques d'urgences veterinaires** — decisions critiques sans contexte
3. **Specialistes** (dermato, ophtalmo, onco, cardio) — besoin de l'historique pour le diagnostic differentiel
4. **Groupes veterinaires multi-sites** (IVC Evidensia, AniCura/Mars, CVS Group) — partage interne deja un besoin

---

## 6. Competition

### Matrice concurrentielle

| Acteur | Dossier partage | Standard ouvert | Multi-PMS | Independant | Menace |
|---|---|---|---|---|---|
| **IDEXX VetConnect Plus** | Resultats labo uniquement | Non (proprietaire) | Integration ezyVet + quelques PMS | Non (IDEXX only) | **Moyenne** — pourrait etendre mais walled garden |
| **Zoetis Vetscan Hub** | Diagnostics uniquement | Non | Integration Covetrus + quelques PMS | Non (Zoetis only) | **Moyenne** — meme logique qu'IDEXX |
| **Instinct Shareville** | Oui (unidirectionnel) | Non | Non (Instinct only) | Non | **Faible** — niche specialite/urgences USA |
| **Digitail** | Non | API REST | Non | Oui | **Faible** — PMS cloud, pas de partage |
| **ezyVet** | Non | Non | Non (IDEXX) | Non | **Faible** — PMS, pas de vision interop |
| **Provet Cloud** | Non | Non | Non | Oui | **Faible** |
| **Covetrus Pulse** | Non | Non | 250+ integrations | Non (Patterson) | **Faible** — integrations =/= partage de dossiers |
| **VetCompass/SAVSNET** | Recherche uniquement | Non | Oui (extraction) | Oui (academique) | **Nulle** — pas un produit commercial |

### Avantage concurrentiel Vetolib

1. **Premier a faire du partage bidirectionnel standardise** — personne ne le fait
2. **Standard ouvert (FHIR)** vs walled gardens IDEXX/Zoetis — les cliniques veulent l'independance
3. **Microchip comme cle primaire** — pas de creation de compte, pas de matching approximate
4. **Multi-marche natif** (UAE + France) — aucun concurrent n'est positionne sur les deux
5. **Deja dans le PMS** — Vetolib peut activer le partage pour ses cliniques existantes sans friction

---

## 7. Risques

### Risques et mitigations

| Risque | Probabilite | Impact | Mitigation |
|---|---|---|---|
| **Resistance des vetos au partage** ("mes patients, mes donnees") | Haute | Critique | Export gratuit + consentement proprietaire (le veto n'a pas le choix si le proprietaire demande). Education: "le dossier appartient au proprietaire, pas a la clinique" |
| **IDEXX/Zoetis lancent leur propre solution** | Moyenne | Haute | Avantage du standard ouvert + independance. Les cliniques ne veulent pas etre lock-in sur un labo |
| **Responsabilite si dossier partage contient une erreur** | Moyenne | Haute | Disclaimer clair: "chaque clinique est responsable de ce qu'elle ecrit". Audit trail immutable. Le dossier partage est une COPIE, pas le dossier officiel |
| **Adoption trop lente pour atteindre la masse critique** | Haute | Critique | Phase 1 automatique pour cliniques Vetolib (zero effort). Cibler les referals qui ont le pain point le plus fort |
| **Competition des registres nationaux (I-CAD evolue)** | Faible | Moyenne | I-CAD est identification, pas dossier medical. Partenariat plutot que competition |
| **Cout de developpement eleve** | Moyenne | Moyenne | MVP: partage vaccinations + allergies + diagnostics uniquement. Pas besoin du dossier complet au lancement |
| **GDPR/privacy incidents** | Faible | Haute | Consentement explicite, audit trail, chiffrement, data residency locale |
| **Fragmentation des PMS** | Haute | Moyenne | API FHIR standard + programme de certification pour les PMS tiers |

### Risque specifique: la propriete du dossier veterinaire

En droit francais, le dossier veterinaire est un document medico-legal tenu par le praticien. Contrairement a la medecine humaine ou le patient a un droit d'acces explicite (Loi Kouchner), le cadre est plus flou en veterinaire. **Le proprietaire de l'animal a un droit d'acces aux informations mais pas forcement au dossier brut.**

**Mitigation**: positionner le dossier partage comme un "resume structure" (vaccinations, diagnostics, traitements) plutot qu'une copie du dossier brut. Le veto choisit ce qui est partage.

---

## 8. Roadmap recommandee

### Phase 1 — MVP (3-6 mois)
- Partage automatique depuis les cliniques Vetolib existantes (vaccinations + allergies + diagnostics)
- Recherche par microchip
- Consentement proprietaire (QR code)
- Interface consultation pour referals

### Phase 2 — Traction (6-12 mois)
- Onboarding cliniques de reference / urgences UAE
- Historique complet multi-cliniques
- Integration IDEXX VetConnect Plus (resultats labo)
- eClaims assureurs

### Phase 3 — Scale (12-24 mois)
- France launch avec partenariat I-CAD
- API FHIR publique pour PMS tiers
- Integration Zoetis diagnostics
- Passeport veterinaire digital (generation certificats MOCCAE/EU)

### Phase 4 — Plateforme (24+ mois)
- Analytics population (epidemiologie, pharmacovigilance)
- AI: detection de patterns pathologiques cross-cliniques
- Marketplace: recommandations de specialistes basees sur le dossier
- One Health: passerelle zoonoses vers systemes sante humaine

---

## 9. Chiffres cles

| Metrique | Valeur | Source |
|---|---|---|
| Marche mondial PMS veterinaire | ~660-880M USD (2025), CAGR 8-9% | Research and Markets, Coherent MI |
| Pet insurance mondial | 14.35B USD (2025), CAGR 15.7% | Mordor Intelligence |
| Pet insurance France | 4.1% du marche mondial, CAGR 13.6% | Grand View Research |
| Pet insurance UAE | 16.4M USD (2024), CAGR 9.6% | Grand View Research |
| Animaux identifies I-CAD France | 42M+ | I-CAD |
| Microchips ISO deployes mondialement | Centaines de millions (pas de chiffre exact public) | WSAVA |
| Cliniques UK sur SAVSNET | ~10% (volontaire) | Univ. Liverpool |
| Temps perdu transfert dossier (fax/email) | 15-30 min par dossier | VCA Hospitals, Instinct |

---

## Sources

- [VetXML Consortium](https://www.co.vet/post/veterinary-electronic-medical-record)
- [FHIR R4 Patient-Animal Extension](https://www.hl7.org/FHIR/R4/extension-patient-animal.html)
- [FHIR Animal Species ValueSet](https://build.fhir.org/ig/HL7/fhir-extensions//ValueSet-animal-species.html)
- [IDEXX VetConnect PLUS](https://www.idexx.com/en/veterinary/software-services/vetconnect-plus/)
- [Zoetis Diagnostics](https://www.zoetis.com/products-and-science/diagnostics)
- [Zoetis Vetscan Hub](https://www.zoetisdiagnostics.com/us/virtual-laboratory/connectivity/vetscan-hub)
- [MOCCAE Import Pets](https://www.moccae.gov.ae/en/services/import-permit-pets)
- [MOCCAE Pet Import Guide](https://www.carrymypet.ae/blog/view/bringing-your-pet-to-the-uae-a-complete-guide-to-moccae-import-permits-and-regulations)
- [Code de deontologie veterinaire (Legifrance)](https://www.legifrance.gouv.fr/codes/id/LEGISCTA000006168195)
- [Dossier medical veterinaire (co.vet)](https://www.co.vet/fr/post/dossier-m%C3%A9dical-v%C3%A9t%C3%A9rinaire)
- [I-CAD France](https://www.i-cad.fr/)
- [I-CAD Wikipedia](https://fr.wikipedia.org/wiki/I-CAD)
- [EU Pet Passport Requirements](https://europa.eu/youreurope/citizens/travel/carry/pets-and-other-animals/index_en.htm)
- [EU Animal Health Law Entry Requirements](https://food.ec.europa.eu/animals/movement-pets/eu-legislation/entry-union_en)
- [GDPR Veterinary Guide (BVA)](https://www.bva.co.uk/resources-support/practice-management/general-data-protection-regulation-gdpr-guide/)
- [GDPR Veterinary Practice](https://www.veterinary-practice.com/article/gdpr-back-to-basics)
- [HDS France (Microsoft)](https://learn.microsoft.com/en-us/compliance/regulatory/offering-hds-france)
- [HDS v2.0 Data Localisation](https://www.fieldfisher.com/en/insights/data-localisation-rules-hds-certification-framework)
- [SAVSNET (Univ. Liverpool)](https://www.liverpool.ac.uk/savsnet/)
- [VetCompass Australia](https://www.vetcompass.com.au/)
- [VetCompass Australia Paper (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC5664033/)
- [Nordic Veterinary Disease Databases](https://link.springer.com/article/10.1186/1751-0147-42-S1-S51)
- [Covetrus Pulse](https://covetrus.com/covetrus-platform/workflow-and-productivity-tools/covetrus-pulse/)
- [Digitail](https://digitail.com/)
- [Shepherd + PetDesk Integration](https://www.prnewswire.com/news-releases/petdesk-expands-shepherd-integration-creating-a-fully-unified-pims--client-engagement-solution-302528517.html)
- [Instinct Shareville](https://pickthebrain.instinct.vet/how-instinct-is-making-record-sharing-easier-for-veterinary-hospitals/)
- [ISO 11784/11785 (Wikipedia)](https://en.wikipedia.org/wiki/ISO_11784_and_ISO_11785)
- [WSAVA Microchip Guidelines](https://wsava.org/global-guidelines/microchip-identification-guidelines/)
- [VCA Hospitals - Sharing Medical Records](https://vcahospitals.com/know-your-pet/the-importance-of-sharing-medical-records)
- [Veterinary Informatics (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC7382640/)
- [EVMR Adoption Survey (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC4782149/)
- [Pet Insurance Market (Mordor Intelligence)](https://www.globenewswire.com/news-release/2026/03/30/3264543/0/en/Pet-Insurance-Market-Set-to-Grow-at-11-23-CAGR-to-Reach-USD-29-94-Billion-by-2031-Says-Mordor-Intelligence.html)
- [France Pet Insurance Market](https://www.grandviewresearch.com/horizon/outlook/pet-insurance-market/france)
- [UAE Pet Insurance Market](https://www.grandviewresearch.com/horizon/outlook/pet-insurance-market/uae)
- [Veterinary PMS Market](https://www.researchandmarkets.com/report/veterinary-practice-management-software)
- [Doctolib Business Model (Contrary Research)](https://research.contrary.com/company/doctolib)
- [Doctolib vs French State (France24)](https://www.france24.com/en/europe/20211224-success-of-online-medical-portal-doctolib-highlights-the-french-state-s-failure-to-digitise)
- [DMP Pro Sante Connect](https://esante.gouv.fr/espace-presse/un-acces-facilite-pour-les-professionnels-aux-services-socles-du-numerique-en-sante-en-2023-avec-pro-sante-connect)
- [Veterinary Malpractice (Animal Law Center)](https://www.animallaw.info/article/detailed-discussion-veterinarian-malpractice)
- [Network Effects Manual (NFX)](https://www.nfx.com/post/network-effects-manual)
- [Two-Sided Markets Pricing (Northwestern)](https://faculty.wcas.northwestern.edu/apa522/Two-Sided-Market-and-Network-Effects.pdf)
