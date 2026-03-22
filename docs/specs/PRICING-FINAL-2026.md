# PRICING FINAL -- Vetolib (2026)

> **Ce document est LA reference unique.** Toute grille de prix dans un autre document est obsolete.
> Valide par le PO le 2026-03-22. Positionnement : **Smart Challenger** (premium accessible, pas budget).

---

## 1. Modele : Per-Vet, 3 tiers, pas de Free tier

**Per-vet/mois** avec paliers de fonctionnalites. Pas de tarif par clinique, pas de metering usage.

- **Pas de Free tier permanent.** Un essai gratuit de 30 jours (voir section 5).
- Les non-vets (receptionists, assistants) ne comptent pas dans la tarification. Seuls les veterinaires actifs sont factures.
- Le prix inclut un nombre illimite de staff non-vet.

---

## 2. LA GRILLE DEFINITIVE

| Plan | AED/vet/mois | EUR/vet/mois | ~USD/vet/mois | Cible |
|---|---|---|---|---|
| **Starter** | **299** | **69** | ~$81 | Solo / petite clinique (1-3 vets) |
| **Pro** | **449** | **99** | ~$122 | Clinique en croissance (3-8 vets) |
| **Enterprise** | **649** | **149** | ~$177 | Multi-site / hopitaux (8+ vets) |

### Remises

| Scenario | Remise | Conditions |
|---|---|---|
| Prepaiement annuel | -15% | Paiement 12 mois d'avance |
| Early adopter | -25% pendant 12 mois | Les 10 premieres cliniques uniquement |
| Multi-site (3+ sites) | -10% | Enterprise tier uniquement |
| Parrainage | 1 mois gratuit pour le parrain | Quand la clinique referee convertit en paye |

Pas de remise permanente. Pas de tarif negocie en dessous du Starter.

---

## 3. Features par plan -- Liste exacte

### 3.1 STARTER (299 AED / 69 EUR)

**Inclus :**
- Agenda avec vue jour/semaine/mois
- Gestion des conflits de rendez-vous
- Dossiers medicaux (notes SOAP, historique, vaccinations)
- Facturation de base + suivi des paiements
- TVA 5% UAE / TVA multi-taux France
- Portail client (prise de RDV en ligne, historique visites)
- Notifications email (confirmations, rappels)
- Module patients (fiche animal, proprietaire, historique)
- Support standard (email, reponse sous 24h ouvrables)
- Max **5 vets** par clinique
- Staff non-vet illimite (assistants, receptionistes)
- Interface bilingue EN + AR (UAE) / EN + FR (France)
- Timezone Asia/Dubai par defaut, configurable

**Exclu :**
- ~~AI (triage, scheduling, no-show, SOAP scribe)~~
- ~~Stock / inventaire~~
- ~~WhatsApp integration~~
- ~~Reporting avance~~
- ~~Multi-site~~
- ~~API access~~
- ~~SLA garanti~~

### 3.2 PRO (449 AED / 99 EUR) -- **Plan recommande**

**Tout ce qui est dans Starter, PLUS :**
- AI scheduling optimization (suggestion de creneaux optimaux)
- AI triage assistant (analyse symptomes, score d'urgence)
- AI SOAP Notes / Scribe (transcription vocale vers notes structurees)
- No-show prediction (score interne, jamais visible par le proprietaire)
- Stock / inventaire (suivi lots, dates d'expiration, alertes seuil)
- WhatsApp integration (rappels RDV, suivis post-consultation)
- Rappels automatiques (vaccination, rappels, suivis)
- Reporting avance (CA par vet, par type d'acte, tendances)
- Devis electroniques avec approbation client digitale
- Multi-vet scheduling avec detection de conflits avancee
- Ramadan scheduling (horaires configurables)
- Semaine dimanche-jeudi (configurable)
- Max **15 vets** par clinique
- Support prioritaire (email + chat, reponse sous 4h ouvrables)

**Exclu :**
- ~~Multi-site (dashboard centralise)~~
- ~~API access~~
- ~~Roles custom~~
- ~~Account manager dedie~~
- ~~SLA garanti~~
- ~~Benchmarking industrie~~

### 3.3 ENTERPRISE (649 AED / 149 EUR)

**Tout ce qui est dans Pro, PLUS :**
- Multi-site management (dashboard centralise, reporting consolide)
- API REST access (integrations custom, systemes labo, imagerie)
- Benchmarking (KPIs clinique vs donnees anonymisees du secteur)
- Roles et permissions custom (au-dela des 4 roles standard)
- Account manager dedie
- SLA garanti 99.9% uptime
- Vets illimites
- Support premium (telephone + email + chat, reponse sous 1h ouvrables)
- Onboarding assiste (migration donnees, formation equipe)
- Lab integration (IDEXX VetConnect, quand disponible)
- Templates especes specialisees (falcon, chameau, equin -- UAE)

---

## 4. Feature Gates -- Ce qui pousse a l'upgrade

Les feature gates sont les fonctionnalites qui creent une friction deliberee pour pousser les cliniques a monter en tier.

### Starter --> Pro (upgrade principal vise)

| Feature gate | Pourquoi ca pousse | Plan ou ca arrive |
|---|---|---|
| **AI SOAP Scribe** | Les vets passent 1h+/jour sur la doc. Quand ils voient la demo, c'est le declencheur #1. | Pro |
| **AI Triage** | L'assistant de triage reduit les erreurs de priorite. Les cliniques occupees en ont besoin. | Pro |
| **Stock / Inventaire** | Toute clinique avec plus de 2 vets a besoin de suivi de stock. | Pro |
| **WhatsApp + Rappels auto** | Les no-shows coutent cher. Les rappels automatiques WhatsApp sont un game-changer aux UAE. | Pro |
| **Reporting avance** | Les owners veulent voir le CA par vet. Pas possible en Starter. | Pro |
| **Limite de 5 vets** | Une clinique qui grandit est bloquee a 5 vets en Starter. | Pro (jusqu'a 15) |

### Pro --> Enterprise

| Feature gate | Pourquoi ca pousse | Plan ou ca arrive |
|---|---|---|
| **Multi-site** | Des que le groupe ouvre un 2e site, il a besoin du dashboard centralise. | Enterprise |
| **API access** | Les grandes cliniques veulent connecter leur labo, leur compta, leur imagerie. | Enterprise |
| **Roles custom** | Les hopitaux ont des roles specifiques (chirurgien, interne, pharmacien). | Enterprise |
| **SLA 99.9%** | Les hopitaux ne tolerent pas de downtime. Le SLA garanti est obligatoire. | Enterprise |
| **Limite de 15 vets** | Au-dela de 15 vets, Enterprise est le seul plan. | Enterprise (illimite) |

---

## 5. Free Trial -- 30 jours, tier Pro

| Parametre | Valeur |
|---|---|
| **Duree** | 30 jours |
| **Tier** | Pro (experience complete, pas Starter) |
| **Carte de credit a l'inscription** | Non |
| **Carte de credit requise** | Jour 14 pour continuer au-dela |
| **Vets pendant le trial** | Jusqu'a 5 (suffisant pour evaluer) |
| **Donnees apres trial** | Conservees 30 jours apres expiration, puis supprimees |
| **Conversion par defaut** | Si pas de choix explicite --> Starter (pas de facturation surprise) |

### Sequence email pendant le trial

| Jour | Email |
|---|---|
| J1 | Welcome + guide de setup rapide (15 min) |
| J3 | "Avez-vous essaye l'AI Triage ?" (feature highlight) |
| J7 | Cas d'usage concret + temoignage clinique |
| J14 | **Prompt carte de credit** + etude de cas ROI |
| J21 | "Plus que 9 jours" + offre early adopter si eligible |
| J28 | "2 jours restants" + comparatif plans Starter vs Pro |
| J30 | "Trial expire" + lien pour choisir un plan |

### Pourquoi Pro pendant le trial (pas Starter)

- Montrer la meilleure experience possible (AI, WhatsApp, Stock)
- Creer l'habitude d'utiliser les features Pro
- La friction de downgrade (perdre l'AI scribe) pousse a convertir en Pro plutot qu'en Starter
- Industrie standard : 30% des trials convertissent mieux quand ils voient le tier premium

---

## 6. Justification concurrentielle

### 6.1 Positionnement vs concurrents UAE

| Concurrent | Prix approx. | Positionnement Vetolib |
|---|---|---|
| **ezyVet** (IDEXX) | $260-549/mo (~955-2015 AED) | **Starter a 30% en dessous de ezyVet entry.** Pro a parite, mais avec AI que ezyVet n'a pas. |
| **Vetspire** | $299-379/vet/mo (~1098-1392 AED) | **Nettement en dessous.** Vetspire est cher et US-only. |
| **Digitail** | ~$250-500/mo (~918-1837 AED) | **A parite sur Pro, mais avec AR/EN bilingue + WhatsApp que Digitail n'a pas.** |
| **VetPort** | $71/mo (~261 AED) | **Au-dessus (4x).** VetPort est budget/legacy. On ne compete pas sur le prix. |
| **vetPMS** (regional) | Non public (devis) | **Transparence tarifaire comme avantage.** vetPMS a une UI datee. |
| **Spreadsheets/papier** | Gratuit | **Le Starter est le cout d'entree pour se moderniser.** |

**Message cle UAE** : "Les features de ezyVet, les AI de Digitail, l'experience locale en plus -- a un prix plus juste."

### 6.2 Positionnement vs concurrents France

| Concurrent | Prix approx. | Positionnement Vetolib |
|---|---|---|
| **Vetup** | 19 EUR/mo | **Au-dessus (3.6x).** Vetup est low-cost avec features limitees. On ne compete pas. |
| **Epivet** | 45-90 EUR/mo + 25-50 EUR/vet | **A parite sur le cout total.** Epivet est local, pas cloud natif. |
| **Vetocom** | 52+ EUR/mo (devis) | **Legerement au-dessus, mais cloud natif + AI justifient l'ecart.** |
| **GmVet** | 64 EUR + 39 EUR support + 25 EUR/vet | **Moins cher au total pour 3+ vets (pas de frais support separee).** |
| **dr.veto** | Non public (devis) | **Transparence tarifaire comme avantage.** |
| **AssistoVet** | Non public (devis) | **Alternative cloud aux solutions lourdes on-premise des CHV.** |

**Message cle France** : "Cloud natif, AI integree, prix transparent -- sans etre lie a une centrale d'achat."

### 6.3 Pourquoi ce prix est juste

1. **Starter a 299 AED/69 EUR** : Point d'entree accessible pour remplacer le papier/Excel. Assez bas pour que le prix ne soit pas un obstacle, assez haut pour signaler un produit professionnel.

2. **Pro a 449 AED/99 EUR** : C'est le plan ou la marge se fait. A parite avec ezyVet mais avec AI (SOAP scribe, triage, no-show) que ezyVet n'a pas. A prix egal, on gagne sur les features.

3. **Enterprise a 649 AED/149 EUR** : Pour les groupes multi-sites. Moins cher que Vetspire Enterprise ($249/vet). Le SLA et l'API justifient le premium.

---

## 7. Distribution de tiers attendue et ARPU

| Tier | % cliniques attendu | Vets moyens | Revenue/client/mois (AED) | Revenue/client/mois (EUR) |
|---|---|---|---|---|
| Starter | 40% | 2 | 598 AED | 138 EUR |
| Pro | 45% | 4 | 1,796 AED | 396 EUR |
| Enterprise | 15% | 8 | 5,192 AED | 1,192 EUR |

### ARPU blende

- **UAE** : (0.40 x 598) + (0.45 x 1796) + (0.15 x 5192) = 239 + 808 + 779 = **1,826 AED/client/mois** (~$497)
- **France** : (0.40 x 138) + (0.45 x 396) + (0.15 x 1192) = 55 + 178 + 179 = **412 EUR/client/mois** (~$447)

### Mix pondere (60% UAE / 40% France en Year 1)

- **~$477/client/mois** en moyenne

Ce chiffre remplace le "$49/client" errone du Business Plan initial.

---

## 8. Ce document annule et remplace

Les grilles de prix dans les documents suivants sont **obsoletes** et doivent pointer vers ce fichier :

- `docs/studies/PRICING-STRATEGY-2026.md` -- section 2 (grille coherente mais ce doc est desormais secondaire)
- `docs/studies/PO-REVIEW-BUSINESS-NIGHT-2026.md` -- section 2 (identifiait les contradictions, maintenant resolues)
- `docs/business-plan.md` -- toute reference a "$49/client/month" est fausse
- `docs/studies/MARKET-RESEARCH-UAE-2026.md` -- section 9.3 (recommandation pricing par tiers plats, remplacee par per-vet)
- `docs/studies/MARKET-RESEARCH-FRANCE-2026.md` -- toute reference pricing

---

*Document cree le 2026-03-22 par le PO. Source unique de verite pour le pricing Vetolib.*
