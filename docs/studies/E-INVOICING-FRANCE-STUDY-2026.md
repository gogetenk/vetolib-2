# Facturation electronique obligatoire en France -- Impact sur Vetolib

> Etude architecturale -- Mars 2026
> Auteur : Agent architecte Vetolib
> Statut : DRAFT -- a valider avec PO et expert-comptable

---

## Table des matieres

1. [Contexte reglementaire](#1-contexte-reglementaire)
2. [Calendrier d'application](#2-calendrier-dapplication)
3. [Obligations pour les cabinets veterinaires](#3-obligations-pour-les-cabinets-veterinaires)
4. [Formats techniques](#4-formats-techniques)
5. [Architecture de la reforme : PPF, PDP, OD](#5-architecture-de-la-reforme--ppf-pdp-od)
6. [Analyse d'impact sur le module Billing de Vetolib](#6-analyse-dimpact-sur-le-module-billing-de-vetolib)
7. [Decisions architecturales](#7-decisions-architecturales)
8. [Plan de taches](#8-plan-de-taches)
9. [Risques et questions ouvertes](#9-risques-et-questions-ouvertes)
10. [Sources et references](#10-sources-et-references)

---

## 1. Contexte reglementaire

La reforme de la facturation electronique en France a ete inscrite dans la **loi de finances 2024 (article 91)**, modifiant les articles 289 et 290 du Code General des Impots. Elle impose progressivement a toutes les entreprises assujetties a la TVA :

- **L'e-invoicing** : emission et reception de factures au format electronique structure entre assujettis a la TVA (B2B domestique).
- **L'e-reporting** : transmission de donnees de transaction a l'administration fiscale pour les operations non couvertes par l'e-invoicing (B2C, international, exonere).

### Pourquoi cette reforme ?

- Lutte contre la fraude a la TVA (estimee a 20-25 milliards EUR/an en France)
- Pre-remplissage des declarations de TVA
- Amelioration de la connaissance en temps reel de l'activite economique
- Alignement avec les directives europeennes (ViDA -- VAT in the Digital Age)

---

## 2. Calendrier d'application

Le calendrier a ete repousse plusieurs fois. Le calendrier en vigueur (ordonnance du 21 septembre 2023 + decret du 1er novembre 2024) :

| Date | Obligation | Qui est concerne |
|---|---|---|
| **1er septembre 2026** | Reception obligatoire de factures electroniques | **Toutes les entreprises** (y compris PME, TPE, micro) |
| **1er septembre 2026** | Emission obligatoire + e-reporting | Grandes entreprises (CA > 10M EUR) et ETI |
| **1er septembre 2027** | Emission obligatoire + e-reporting | PME et micro-entreprises |

### Impact pour les cabinets veterinaires

Un cabinet veterinaire est typiquement une **TPE ou PME** (CA moyen entre 300K et 2M EUR). Donc :

- **1er septembre 2026** : obligation de RECEVOIR des factures electroniques (fournisseurs, labos, grossistes qui les emettront)
- **1er septembre 2027** : obligation d'EMETTRE des factures electroniques ET de faire du e-reporting

**Point critique** : la majorite des factures veterinaires sont des factures B2C (proprietaires d'animaux). Ces factures ne sont PAS soumises a l'e-invoicing mais SONT soumises au **e-reporting**.

---

## 3. Obligations pour les cabinets veterinaires

### 3.1 E-invoicing (B2B uniquement)

Concerne les factures emises vers d'autres entreprises assujetties a la TVA :
- Factures a des eleveurs professionnels
- Factures a des refuges/associations (si assujettis)
- Factures a d'autres cliniques (cas de referement)
- Factures aux assurances animales (si B2B)

**Format** : facture electronique structuree, transmise via une PDP ou le PPF.

### 3.2 E-reporting (B2C et autres)

Concerne TOUTES les autres transactions :
- **Factures aux proprietaires d'animaux (particuliers)** -- c'est 80-90% du CA d'une clinique
- Ventes exonerees de TVA
- Operations internationales

**Obligation** : transmettre les donnees de transaction (pas la facture complete) a l'administration fiscale via une PDP ou le PPF.

**Frequence de transmission e-reporting** :
- Regime reel normal : tous les 10 jours (J+10, J+20, J+30 du mois)
- Regime reel simplifie : mensuel
- Micro-entreprises : bimestriel

### 3.3 Mentions obligatoires supplementaires sur les factures

La reforme ajoute des mentions obligatoires :
- **SIREN** du vendeur et de l'acheteur (si B2B)
- **Numero de TVA intracommunautaire** du vendeur
- **Adresse de livraison** (si differente de l'adresse de facturation)
- **Type d'operation** : livraison de biens, prestation de services, mixte
- **Option de debit** (le cas echeant)
- **Reference du bon de commande** (si applicable)

### 3.4 Cycle de vie de la facture (statut obligatoire)

La reforme impose un **cycle de vie avec statuts** transmis au PPF :
- `Deposee` : facture emise sur la plateforme
- `Rejetee` : refusee par le destinataire
- `Refusee` : refusee par la plateforme (format invalide)
- `Encaissee` : paiement recu (pour le e-reporting de paiement)

**Statut de paiement** : les factures de prestation de services doivent aussi declarer les encaissements (e-reporting de paiement), car la TVA sur services est exigible a l'encaissement en France.

---

## 4. Formats techniques

### 4.1 Formats acceptes

Trois formats sont admis par le PPF et les PDP :

| Format | Type | Description |
|---|---|---|
| **Factur-X** | Hybride (PDF/A-3 + XML CII) | PDF lisible par l'humain + donnees structurees XML embarquees. Profils : Minimum, Basic, EN16931 (Comfort), Extended |
| **UBL (Universal Business Language)** | XML pur | Standard OASIS, utilise par Chorus Pro pour le secteur public. Format tres structure |
| **CII (Cross-Industry Invoice)** | XML pur | Standard UN/CEFACT. Le XML embarque dans Factur-X est du CII |

### 4.2 Recommandation pour Vetolib : Factur-X profil EN16931

**Factur-X est le choix optimal** pour plusieurs raisons :

1. **Hybride** : le PDF reste lisible par les clients (proprietaires d'animaux) tout en contenant les donnees structurees pour l'administration
2. **Compatible avec l'existant** : Vetolib genere deja des PDF via QuestPDF -- on peut migrer vers PDF/A-3 et y embarquer le XML CII
3. **Standard francais de reference** : pousse par la FNFE-MPE (Forum National de la Facture Electronique), co-developpe par la France et l'Allemagne (ZUGFeRD 2.x = Factur-X)
4. **Profil EN16931** (dit "Comfort") : contient toutes les mentions obligatoires sans la complexite du profil Extended
5. **Bibliotheques .NET disponibles** : `Factur-X.NET`, `ZUGFeRD-csharp`

### 4.3 Structure d'un Factur-X

```
facture.pdf (PDF/A-3)
  |-- factur-x.xml (CII XML embarque dans le PDF comme piece jointe)
```

Le XML CII contient :
- Identification vendeur/acheteur (SIREN, TVA)
- Lignes de facture (description, quantite, prix, TVA)
- Totaux (HT, TVA, TTC)
- Conditions de paiement
- Type d'operation

---

## 5. Architecture de la reforme : PPF, PDP, OD

### 5.1 Les trois acteurs

| Acteur | Role | Statut |
|---|---|---|
| **PPF** (Portail Public de Facturation) | Plateforme gratuite de l'Etat, successeur de Chorus Pro | Service minimum gratuit. Annuaire central. Concentrateur des donnees fiscales |
| **PDP** (Plateforme de Dematerialisation Partenaire) | Operateur prive agree par l'Etat | Services a valeur ajoutee (integration ERP, archivage legal, rapprochement auto). Payant |
| **OD** (Operateur de Dematerialisation) | Prestataire technique non agree | Prepare les factures au bon format, mais DOIT passer par le PPF ou une PDP pour la transmission |

### 5.2 Flux de transmission

```
Emetteur (Vetolib) --> PDP ou PPF --> Destinataire
                         |
                         v
                    Administration fiscale (DGFiP)
```

### 5.3 Recommandation pour Vetolib : OD qui passe par une PDP

Vetolib n'a PAS vocation a devenir PDP (cout d'immatriculation, audit, responsabilite fiscale). Vetolib doit se positionner comme **OD (Operateur de Dematerialisation)** :

1. **Generer les factures au format Factur-X** dans le module Billing
2. **Transmettre via l'API d'une PDP partenaire** (Chorus Pro / PPF en fallback gratuit)
3. **Recevoir les statuts** de la PDP (deposee, acceptee, rejetee, encaissee)

PDP partenaires potentielles pour l'integration API :
- **Chorus Pro** (PPF gratuit, API REST documentee, mais service minimum)
- **Cegid** (PDP immatriculee, forte presence chez les comptables veterinaires)
- **Sage** (PDP, integration comptable)
- **Pennylane** (PDP, API moderne, populaire chez les startups)
- **Yooz**, **Basware**, **Tungsten** (PDP internationales)

### 5.4 API Chorus Pro / PPF

L'API Chorus Pro est deja operationnelle pour le secteur public. Elle sera etendue pour le PPF :

- **Authentification** : OAuth2 / PISTE (plateforme d'API de l'Etat)
- **Depot de facture** : `POST /factures` avec le fichier Factur-X
- **Consultation statut** : `GET /factures/{id}/statut`
- **Webhook / callback** : notification de changement de statut
- **Annuaire** : recherche du destinataire par SIREN/SIRET

L'API PPF n'est pas encore finalisee (specifications prevues T1 2026). L'API PDP varie selon le fournisseur.

**Recommandation** : implementer une **abstraction (interface)** dans les Contracts qui permette de brancher n'importe quelle PDP. Ne pas se coupler a Chorus Pro directement.

---

## 6. Analyse d'impact sur le module Billing de Vetolib

### 6.1 Etat actuel du module Billing

Le module est concu pour le marche UAE :

| Element | Etat actuel | Besoin France |
|---|---|---|
| Devise | AED hardcode | EUR pour la France |
| TVA | 5% UAE hardcode (`TaxRate = 0.05m`) | 20% standard, 10% reduit (medicaments veterinaires), 5.5% (alimentation animale) |
| Format facture | PDF simple via QuestPDF | Factur-X (PDF/A-3 + XML CII) |
| Numero TVA | TRN (Tax Registration Number UAE) | Numero TVA intracommunautaire (FR + 11 chiffres) |
| SIREN/SIRET | Non existant | Obligatoire |
| Statuts facture | Draft, Sent, Paid, Cancelled | + Deposee, Rejetee, Refusee, Encaissee (cycle PPF) |
| E-reporting | Non existant | Obligatoire pour B2C |
| Transmission | Aucune (PDF telecharge) | API PDP/PPF |
| Mentions legales | Minimales | 15+ mentions obligatoires France |
| Type d'operation | Non existant | Obligatoire (service, bien, mixte) |
| Archivage legal | Non existant | 10 ans (obligations fiscales francaises) |

### 6.2 Impacts techniques identifies

#### Impact 1 : Multi-pays dans le Domain (MAJEUR)

Le modele `Invoice` et `InvoiceItem` ont la TVA hardcodee a 5%. Il faut :
- Rendre le taux de TVA configurable par pays ET par type de produit/service
- Supporter les taux multiples sur une meme facture (acte veterinaire a 20% + medicament a 10%)
- Ajouter les concepts de `TaxCategory` et `TaxScheme`

#### Impact 2 : Generation Factur-X (MAJEUR)

Le `InvoicePdfGenerator` actuel genere un PDF simple. Il faut :
- Generer un **PDF/A-3** (norme ISO 19005-3) au lieu d'un PDF standard
- Embarquer le **XML CII (Cross-Industry Invoice)** dans le PDF
- Valider le XML contre le schema Factur-X EN16931
- Ajouter toutes les mentions legales obligatoires

#### Impact 3 : Transmission PDP/PPF (MAJEUR)

Nouveau flux complet :
- Interface `IEInvoicingGateway` dans les Contracts
- Implementation pour chaque PDP (Chorus Pro, Pennylane, etc.)
- Gestion des retours de statut (webhook ou polling)
- File d'attente pour les envois (resilience)

#### Impact 4 : E-reporting B2C (MAJEUR)

80-90% des factures veterinaires sont B2C. Necessaire :
- Agregation des donnees de transaction par periode (J+10 ou mensuel)
- Transmission des montants HT, TVA, TTC par taux
- Transmission des statuts de paiement (TVA a l'encaissement pour les services)

#### Impact 5 : Nouveaux champs Domain (MOYEN)

Ajouter a `Invoice` :
- `BuyerSiren` / `BuyerVatNumber` (si B2B)
- `SellerSiren` / `SellerVatNumber` (de la clinique)
- `OperationType` (Service, Goods, Mixed)
- `PaymentTerms` / `PaymentDueDate` (conditions de paiement)
- `InvoiceTypeCode` (380 = facture, 381 = avoir, 386 = facture prepayee)
- `EInvoicingStatus` (cycle de vie PPF)
- `CountryCode` (pour determiner les regles applicables)

#### Impact 6 : Devise multi-pays (MOYEN)

- Extraire la devise de la configuration clinique (AED, EUR, PLN)
- Le format Factur-X exige le code devise ISO 4217

#### Impact 7 : Archivage legal (MINEUR a court terme)

- Les factures electroniques doivent etre archivees 10 ans en France
- Format d'archivage : le Factur-X original (PDF/A-3 + XML)
- Peut etre delegue a la PDP dans un premier temps

---

## 7. Decisions architecturales

### ADR-1 : Strategie par pays via configuration, pas par code

```
Billing.Domain reste agnostique du pays.
La configuration (taux TVA, mentions, format) est resolue
par un service ICountryBillingPolicy injecte au runtime.
```

Justification : Vetolib cible UAE (MVP), France, Pologne. Hardcoder les regles par pays dans le Domain serait un cauchemar de maintenance. Le Domain manipule des concepts generiques (TaxRate, TaxCategory) et un service de politique par pays fournit les valeurs.

### ADR-2 : Factur-X genere dans une sous-couche Infrastructure

```
Domain --> Application (Handler) --> Infrastructure (FacturXGenerator)
                                      |-- IFacturXGenerator (dans Contracts)
                                      |-- FacturXGenerator (dans Billing runtime)
```

Le Domain ne connait pas Factur-X. Le Handler appelle `IFacturXGenerator.Generate(invoice)` qui produit les bytes PDF/A-3. Cela respecte l'isolation Clean Architecture.

### ADR-3 : Interface IEInvoicingGateway dans Contracts

```csharp
// Dans Vetolib.Billing.Contracts
public interface IEInvoicingGateway
{
    Task<Result<EInvoiceSubmissionResult>> SubmitInvoice(EInvoicePayload payload);
    Task<Result<EInvoiceStatus>> GetStatus(string platformInvoiceId);
    Task<Result> SubmitEReporting(EReportingPayload payload);
}
```

Permet de brancher n'importe quelle PDP sans modifier le Domain ni les Handlers.

### ADR-4 : Feature flag par clinique

La facturation electronique ne concerne que les cliniques francaises. Les cliniques UAE et Pologne ne doivent pas etre affectees. Un feature flag `EInvoicingEnabled` dans la configuration clinique controle l'activation.

### ADR-5 : Pas de modification de Shared/

Toute l'implementation reste dans le module Billing. Aucune modification de `Shared/Kernel` ou `Shared/Infrastructure` n'est necessaire. Le `CountryCode` et les configurations associees vivent dans le module Billing ou dans la configuration clinique (module Auth).

---

## 8. Plan de taches

### Phase 1 : Fondations (a demarrer maintenant -- livraison juin 2026)

| Tache | Description | Priorite |
|---|---|---|
| `todo-back-billing-multi-tax-001` | Rendre le taux de TVA configurable (multi-taux par facture, par pays) | Critique |
| `todo-back-billing-multi-currency-002` | Extraire la devise de la config clinique (AED, EUR, PLN) | Critique |
| `todo-back-billing-invoice-fields-003` | Ajouter les champs SIREN, TVA intracom, type operation, mentions legales | Critique |
| `todo-back-billing-country-policy-004` | Implementer `ICountryBillingPolicy` (UAE, France, Pologne) | Importante |

### Phase 2 : Factur-X (livraison juillet 2026)

| Tache | Description | Priorite |
|---|---|---|
| `todo-back-billing-facturx-gen-005` | Generer des factures Factur-X (PDF/A-3 + XML CII EN16931) | Critique |
| `todo-back-billing-facturx-validate-006` | Validation du XML CII contre le schema Factur-X | Importante |

### Phase 3 : Transmission PDP (livraison aout 2026)

| Tache | Description | Priorite |
|---|---|---|
| `todo-back-billing-einvoicing-gateway-007` | Interface `IEInvoicingGateway` + implementation PPF/Chorus Pro | Critique |
| `todo-back-billing-einvoicing-status-008` | Reception et gestion des statuts PDP (webhook/polling) | Critique |
| `todo-back-billing-ereporting-009` | E-reporting B2C (agregation + transmission periodique) | Critique |
| `todo-back-billing-payment-reporting-010` | E-reporting de paiement (TVA a l'encaissement) | Importante |

### Phase 4 : Frontend + integration (livraison aout 2026)

| Tache | Description | Priorite |
|---|---|---|
| `todo-front-billing-einvoicing-011` | UI pour les statuts e-invoicing (deposee, acceptee, rejetee) | Importante |
| `todo-front-billing-settings-012` | Page settings : SIREN, TVA intracom, choix PDP | Importante |

### Phase 5 : Tests et conformite (livraison fin aout 2026)

| Tache | Description | Priorite |
|---|---|---|
| `todo-test-billing-facturx-validation-013` | TU : validation Factur-X XML, edge cases taux TVA multiples | Critique |
| `todo-test-billing-ereporting-014` | TF : scenarios BDD e-reporting B2C | Critique |

---

## 9. Risques et questions ouvertes

### Risques

| Risque | Impact | Mitigation |
|---|---|---|
| API PPF pas finalisee avant septembre 2026 | Impossible de tester en conditions reelles | Developper contre l'API Chorus Pro existante + mock. Le PPF sera compatible |
| Report du calendrier (deja arrive 3 fois) | Effort premature | Les fondations (multi-taux, Factur-X) sont utiles independamment de la reforme |
| Complexite des taux de TVA veterinaires en France | Mauvais calculs, redressement fiscal | Valider avec un expert-comptable veterinaire les taux exacts par type d'acte |
| PDP partenaire pas encore choisie | Integration a refaire | L'interface `IEInvoicingGateway` abstrait ce choix |

### Questions ouvertes (a traiter avec le PO)

1. **Choix de la PDP partenaire** : Chorus Pro (gratuit, basique) ou PDP privee (Pennylane, Cegid) ? Impact sur le modele economique de Vetolib (commission ? white-label ?)

2. **Taux de TVA veterinaires en France** : les actes veterinaires sont-ils a 20% ou a 10% (taux reduit pour services a la personne) ? Les medicaments veterinaires sont a 10% ou 20% ? Necessaire de valider avec un expert-comptable.

3. **Archivage legal** : Vetolib assure l'archivage 10 ans ou on delegue a la PDP ? Implications de stockage et de cout.

4. **Impact sur le modele de prix Vetolib** : la facturation electronique est-elle un service inclus ou un add-on payant pour les cliniques francaises ?

5. **Pologne (3eme marche)** : la Pologne a son propre systeme de e-invoicing (KSeF -- Krajowy System e-Faktur). Le meme pattern `IEInvoicingGateway` pourra-t-il couvrir les deux ? A investiguer.

---

## 10. Sources et references

- **Loi de finances 2024, article 91** -- cadre legal de la reforme
- **Ordonnance n 2023-834 du 21 septembre 2023** -- calendrier revise
- **Decret n 2024-1090 du 1er novembre 2024** -- modalites d'application
- **FNFE-MPE** (Forum National de la Facture Electronique) -- specifications Factur-X
- **Factur-X.org** -- specifications techniques du format hybride PDF/A-3 + CII
- **API Chorus Pro** -- documentation AIFE (chorus-pro.gouv.fr)
- **Norme EN16931** -- norme europeenne de facturation electronique
- **ZUGFeRD 2.x** -- equivalent allemand de Factur-X (meme specification technique)
- **DGFiP** -- Direction Generale des Finances Publiques, specifications e-reporting
- **AIFE** -- Agence pour l'Informatique Financiere de l'Etat (gestionnaire Chorus Pro)

---

## Synthese executive

| Question | Reponse |
|---|---|
| Les veterinaires francais sont-ils concernes ? | **OUI** -- toutes les entreprises assujetties TVA |
| Quand doivent-ils etre prets ? | **Sept 2026** (reception) / **Sept 2027** (emission + e-reporting) |
| Quel format ? | **Factur-X EN16931** (recommande) |
| PPF ou PDP ? | Vetolib = **OD**, passe par une **PDP** (ou PPF en fallback) |
| Impact sur le module Billing ? | **MAJEUR** -- multi-taux TVA, Factur-X, transmission PDP, e-reporting |
| Vetolib doit-il etre pret pour sept 2026 ? | **OUI pour la reception** (les fournisseurs des cliniques enverront du Factur-X) |
| Timeline dev recommandee | **Juin-aout 2026** -- 3 mois de dev, avant le 1er sept 2026 |
| Peut-on reporter ? | Les fondations (multi-taux, multi-devise) sont utiles pour les 3 marches, pas que la France |
