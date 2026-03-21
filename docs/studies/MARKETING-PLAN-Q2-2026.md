# Vetolib Marketing Plan Q2 2026 (April - June)

> Plan de lancement et growth hacking pour les 3 premiers mois post-MVP.
> Marche primaire : UAE (Dubai, Abu Dhabi). Secondaires : France, Pologne.
> Date de creation : 2026-03-21

---

## Contexte marche

### UAE - Chiffres cles

- **1,5 million** de proprietaires d'animaux aux UAE, **2+ millions d'animaux**
- Marche veterinaire UAE : **$793M projetes en 2030** (CAGR 8,8%)
- Marche des hopitaux veterinaires : **$204,8M en 2024**, $303,7M projetes en 2030
- Depense mensuelle par animal : **AED 500 a 2 000** (grooming, nourriture, soins)
- WhatsApp utilise par **95%+ de la population UAE**
- Croissance adoption chiens : **la plus rapide**, surtout chez les expats et jeunes pros
- Services pet care : CAGR **13-17%**, un des plus rapides du lifestyle UAE

### Concurrence directe (Practice Management Software)

| Concurrent | Forces | Faiblesses vs Vetolib |
|---|---|---|
| IDEXX Neo | Integration labo, marque connue | Pas de WhatsApp, pas d'arabe, cher |
| ezyVet | Cloud moderne, bon UX | Zero presence Moyen-Orient, anglais only |
| Provet Cloud | Multi-taille clinique | Pas d'AI triage, pas de WhatsApp |
| Digitail | UX moderne, telemedicine | Pas localise UAE/arabe |
| VetPort | Prix bas | Interface datee, pas de WhatsApp |

### Avantage competitif Vetolib

1. **Premier logiciel veto avec WhatsApp integre aux UAE** -- le canal #1 de communication
2. **Interface en arabe natif** -- aucun concurrent ne le fait
3. **AI triage pour urgences** -- differenciation technologique forte
4. **Pricing agressif** -- Free tier + $29 starter vs $200+/mois chez IDEXX/ezyVet
5. **Design moderne** -- shadcn/ui, pas une app des annees 2010

---

## Phase 1 : Pre-launch & Lancement (Mois 1 - Avril 2026)

### Semaine 1-2 : Pre-launch

#### Landing page + Waiting list

- **Outil** : Page dediee sur vetolib.com avec Waitlist (utiliser Loops.so ou Tally.so)
- **Hook principal** : "The first veterinary clinic software built for the UAE. WhatsApp-native. Arabic-ready. AI-powered."
- **CTA** : "Join the beta -- 3 months free for early adopters"
- **Social proof** : Compteur de cliniques inscrites (meme si c'est 5 au debut)
- **Contenu landing** :
  - Video demo 90 secondes (screen recording commente, pas besoin de production lourde)
  - 3 features highlights : WhatsApp, AI Triage, Arabic UI
  - Pricing transparent avec le Free tier mis en avant
  - Logos "Built with" (Next.js, .NET, PostgreSQL) pour la credibilite tech

#### Beta privee (10 cliniques)

- **Objectif** : 5-10 cliniques beta a Dubai avant le lancement public
- **Comment les trouver** :
  1. Google Maps scraping (voir Section 4) -- extraire les 70+ cliniques de Dubai
  2. Message LinkedIn direct aux veterinaires/managers identifies
  3. Visite physique dans 5 cliniques a Dubai (rien ne bat le face-to-face dans la culture UAE)
- **Offre beta** : 6 mois Pro gratuit + onboarding personnalise + badge "Founding Clinic"
- **Engagement demande** : 2 feedbacks structures par mois + droit d'utiliser leur nom/logo

#### Contenu a preparer

| Type | Sujet | Canal | Statut |
|---|---|---|---|
| Video demo | Walkthrough complet 3 min | YouTube, LinkedIn, Landing | A produire |
| Video courte | "Book a vet appointment via WhatsApp in 30 sec" | Instagram Reels, TikTok | A produire |
| Blog post | "Why UAE vet clinics still run on paper in 2026" | Blog, LinkedIn | A rediger |
| Blog post | "AI triage: how it works for veterinary emergencies" | Blog, Medium | A rediger |
| Infographie | "UAE Pet Market 2026 in Numbers" | LinkedIn, Instagram | A designer |
| Case study | Beta clinic #1 -- avant/apres | Blog, LinkedIn | Apres 2 sem de beta |

### Semaine 3 : Product Hunt Launch

#### Preparation (J-7)

- [ ] Creer la page "Upcoming" sur Product Hunt 7 jours avant
- [ ] Demander aux beta testers + contacts de follow la page
- [ ] Preparer le copy PH : tagline + description + 5 images/GIFs + video
- [ ] Identifier 3-5 "Hunters" potentiels (chercher des hunters qui ont lance des SaaS verticaux)
- [ ] Preparer les reponses aux commentaires types (pricing, vs competitors, roadmap)

#### Tagline PH

> "Vetolib -- The first WhatsApp-native veterinary clinic software for the Middle East. AI triage, Arabic UI, modern design."

#### Launch Day (mardi ou mercredi, 00:01 PST)

- [ ] Poster a 00:01 PST (08:01 heure Dubai) -- le mardi ou mercredi sont les meilleurs jours
- [ ] Activer le reseau : email aux beta testers, post LinkedIn, DMs persos
- [ ] Repondre a CHAQUE commentaire PH dans les 3 premieres heures (critique pour l'algorithme)
- [ ] Poster un "Maker comment" detaille : histoire perso, pourquoi le marche UAE, stack tech
- [ ] Cross-poster sur X/Twitter, LinkedIn, r/veterinary, r/SaaS, Hacker News (Show HN)

#### Objectifs PH

- Top 5 du jour (realiste pour un SaaS vertical bien prepare)
- 200+ upvotes
- 50+ signups depuis PH
- 3-5 articles/mentions presse apres le buzz PH

### Semaine 4 : Outreach direct

#### Cold outreach structure

**Canal principal : WhatsApp Business** (ironique et efficace -- on vend WhatsApp, on prospecte via WhatsApp)

Template message (EN) :
```
Hi [Name], I'm [Founder] from Vetolib. We just launched the first
veterinary clinic software built specifically for the UAE market --
with WhatsApp appointment booking, Arabic interface, and AI triage.

We're offering 3 months free to the first 20 clinics in Dubai.
Would you be open to a 15-min demo this week?

[Link to demo video]
```

Template message (AR) :
```
مرحبا [الاسم]، أنا [المؤسس] من فيتوليب. أطلقنا أول برنامج إدارة
عيادات بيطرية مصمم خصيصا للإمارات -- بحجز مواعيد عبر واتساب، واجهة
عربية، وفرز ذكي بالذكاء الاصطناعي.

نقدم 3 أشهر مجانا لأول 20 عيادة في دبي. هل تحب نعرض لك ديمو 15 دقيقة هالأسبوع؟

[رابط الفيديو التوضيحي]
```

**Canal secondaire : Email** (extrait via Google Maps scraping)

**Canal tertiaire : LinkedIn** (cibler les profils "Veterinary Practice Manager", "Vet Clinic Owner" a Dubai)

#### Partenariats strategiques

| Partenaire | Approche | Valeur |
|---|---|---|
| Dubai Municipality Veterinary Section | Contact officiel via dm.gov.ae | Legitimite institutionnelle |
| UAE Veterinary Association | Sponsoring newsletter/event | Acces au reseau veto |
| Ecoles veto UAE (Abu Dhabi University) | Licence educative gratuite | Pipeline futurs vetos |
| Pet influencers Dubai (Instagram/TikTok) | Collaboration contenu | Visibilite B2C qui impacte B2B |
| Fournisseurs equipements veto UAE | Co-marketing | Leads croises |

---

## Phase 2 : Growth Hacking (Mois 2-3 -- Mai/Juin 2026)

### Referral Program : "Refer a Clinic"

**Mecanique** :
- Clinique existante invite une autre clinique
- **Parrain** : 1 mois gratuit sur son plan actuel
- **Filleul** : 1 mois gratuit (au lieu du trial standard de 14 jours)
- **Bonus** : si le filleul passe en paid dans les 60 jours, le parrain recoit un 2eme mois gratuit

**Implementation** :
- Lien de referral unique par clinique (trackable via PostHog)
- Widget in-app "Invite a clinic" dans le dashboard
- Email automatique de rappel a J+7 si le lien n'a pas ete partage
- Leaderboard des cliniques qui referent le plus (gamification legere)

**Outil** : Cello (https://cello.so) ou custom via PostHog -- eviter les usines a gaz

### Free Tier comme funnel d'acquisition

Le plan Free n'est PAS un produit d'appel au rabais. C'est un outil d'acquisition :

| Free | Starter ($29) | Pro ($79) |
|---|---|---|
| 1 vet, 50 patients | 3 vets, illimite | Illimite |
| Agenda basique | + WhatsApp booking | + AI triage |
| Dossiers patients | + Rappels SMS/email | + Analytics avancees |
| Pas de WhatsApp | + Stock basique | + Multi-site |

**Pourquoi ca marche** :
- Une clinique demarre en Free, se rend compte qu'elle a besoin de WhatsApp booking (la feature killer aux UAE) --> upgrade Starter
- Apres 2-3 mois, elle veut l'AI triage et les analytics --> upgrade Pro
- Le Free cree un **switching cost** : une fois les patients importes, on ne repart pas

**Trigger d'upgrade in-app** :
- "You've reached 50 patients. Upgrade to Starter for unlimited patients." (notification douce)
- "15 pet owners tried to book via WhatsApp this week. Enable WhatsApp booking with Starter." (FOMO data-driven)

### SEO -- Content Strategy

#### Mots-cles prioritaires (volume recherche UAE + faible competition)

| Mot-cle | Intent | Priorite | Contenu |
|---|---|---|---|
| veterinary software UAE | Transactionnel | P0 | Landing page |
| vet clinic management Dubai | Transactionnel | P0 | Landing page |
| veterinary practice management software | Transactionnel | P0 | Comparatif |
| WhatsApp booking veterinary | Transactionnel | P1 | Feature page |
| AI triage veterinary | Informationnel | P1 | Blog + feature page |
| pet clinic software Arabic | Transactionnel | P1 | Landing page AR |
| best vet software 2026 | Informationnel | P1 | Blog comparatif |
| how to manage veterinary clinic | Informationnel | P2 | Blog serie |
| veterinary appointment scheduling | Transactionnel | P2 | Feature page |
| emergency vet Dubai | Informationnel (indirect) | P2 | Blog (attire les pet owners, visibilite indirecte) |

#### Content calendar SEO (1 article/semaine)

- Semaine 1 : "Best Veterinary Practice Management Software 2026: Complete Comparison"
- Semaine 2 : "How WhatsApp is Transforming Veterinary Care in the UAE"
- Semaine 3 : "AI Triage for Veterinary Emergencies: What Clinic Owners Need to Know"
- Semaine 4 : "Running a Vet Clinic in Dubai: The Complete 2026 Guide"
- Semaine 5 : "Paper vs Digital: The Real Cost of Not Using Clinic Software"
- Semaine 6 : "Veterinary Clinic Software in Arabic: Why It Matters"
- Semaine 7 : "How to Reduce No-Shows at Your Vet Clinic (WhatsApp Reminders)"
- Semaine 8 : "The UAE Pet Market in 2026: Trends Every Vet Should Know"

**Format** : 1 500-2 000 mots, optimise SEO, CTA vers signup en fin d'article.

### Angles marketing differenciants

#### Angle 1 : "Le premier logiciel veto avec WhatsApp aux UAE"

- **Pourquoi ca tue** : 95% des UAE utilisent WhatsApp. Les pet owners veulent booker par WhatsApp, pas par telephone. Aucun concurrent ne le fait.
- **Contenu** : Video "Watch a pet owner book a vet appointment via WhatsApp in 30 seconds"
- **Canal** : LinkedIn (decision-makers), Instagram Reels (pet owners qui font remonter la demande)

#### Angle 2 : "AI triage pour urgences veterinaires"

- **Pourquoi ca tue** : Un pet owner panique a 23h. L'AI triage dit "Emergency -- go to clinic now" ou "Monitor -- book appointment tomorrow". Ca sauve des vies et desengorgent les urgences.
- **Contenu** : Blog + video demo + temoignage beta tester
- **Canal** : LinkedIn (credibilite pro), YouTube (recherche informationelle)

#### Angle 3 : "La premiere interface veto en arabe"

- **Pourquoi ca tue** : Les staff emiratis et les clients arabophones n'ont jamais eu de logiciel veto dans leur langue.
- **Contenu** : Screenshot comparatif EN/AR, temoignage d'un vet emiratis
- **Canal** : LinkedIn UAE, Instagram, presse locale (Khaleej Times, Gulf News)

### Cold Outreach a l'echelle

#### Trouver les cliniques (lead generation)

1. **Google Maps Scraping** -- outil recommande : [Outscraper](https://outscraper.com/) ou [Apify Google Maps Scraper](https://apify.com/compass/crawler-google-places)
   - Recherche : "veterinary clinic" + "Dubai", "Abu Dhabi", "Sharjah", "Al Ain", "Ajman"
   - Donnees extraites : nom, adresse, telephone, site web, horaires, avis Google
   - Enrichissement email : Outscraper crawl les sites web pour trouver les emails
   - **Resultat attendu** : 70-120 cliniques a Dubai, 30-50 a Abu Dhabi, 20-30 dans les autres emirats

2. **Annuaires en ligne**
   - [EasyUAE.com](https://www.easyuae.com/en/dubai/veterinary-clinics/directory.html) -- top 20 cliniques Dubai
   - [Yello.ae](https://www.yello.ae/category/veterinary-clinics) -- annuaire complet UAE
   - [Dubiki.com](https://www.dubiki.com/en/abu-dhabi/hospitals-and-clinics/veterinary-clinics/category.html) -- Abu Dhabi

3. **LinkedIn Sales Navigator**
   - Filtres : "Veterinary" + "United Arab Emirates" + "Owner/Manager/Director"
   - Resultat attendu : 50-100 profils pertinents
   - Outreach : InMail personnalise + connexion

#### Sequence outreach (5 touches sur 3 semaines)

| Jour | Canal | Message |
|---|---|---|
| J0 | WhatsApp | Message initial (voir template ci-dessus) |
| J+3 | Email | "Did you see my WhatsApp? Here's a 90-sec demo video" |
| J+7 | LinkedIn | Connexion + message court |
| J+14 | WhatsApp | Suivi : "We just onboarded [Clinic X] in Dubai. Curious?" |
| J+21 | Email | Dernier message : "Last chance for the 3-month free offer" |

### Webinaires / Demos live

**Format** : "Vetolib Live Demo -- 30 min" (bi-mensuel)

- **Plateforme** : Zoom ou Google Meet (simple, pas besoin de plus)
- **Audience** : Veterinaires et managers de cliniques UAE
- **Structure** :
  - 5 min : probleme (les outils actuels sont nuls)
  - 15 min : demo live du produit (WhatsApp booking, AI triage, interface arabe)
  - 10 min : Q&A
- **CTA** : "Sign up for free today -- we'll help you migrate"
- **Promotion** : LinkedIn event + email aux leads + WhatsApp broadcast
- **Recording** : Publie sur YouTube apres chaque session

---

## Phase 3 : Social Media Plan (12 semaines)

### Strategie par plateforme

#### LinkedIn (canal #1 -- B2B, decision-makers)

- **Ton** : Professionnel mais pas corporate. Thought leadership.
- **Frequence** : 3-4 posts/semaine
- **Types de contenu** :
  - Thought leadership : "Why the UAE vet industry needs better software"
  - Chiffres marche : infographies sur le pet market UAE
  - Product updates : features, screenshots, videos
  - Case studies : temoignages beta cliniques
  - Behind-the-scenes : stack tech, decisions produit
- **Format optimal** : Texte + image ou carrousel. Les videos natives LinkedIn performent bien aussi.
- **Hashtags** : #VeterinarySoftware #UAE #PetCare #VetTech #Dubai #AnimalHealth

#### Instagram (canal #2 -- B2C indirect, brand awareness)

- **Ton** : Chaleureux, visuel, emotionnel (les animaux, ca marche TOUJOURS)
- **Frequence** : 3-4 posts/semaine + stories quotidiennes
- **Types de contenu** :
  - Reels : demos produit en 30 sec, "day in the life of a UAE vet"
  - Carrousels : tips veterinaires, infographies marche
  - Stories : polls ("How do you book your vet?"), behind-the-scenes, Q&A
  - UGC : repost des cliniques beta qui utilisent Vetolib
- **Pourquoi B2C** : les pet owners qui voient Vetolib demandent a leur clinique "Why don't you use this?" -- pression bottom-up

#### X / Twitter (canal #3 -- tech community, early adopters)

- **Ton** : Direct, technique, transparent. Build in public.
- **Frequence** : 5-7 tweets/semaine (dont threads)
- **Types de contenu** :
  - Build in public : "Week 12: here's what we shipped"
  - Tech takes : "Why we chose .NET Aspire over microservices"
  - Engagement : repondre aux discussions #VetTech, #SaaS, #Startup
  - Threads : deep dives sur le marche UAE, les decisions produit
  - Product Hunt launch day : live-tweeting

#### TikTok (canal #4 -- experimental, haut potentiel viral)

- **Pertinence** : OUI. Le contenu veterinaire est extremement viral sur TikTok. Les videos de soins animaux, de cliniques, de "cute pet moments" font des millions de vues.
- **Ton** : Fun, educatif, emotionnel
- **Frequence** : 2-3 videos/semaine
- **Types de contenu** :
  - "POV: You're a vet in Dubai and you finally get modern software"
  - "Watch a pet owner book an appointment via WhatsApp" (screen recording)
  - "3 things every vet clinic in Dubai needs in 2026"
  - Collaborations avec des pet influencers UAE
- **Objectif** : Brand awareness, pas conversion directe. Le ROI est indirect mais reel.

#### YouTube (canal #5 -- SEO long terme, credibilite)

- **Ton** : Professionnel, educatif
- **Frequence** : 1 video/semaine
- **Types de contenu** :
  - Demo produit complete (5-10 min)
  - Tutoriels : "How to set up WhatsApp booking for your vet clinic"
  - Webinaires enregistres
  - Interviews de veterinaires UAE
- **SEO** : Titres optimises pour les mots-cles (voir section SEO)

### Calendrier de publication -- 12 semaines

#### Semaine 1 (Avril W1) -- Pre-launch teasing

| Jour | Plateforme | Contenu |
|---|---|---|
| Lun | LinkedIn | "We've been building something for UAE vet clinics. Stay tuned." + screenshot floutte |
| Mar | Instagram | Reel teaser : montage rapide de l'interface avec musique |
| Mer | X | Thread : "Why we're building veterinary software for the UAE market (a thread)" |
| Jeu | Instagram Stories | Poll : "How does your vet clinic manage appointments?" |
| Ven | LinkedIn | Infographie : "UAE Pet Market 2026 in Numbers" |

#### Semaine 2 (Avril W2) -- Beta stories

| Jour | Plateforme | Contenu |
|---|---|---|
| Lun | LinkedIn | "We just onboarded our first beta clinic in Dubai. Here's what they said." |
| Mar | Instagram | Carrousel : "5 reasons your vet clinic needs WhatsApp booking" |
| Mer | X | Tweet : "Day 1 of our beta. 3 appointments booked via WhatsApp. The vet literally said 'finally'." |
| Jeu | TikTok | "POV: first day using modern vet software at a Dubai clinic" |
| Ven | LinkedIn | Blog share : "Why UAE vet clinics still run on paper in 2026" |

#### Semaine 3 (Avril W3) -- Product Hunt launch week

| Jour | Plateforme | Contenu |
|---|---|---|
| Lun | Toutes | "We launch on Product Hunt this week. Follow our page [link]" |
| Mar/Mer | Toutes | LAUNCH DAY -- cross-platform activation |
| Jeu | LinkedIn | "We hit Top [X] on Product Hunt. Here's what happened." |
| Ven | Instagram | Reel : recap de la launch day, screenshots des commentaires |

#### Semaine 4 (Avril W4) -- Post-launch momentum

| Jour | Plateforme | Contenu |
|---|---|---|
| Lun | LinkedIn | "Week 1 post-launch: [X] clinics signed up. Lessons learned." |
| Mar | YouTube | Video demo complete (5 min walkthrough) |
| Mer | X | Thread : "10 things I learned launching a SaaS in the UAE market" |
| Jeu | Instagram | Carrousel : "Meet the team behind Vetolib" |
| Ven | TikTok | "Showing my vet the AI triage feature for the first time" |

#### Semaines 5-8 (Mai) -- Growth + contenu educatif

- **LinkedIn** : 3x/sem -- case studies, product updates, thought leadership marche UAE
- **Instagram** : 3x/sem -- reels demos, tips veto, behind-the-scenes
- **X** : 5x/sem -- build in public, engagement community, product updates
- **TikTok** : 2x/sem -- contenus viraux (animaux + tech), collabs influencers
- **YouTube** : 1x/sem -- tutoriels, webinaire enregistre, interview vet

**Themes Mai** :
- Semaine 5 : "WhatsApp booking deep dive" (toutes plateformes)
- Semaine 6 : "AI triage explained" (focus LinkedIn + YouTube)
- Semaine 7 : "Arabic interface showcase" (focus Instagram + TikTok)
- Semaine 8 : "Customer story #1" (case study video + blog)

#### Semaines 9-12 (Juin) -- Scale + referral push

- Meme cadence que Mai, avec en plus :
- **Semaine 9** : Lancement officiel du referral program (toutes plateformes)
- **Semaine 10** : "Vetolib by the numbers" -- metriques publiques (transparence)
- **Semaine 11** : Annonce de la presence au VET ME 2026 (si budget le permet)
- **Semaine 12** : Bilan Q2 public -- build in public style

---

## 4. Leads qualifies -- Comment trouver les cliniques UAE

### Sources de leads

| Source | Methode | Volume estime | Cout |
|---|---|---|---|
| Google Maps Scraping | Outscraper / Apify -- "veterinary clinic" par ville | 150-200 cliniques | $20-50 |
| EasyUAE.com | Scraping manuel de l'annuaire | 40-60 cliniques | Gratuit |
| Yello.ae | Annuaire businesses UAE | 100+ cliniques | Gratuit |
| LinkedIn Sales Navigator | Filtre "Veterinary" + "UAE" | 50-100 profils | $80/mois |
| Dubai Municipality | Liste officielle des cliniques licensees | Exhaustif | Demande officielle |
| Google search | "veterinary clinic [ville] site:instagram.com" | 30-50 comptes | Gratuit |

### Associations et organismes

| Organisation | Contact | Utilite |
|---|---|---|
| Dubai Municipality Veterinary Services | vetsection@dm.gov.ae / 800900 | Listes officielles, legitimite |
| Emirates Veterinary Association | Via LinkedIn / conferences | Reseau de vetos |
| Abu Dhabi Agriculture and Food Safety Authority (ADAFSA) | Site officiel | Reglementation, listing |

### Conferences et events 2026

| Event | Date | Lieu | Action |
|---|---|---|---|
| Int'l Conference on Veterinary Medicine | 20-21 Mars 2026 | Abu Dhabi | PASSE -- suivre les participants sur LinkedIn |
| Int'l Summit on Veterinary Medicine | 2-3 Avril 2026 | Abu Dhabi | Participer, networker, distribuer flyers |
| Int'l Symposium on Vet Medicine & Biotech | 3-4 Avril 2026 | Dubai | Participer, presenter si slot dispo |
| **VET ME 2026** | **8-10 Sept 2026** | **Dubai World Trade Centre** | **Event majeur** -- reserver un stand (budget $3-5K), demo live |

**VET ME 2026 est l'event de l'annee.** C'est le plus grand salon veterinaire du Moyen-Orient. Y avoir un stand est non-negociable si le budget le permet. Sinon, au minimum y aller en visiteur et networker agressivement.

---

## 5. Metriques et KPIs

### Funnel AARRR

```
Awareness     --> Visit website
Acquisition   --> Sign up (free or trial)
Activation    --> Create first patient + book first appointment
Revenue       --> Upgrade to paid plan
Referral      --> Invite another clinic
```

### KPIs par phase

#### Mois 1 (Avril) -- Lancement

| Metrique | Objectif | Outil |
|---|---|---|
| Waitlist signups | 100 | Tally.so |
| Beta clinics onboarded | 5-10 | CRM interne |
| Product Hunt upvotes | 200+ | Product Hunt |
| Website visitors | 2 000 | PostHog |
| Social media followers (total) | 500 | LinkedIn + Instagram + X |
| Demo requests | 20 | Calendly / Typeform |

#### Mois 2 (Mai) -- Traction

| Metrique | Objectif | Outil |
|---|---|---|
| Free signups | 30 | PostHog |
| Paid conversions | 5 | Stripe |
| MRR | $300-500 | Stripe |
| Activation rate (created patient) | 60% | PostHog |
| Churn | < 5% | PostHog |
| Referral links shared | 10 | PostHog |
| Blog organic traffic | 500 visits | PostHog / Google Search Console |

#### Mois 3 (Juin) -- Scale

| Metrique | Objectif | Outil |
|---|---|---|
| Total signups (cumule) | 80+ | PostHog |
| Paid clinics | 15 | Stripe |
| MRR | $1 000-1 500 | Stripe |
| CAC (cout acquisition client) | < $100 | Calcul manuel |
| LTV estimee | > $500 | Projection 12 mois |
| LTV/CAC ratio | > 5x | Objectif |
| Net Promoter Score | > 40 | Enquete in-app |
| Organic search impressions | 5 000/mois | Google Search Console |

### Outils de mesure

| Outil | Usage | Cout |
|---|---|---|
| **PostHog** (deja integre) | Analytics produit, funnels, feature flags, session replays | Self-hosted gratuit |
| **Google Search Console** | SEO, impressions, CTR, positions | Gratuit |
| **Stripe Dashboard** | MRR, churn, revenue | Inclus |
| **Loops.so** ou **Resend** | Email marketing, sequences, newsletters | $25-50/mois |
| **Calendly** | Booking de demos | Gratuit (tier basique) |
| **Notion** ou **Linear** | CRM leger pour tracker les leads | Gratuit / deja utilise |

### Formules cles

```
CAC = (depenses marketing + sales) / nombre de clients payes acquis
LTV = ARPU mensuel x duree moyenne abonnement (mois)
Churn mensuel = clients perdus ce mois / clients au debut du mois
Activation rate = users qui ont cree 1 patient / total signups
Viral coefficient = invitations envoyees x taux de conversion des invites
```

---

## Budget estime Q2 2026

| Poste | Mensuel | Q2 Total | Notes |
|---|---|---|---|
| Google Maps scraping | $50 | $50 | One-shot |
| LinkedIn Sales Navigator | $80 | $240 | 3 mois |
| Email marketing (Loops/Resend) | $30 | $90 | |
| Design (Canva Pro) | $13 | $39 | Templates social media |
| Product Hunt (featured) | $0 | $0 | Gratuit, le travail est dans la prep |
| Contenu video (Founder DIY) | $0 | $0 | Screen recordings + Loom |
| Pub LinkedIn (optionnel) | $200 | $600 | Ciblage UAE veterinary |
| Event VET ME stand (Sept) | - | $3-5K | Hors Q2 mais a budgeter maintenant |
| **TOTAL Q2** | | **$1 000-1 020** | Hors event VET ME |

Le budget est volontairement minimal. A ce stade, le temps du fondateur est l'investissement principal. Les paid ads ne sont pas prioritaires tant que le product-market fit n'est pas confirme avec les 10-15 premieres cliniques payantes.

---

## Actions immediates (cette semaine)

- [ ] Scraper Google Maps pour les cliniques Dubai + Abu Dhabi (Outscraper, $20)
- [ ] Creer la landing page waitlist
- [ ] Enregistrer la video demo 90 secondes
- [ ] Poster le premier teaser LinkedIn
- [ ] Contacter 5 cliniques a Dubai pour la beta (WhatsApp + visite physique)
- [ ] Creer les comptes social media (LinkedIn company page, Instagram, X, TikTok)
- [ ] Preparer la page Product Hunt Upcoming

---

## Sources

- [Middle East Veterinary Hospital Market - $1.4B by 2033](https://www.globenewswire.com/news-release/2026/02/17/3239399/28124/en/Middle-East-Veterinary-Hospital-Trends-Analysis-Report-2025-A-1-4-Billion-Market-by-2033-from-750-Million-in-2024-with-Focus-on-UAE-Saudi-Arabia-Kuwait-Qatar-and-Oman.html)
- [UAE Veterinary Services Market Size & Outlook 2025-2033](https://www.grandviewresearch.com/horizon/outlook/veterinary-services-market/uae)
- [Dubai Pet Ownership Report 2026](https://petsinthecity.me/the-state-of-pet-ownership-in-dubai-2026-report/)
- [UAE Pet Industry Insights 2025](https://www.happypet.tech/blog/expert-advice/uae-pet-industry-insights-2025)
- [UAE Pet Ownership Grows 30%](https://www.zawya.com/en/press-release/events-and-conferences/uae-pet-ownership-grows-30-catalyzes-pet-industry-to-over-us300mln-market-knh9xbuu)
- [WhatsApp Business API in UAE](https://sleekflow.io/blog/whatsapp-business-api-uae)
- [VET ME 2026 Dubai](https://www.eventseye.com/fairs/f-veterinary-vet-me-18994-1.html)
- [Veterinary Clinics Directory Dubai](https://www.easyuae.com/en/dubai/veterinary-clinics/directory.html)
- [Veterinary Clinics Directory Abu Dhabi](https://www.easyuae.com/en/abu-dhabi/veterinary-clinics/directory.html)
- [Product Hunt Launch Strategy 2025](https://beyondlabs.io/blogs/how-to-get-your-first-100-saas-users-with-a-product-hunt-launch)
- [How to Launch on Product Hunt in 2026](https://hackmamba.io/developer-marketing/how-to-launch-on-product-hunt/)
- [SaaS Referral Programs 2025](https://refgrow.com/blog/saas-referral-programs)
- [B2B Growth Hacking Strategies 2026](https://www.leanlabs.com/blog/b2b-saas-growth-hacking-strategies)
- [TikTok for Veterinarians](https://yoyofumedia.com/tiktok-for-veterinarians/)
- [Google Maps Scraper - Outscraper](https://outscraper.com/google-maps-scraper/)
- [Apify Google Maps Email Extractor](https://apify.com/lukaskrivka/google-maps-with-contact-details)
- [Top Veterinary Software Solutions 2025 - IDEXX](https://software.idexx.com/top-veterinary-software-solutions-a-2025-comparison-guide)
- [Digitail - Veterinary Practice Management](https://digitail.com/)
- [ezyVet - Cloud Veterinary Software](https://www.ezyvet.com/)
- [Cello - SaaS Referral Platform](https://cello.so/best-referral-marketing-platform-2025/)
