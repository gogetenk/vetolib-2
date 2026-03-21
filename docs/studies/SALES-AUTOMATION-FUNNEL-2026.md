# Sales Automation Funnel -- Vetolib (2026)

> Funnel de vente 100% automatise pour un solo founder.
> Marches : UAE (primaire) + France (secondaire).
> Objectif : minimiser le temps humain, maximiser les conversions.
> Prerequis : les docs ONBOARDING-EMAIL-SEQUENCE-2026.md, OUTREACH-TEMPLATES-2026.md et MARKETING-PLAN-Q2-2026.md existent deja.

---

## Table des matieres

1. [Funnel complet](#1-funnel-complet)
2. [Lead scoring](#2-lead-scoring)
3. [Outils recommandes](#3-outils-recommandes)
4. [Sequences automatisees](#4-sequences-automatisees)
5. [Metriques sales](#5-metriques-sales)
6. [Approche cabinets](#6-approche-cabinets)

---

## 1. Funnel complet

### Vue d'ensemble

```
VISIT ──> LEAD ──> TRIAL ──> PAID ──> ADVOCATE
 (site)   (signup)  (14j)    ($$)    (referral)

  │         │         │        │         │
  │         │         │        │         └── NPS > 8 → demande temoignage + referral
  │         │         │        └── Upgrade push J-3, J-1, J0 → welcome paid
  │         │         └── Onboarding 7 emails (deja crees) → activation 3 milestones
  │         └── Demo auto Cal.com → scoring → si score > 60 : call humain
  └── PostHog tracking → retargeting → popup exit-intent
```

### Etape par etape

#### VISIT → LEAD

| Element | Detail |
|---|---|
| **Trigger** | Visiteur arrive sur vetolib.com (SEO, PH, WhatsApp link, LinkedIn) |
| **Action auto** | PostHog track pageviews, scroll depth, CTA clicks |
| **Popup exit-intent** | "Get 3 months free -- join 60+ UAE clinics" → email capture |
| **CTA principal** | "Start free" → signup form (nom, email, clinic name, nb vets, pays) |
| **CTA secondaire** | "Book a demo" → Cal.com embed |
| **Outil** | PostHog (analytics + feature flags), Cal.com (demo booking) |
| **Humain requis** | NON -- tout automatique |

#### LEAD → TRIAL

| Element | Detail |
|---|---|
| **Trigger** | Signup complete (compte cree) |
| **Action auto** | 1. Email welcome (J+0) -- voir ONBOARDING-EMAIL-SEQUENCE-2026.md |
|  | 2. Sequence 7 emails onboarding sur 14 jours |
|  | 3. In-app checklist (3 milestones : patient, RDV, facture) |
|  | 4. Si demo demandee : booking auto Cal.com + rappel J-1 + rappel H-1 |
| **Scoring** | Lead score calcule automatiquement (voir section 2) |
| **Seuil humain** | Score > 60 ET nb vets >= 3 → notification Slack au founder pour call |
| **Outil** | Loops (emails), PostHog (tracking activation), Cal.com (demo) |
| **Humain requis** | Seulement pour les leads qualifies (score > 60) |

#### TRIAL → PAID

| Element | Detail |
|---|---|
| **Trigger J-5** | Email "Your trial ends in 5 days -- here's what you'll lose" |
| **Trigger J-3** | Email "3 days left -- upgrade now, keep everything" + comparatif Free vs Pro |
| **Trigger J-1** | Email "Tomorrow your trial ends" + offre 20% si upgrade dans les 24h |
| **Trigger J0** | Email "Trial ended" + downgrade auto vers Free. Pas de coupure brutale |
| **Trigger J+3** | Email "Missing your Pro features?" + offre 1 mois a -50% |
| **Trigger J+7** | Dernier email "Special offer: 3 months Pro for the price of 2" |
| **Si aucun upgrade** | Passage en Free tier, nurturing mensuel (newsletter, nouvelles features) |
| **Seuil humain** | Si lead score > 70 ET pas upgrade a J-1 → call humain proactif |
| **Outil** | Loops (emails), Stripe (billing), PostHog (tracking) |

#### PAID → ADVOCATE

| Element | Detail |
|---|---|
| **Trigger J+30** | NPS survey in-app (1 question : "0-10, recommanderiez-vous Vetolib ?") |
| **Si NPS 9-10** | Auto : email "Would you share a quick testimonial?" + lien Testimonial.to |
|  | Auto : email "Refer a clinic, get 1 month free" + lien referral unique |
| **Si NPS 7-8** | Auto : email "What would make us a 10?" + formulaire feedback |
| **Si NPS 0-6** | ALERTE Slack → call humain dans les 24h (risque churn) |
| **Trigger J+90** | Demande Google review (si NPS > 7) |
| **Trigger J+180** | Case study request (si clinique active + NPS > 8) |
| **Referral program** | Parrain : 1 mois gratuit. Filleul : 1 mois gratuit (au lieu de 14j trial) |
| **Outil** | PostHog (NPS survey), Loops (emails), custom referral tracking |

---

## 2. Lead scoring

### Grille de scoring (0-100)

#### Criteres positifs (demographiques) -- max 40 points

| Critere | Points | Justification |
|---|---|---|
| Pays = UAE | +10 | Marche cible primaire |
| Pays = France | +7 | Marche secondaire |
| Autre pays | +2 | Potentiel mais pas prioritaire |
| 1 vet | +3 | Petite clinique, valeur faible |
| 2-3 vets | +8 | Taille ideale pour Starter |
| 4-10 vets | +15 | Gros prospect, valeur elevee |
| 10+ vets / multi-site | +20 | Prospect premium, potentiel Pro/Enterprise |
| Techno actuelle = papier/Excel | +10 | Douleur maximale, conversion facile |
| Techno actuelle = logiciel concurrent | +5 | Switch possible mais plus difficile |
| Techno actuelle = "ne sait pas" | +3 | Probablement papier |

#### Criteres positifs (comportementaux) -- max 60 points

| Critere | Points | Justification |
|---|---|---|
| Page pricing visitee | +5 | Intent d'achat |
| Page pricing visitee 2+ fois | +10 | Intent serieux |
| Demo demandee via Cal.com | +20 | Intent maximal |
| Signup effectue | +10 | Engagement concret |
| 1er patient cree (milestone 1) | +5 | Activation |
| 1er RDV cree (milestone 2) | +5 | Activation avancee |
| 1ere facture creee (milestone 3) | +10 | Activation complete -- tres qualifie |
| 3+ logins dans la semaine | +5 | Usage regulier |
| Invite un collegue (team member) | +10 | Adoption equipe |
| Ouvre 4+ emails onboarding | +5 | Engage avec le contenu |
| Visite help center | +3 | Investit du temps |
| Repond a un email | +10 | Engagement direct |

#### Criteres negatifs (dequalification)

| Critere | Points | Justification |
|---|---|---|
| Email @gmail/@hotmail (pas de domaine clinique) | -5 | Possiblement pas decision-maker |
| Email @university.edu ou @student | -20 | Etudiant, pas un acheteur |
| Nom de clinique = "test" / "aaa" / vide | -15 | Spam ou curiosite |
| 0 login apres signup (7 jours) | -10 | Lead mort |
| Unsubscribe email | -15 | Desinteret confirme |
| Bounce email | -30 | Lead invalide |
| Pays hors UAE/France/Europe/MENA | -5 | Hors scope actuel |

### Seuils d'action

| Score | Action |
|---|---|
| **0-20** | Nurturing auto uniquement (newsletter mensuelle). Pas de contact humain. |
| **21-40** | Sequence onboarding standard. Suivi PostHog passif. |
| **41-60** | Onboarding + email personnalise auto (merge tag clinique). Suivi PostHog actif. |
| **61-80** | **ALERTE SLACK** → Founder contacte dans les 24h. WhatsApp (UAE) ou email perso (France). |
| **81-100** | **ALERTE SLACK URGENTE** → Call dans les 4h. Ce lead va signer ou partir chez un concurrent. |

### Implementation technique

```
PostHog event tracking → calcul score cote serveur (API PostHog + webhook)
  → si score franchit un seuil → webhook vers Slack
  → si score > 60 → ajouter tag "hot" dans CRM
  → si score < 0 → archiver le lead
```

Le scoring est recalcule a chaque evenement (pas en batch). Stocke dans le CRM comme propriete du contact.

---

## 3. Outils recommandes

### Stack recommandee pour solo founder (cout mensuel estime)

| Fonction | Outil | Plan | Cout/mois | Pourquoi celui-la |
|---|---|---|---|---|
| **CRM** | HubSpot Free | Free CRM | $0 | Pipeline deals, contact management, email tracking. Suffisant jusqu'a 1 000 contacts. Upgrade a Starter ($20/mois) quand necessaire. |
| **Email automation** | Loops | Starter | $49/mois | Concu pour SaaS, event-driven, beau design, API simple. Alternative : Resend ($20/mois) si besoin transactionnel pur. |
| **Chat live** | Crisp | Pro | $25/mois | Chat live + chatbot + knowledge base. Widget en arabe. WhatsApp Business integration. Moins cher qu'Intercom ($89+). Alternative gratuite : Tawk.to (mais UX datee). |
| **Demo booking** | Cal.com | Free | $0 | Open-source, self-hostable, integration Google Calendar + Zoom. Workflow auto : confirmation, rappel J-1, rappel H-1. |
| **Analytics** | PostHog | Free | $0 | Deja integre dans Vetolib. Event tracking, feature flags, surveys (NPS), session replay. Gratuit jusqu'a 1M events/mois. |
| **Billing** | Stripe | Pay-as-you-go | 2.9% + 30c | Deja prevu. Checkout, subscriptions, invoices, dunning auto. |
| **NPS/Surveys** | PostHog Surveys | Free | $0 | Integre, pas besoin d'un outil supplementaire. |
| **Testimonials** | Testimonial.to | Free | $0 | Widget video/texte, embed sur landing page. |
| **Referral tracking** | Custom (PostHog) | -- | $0 | Lien unique par clinique, tracking via PostHog. Pas besoin de Cello au debut. |
| **Notifications internes** | Slack (webhook) | Free | $0 | Alertes lead scoring, NPS < 6, nouveau signup. |
| **Spreadsheet pipeline** | Google Sheets | Free | $0 | Dashboard metriques (voir section 5). Connecte via Zapier ou n8n. |
| **Automation glue** | n8n (self-hosted) | Free | $0 | Open-source, self-hostable sur le meme VPS. Connecte PostHog → Loops → HubSpot → Slack. Alternative : Zapier ($20/mois) si pas envie de self-host. |

**Cout total : ~$74/mois** (Loops $49 + Crisp $25). Tout le reste est gratuit.

### Alternatives evaluees et rejetees

| Outil | Raison du rejet |
|---|---|
| **Attio** (CRM) | $0 pour 3 users mais ecosysteme plus jeune, moins d'integrations que HubSpot |
| **Folk** (CRM) | Bon pour le networking, mauvais pour un pipeline sales structure |
| **Customer.io** | Plus puissant que Loops mais plus cher ($100+/mois), overkill au debut |
| **Intercom** | Excellent mais $89/mois minimum, trop cher pour le volume actuel |
| **Mailchimp** | Pas concu pour du SaaS event-driven, interface lourde |
| **Brevo** (ex-Sendinblue) | Option viable mais moins elegant que Loops pour du SaaS |

---

## 4. Sequences automatisees

### 4.1 Signup → Onboarding (EXISTE DEJA)

Voir `ONBOARDING-EMAIL-SEQUENCE-2026.md`. Resume :

```
J+0  : Welcome + quickstart link
J+1  : Setup checklist (skip si complete)
J+3  : "Add your first patient" (skip si fait)
J+5  : "Book your first appointment" (skip si fait)
J+7  : Mid-trial check-in + feature highlight
J+10 : "Have you tried AI triage?" (Pro feature teaser)
J+12 : "2 days left" upgrade push
```

### 4.2 Demo request → Booking → Follow-up

```
TRIGGER : Visiteur clique "Book a demo" ou score > 60

T+0min  : [AUTO] Page Cal.com ouverte (creneaux dispo : dim-jeu 9h-11h Dubai / lun-ven 9h-11h Paris)
T+0min  : [AUTO] Confirmation email avec lien Zoom + "What to prepare" (liste patients, questions)
T-24h   : [AUTO] Rappel email "Your demo is tomorrow at {{time}}"
T-1h    : [AUTO] Rappel WhatsApp (UAE) ou email (France) "Starting in 1 hour"
T+0     : [HUMAIN] Demo live 15 min — ecran partage, setup en direct avec les donnees de la clinique
T+1h    : [AUTO] Email "Thanks for the demo" + recap personnalise + lien signup si pas encore fait
T+3j    : [AUTO] Email "Any questions after the demo?" + offre trial etendu 30j (au lieu de 14)
T+7j    : [AUTO] Si pas de signup → email "Last chance: 30-day extended trial expires Friday"
T+14j   : [AUTO] Si toujours rien → archiver. Ajout a la newsletter mensuelle.
```

**Workflow n8n :**
```
Cal.com booking webhook
  → Creer contact HubSpot (deal = "Demo booked", stage = "Demo scheduled")
  → Envoyer confirmation via Loops
  → Programmer rappels (Loops scheduled send)
  → Apres demo : founder met a jour HubSpot manuellement (1 clic : "Demo done")
  → Declencher sequence post-demo via Loops
```

### 4.3 Trial ending → Upgrade push → Extension

```
TRIGGER : Trial expire dans 5 jours (calcule depuis la date de signup)

J-5  : [AUTO] Email "5 days left on your Pro trial"
       Contenu : tableau comparatif Free vs Starter vs Pro
       CTA : "Upgrade now — keep everything"

J-3  : [AUTO] Email "3 days left — here's what changes"
       Contenu : liste concrete de ce que la clinique perd (WhatsApp booking, AI triage, etc.)
       Dynamique : si milestone 3 atteint → "You've already sent {{invoice_count}} invoices with Pro"
       CTA : "Stay on Pro — AED 289/month"

J-1  : [AUTO] Email "Tomorrow is the last day"
       Contenu : offre 20% de reduction sur le 1er mois si upgrade dans les 24h
       CTA urgente : "Lock in 20% off → {{checkout_link_with_coupon}}"

       [CONDITIONNEL] Si lead score > 70 ET pas encore upgrade :
       → Notification Slack → founder envoie un WhatsApp/email personnel

J0   : [AUTO] Downgrade automatique vers Free (Stripe webhook)
       Email "Your trial has ended — you're now on Free"
       Ton : pas punitif. "You can upgrade anytime to get Pro features back."

J+3  : [AUTO] Email "Missing WhatsApp booking?"
       Feature spotlight sur la feature la plus utilisee pendant le trial
       Offre : 1 mois a -50% (coupon unique, expire dans 48h)

J+7  : [AUTO] Dernier push : "3 months Pro for the price of 2"
       Bundle deal. CTA finale.

J+14 : [AUTO] Stop les emails push. Passage en nurturing mensuel (newsletter).
```

### 4.4 Churn risk → Win-back campaign

```
TRIGGER : Indicateurs de churn detectes par PostHog

Indicateurs de churn (n'importe lequel declenche l'alerte) :
  - 0 login depuis 7 jours (alors que l'usage etait quotidien)
  - 0 RDV cree depuis 14 jours (alors que la moyenne etait 5+/semaine)
  - Feature usage en baisse de 50%+ sur 2 semaines
  - NPS < 6

ALERTE CHURN :

J+0  : [AUTO] Notification Slack "Churn risk: {{clinic_name}} inactive since {{last_login}}"
       [AUTO] Email "We miss you at Vetolib — is everything OK?"
       Ton : empathique, pas commercial. "Did you hit a roadblock? Reply and I'll help."

J+3  : [AUTO] Email "3 tips to get more value from Vetolib"
       Contenu : 3 features que la clinique n'utilise PAS encore (basees sur PostHog)
       Ex : "You haven't tried stock management yet — here's a 2-min setup guide"

J+7  : [HUMAIN] Si lead score > 50 → WhatsApp perso du founder
       "Hi {{name}}, I noticed {{clinic_name}} hasn't been active lately.
        Is there something I can help with? Happy to jump on a quick call."

J+14 : [AUTO] Si toujours inactif → "Special offer: downgrade to Starter at 50% for 3 months"
       (mieux que perdre le client completement)

J+30 : [AUTO] Si toujours inactif → email final "We're keeping your data safe"
       Confirmer que les donnees ne seront pas supprimees.
       CTA : "Reactivate anytime"
       Archiver le lead. Stop toute communication sauf newsletter trimestrielle.
```

### 4.5 Paid → NPS → Testimonial → Referral

```
TRIGGER : Client en plan payant depuis 30 jours

J+30  : [AUTO] NPS survey in-app (PostHog Surveys)
         1 question : "How likely are you to recommend Vetolib? (0-10)"
         + champ libre optionnel

         ROUTING selon score :
         ┌─────────────────────────────────────────────┐
         │ NPS 9-10 (Promoter)                         │
         │  → J+31 : Email "Would you leave a review?" │
         │    Lien Google Business + Trustpilot         │
         │  → J+33 : Email "Refer a clinic, get 1      │
         │    month free" + lien referral unique        │
         │  → J+60 : Email "Would you share your       │
         │    story?" + lien Testimonial.to             │
         │  → J+90 : Demande case study (si > 100      │
         │    patients actifs)                          │
         └─────────────────────────────────────────────┘
         ┌─────────────────────────────────────────────┐
         │ NPS 7-8 (Passive)                           │
         │  → J+31 : Email "What would make us a 10?"  │
         │    Formulaire court (3 questions max)        │
         │  → Analyser les reponses. Si feature        │
         │    request → ajouter au backlog + notifier   │
         │    quand c'est ship ("You asked, we built")  │
         └─────────────────────────────────────────────┘
         ┌─────────────────────────────────────────────┐
         │ NPS 0-6 (Detractor)                         │
         │  → IMMEDIATE : Slack alert #churn-risk      │
         │  → J+0 : [HUMAIN] Call/WhatsApp dans les 4h │
         │    "Hi {{name}}, I saw your feedback.        │
         │    I'd love to understand what's not working │
         │    and fix it personally."                   │
         │  → Creer ticket prioritaire dans le backlog  │
         │  → J+7 : Follow-up "We fixed {{issue}},     │
         │    would you try again?"                     │
         └─────────────────────────────────────────────┘

J+90  : [AUTO] 2eme NPS survey (pour mesurer l'evolution)
J+180 : [AUTO] 3eme NPS survey
         Si NPS monte (ex: 6→8) → celebrer : "You rated us 8 this time! Thank you."
         Si NPS baisse → declenchement churn risk (section 4.4)
```

### 4.6 Referral loop (post-advocate)

```
TRIGGER : Client NPS 9-10 OU client depuis 90+ jours avec usage actif

J+0  : [AUTO] Email "Know a clinic that needs better software?"
       Lien referral unique : vetolib.com/r/{{clinic_slug}}
       Mecanisme : parrain → 1 mois gratuit, filleul → 1 mois gratuit (au lieu de 14j)

J+14 : [AUTO] Si lien pas clique → rappel in-app (banner discret dans le dashboard)
       "Share Vetolib with a colleague — you both get 1 month free"

J+30 : [AUTO] Si 1+ referral converti → email de remerciement
       "{{referred_clinic}} just joined thanks to you! Your free month is active."
       + badge "Founding Advocate" dans le profil

BONUS : Si 3+ referrals convertis → upgrade gratuit au plan superieur pendant 3 mois
```

---

## 5. Metriques sales

### KPIs principaux

| Metrique | Definition | Cible M1 | Cible M3 | Cible M6 |
|---|---|---|---|---|
| **Visitors** | Visiteurs uniques/mois sur vetolib.com | 500 | 2 000 | 5 000 |
| **Signup rate** | Signups / Visitors | 5% | 7% | 8% |
| **Activation rate** | Milestone 1 atteint / Signups | 40% | 55% | 65% |
| **Trial-to-Paid** | Upgrade / Trial starts | 8% | 12% | 18% |
| **Monthly churn** | Clients perdus / Clients totaux | < 5% | < 4% | < 3% |
| **NPS** | Score moyen NPS | > 30 | > 40 | > 50 |
| **Time-to-close** | Jours entre signup et premier paiement | 20j | 16j | 14j |
| **Pipeline velocity** | Deals * Win rate * Avg deal / Sales cycle | Mesurer | Optimiser | $2k+/mois |
| **CAC** | Cout d'acquisition client (ads + temps humain) | < $50 | < $40 | < $30 |
| **LTV** | Revenue moyen par client sur 24 mois | $600 | $700 | $800 |
| **LTV/CAC ratio** | Doit etre > 3 | > 3 | > 5 | > 8 |

### Conversion funnel detaille

```
                 Visitors (5 000/mois cible M6)
                        │
                   5-8% │ signup
                        ▼
                   Signups (400/mois)
                        │
                  55-65% │ activation (milestone 1)
                        ▼
                 Activated (260/mois)
                        │
                  15-18% │ upgrade
                        ▼
                   Paid (47/mois)
                        │
                   3-5%  │ churn mensuel
                        ▼
                   Net Growth: +44/mois
```

### Dashboard Google Sheets

Creer un Google Sheet avec 4 onglets :

**Onglet 1 : Pipeline weekly**

| Semaine | New Leads | Demos booked | Demos done | Trials started | Upgrades | Revenue |
|---|---|---|---|---|---|---|
| W1 Apr | | | | | | |
| W2 Apr | | | | | | |

**Onglet 2 : Cohort analysis**

| Cohorte signup | Total signups | Activated J+7 | Paid J+14 | Paid J+30 | Still paid M3 | Still paid M6 |
|---|---|---|---|---|---|---|
| Apr W1 | | | | | | |

**Onglet 3 : Lead scoring distribution**

| Score range | Nb leads | Conversion rate | Avg time-to-close |
|---|---|---|---|
| 0-20 | | | |
| 21-40 | | | |
| 41-60 | | | |
| 61-80 | | | |
| 81-100 | | | |

**Onglet 4 : Channel attribution**

| Canal | Visitors | Signups | Paid | CAC | Revenue |
|---|---|---|---|---|---|
| Organic SEO | | | | | |
| Product Hunt | | | | | |
| WhatsApp outreach | | | | | |
| LinkedIn | | | | | |
| Referral | | | | | |
| Google Ads | | | | | |

**Alimentation** : n8n webhook depuis PostHog → Google Sheets API (automatique, pas de saisie manuelle).

---

## 6. Approche cabinets

### UAE : WhatsApp first

#### Pourquoi WhatsApp

- 95%+ de la population UAE utilise WhatsApp
- Les gerants de cliniques repondent plus vite sur WhatsApp que par email
- C'est coherent avec le produit (on vend du WhatsApp booking)
- Le taux de reponse WhatsApp Business est de 40-60% vs 5-15% pour le cold email

#### Quand contacter

| Jour | Qualite | Raison |
|---|---|---|
| **Dimanche** | EXCELLENT | Debut de semaine UAE, gerant frais, planning de la semaine |
| **Lundi** | BON | 2eme jour, encore receptif |
| **Mardi** | BON | Milieu de semaine, charge de travail moderee |
| **Mercredi** | MOYEN | Milieu de semaine, commence a etre charge |
| **Jeudi** | MAUVAIS | Dernier jour avant weekend, personne ne prend de decisions |
| **Vendredi** | INTERDIT | Weekend UAE (jour de priere) |
| **Samedi** | MAUVAIS | Weekend UAE |

| Heure | Qualite | Raison |
|---|---|---|
| **7h30-8h30** | EXCELLENT | Gerant arrive avant les consultations, check son tel |
| **8h30-10h** | BON | Debut des consultations, mais peut repondre entre 2 |
| **10h-13h** | MAUVAIS | Pic de consultations |
| **13h-14h** | MOYEN | Pause dejeuner, mais fatigue |
| **14h-17h** | MAUVAIS | 2eme pic consultations |
| **17h-18h** | MOYEN | Fin de journee, peut lire un message |
| **Apres 18h** | INTERDIT | Vie perso, ne pas deranger |

**Creneau optimal : Dimanche ou Lundi, 7h30-8h30 heure Dubai (UTC+4)**

#### Qui contacter

| Role | Contacter ? | Pourquoi |
|---|---|---|
| **Gerant / Owner** | OUI -- cible #1 | C'est lui qui decide du budget et des outils |
| **Practice Manager** | OUI -- cible #2 | Gere les operations au quotidien, influenceur cle |
| **Receptionniste** | NON | Pas le pouvoir de decision. Peut bloquer ton message |
| **Veterinaire en consultation** | JAMAIS | Il est avec un patient. Tu seras bloque immediatement |

Comment identifier le gerant :
1. LinkedIn : chercher "Owner" ou "Founder" + nom de la clinique
2. Google Maps : le numero affiche est souvent celui du gerant dans les petites cliniques
3. Site web : page "About us" ou "Our Team" -- le premier nom est souvent le gerant
4. DM Instagram de la clinique : "Hi, who handles your clinic software?" → ils donnent le contact

#### Ice breaker qui marche

**NE PAS commencer par :**
- "I have a software for you" → blocage immediat
- "Are you looking for a new clinic management system?" → "No" et fin de conversation
- "We're cheaper than IDEXX" → comparaison que personne n'a demandee

**CE QUI OUVRE LA PORTE :**

1. **Le probleme du telephone qui sonne** (taux de reponse le plus eleve)
   ```
   "Hi {{name}}, quick question — how many calls does your front desk
   handle daily just for appointment booking? I ask because we built
   something that automates 80% of those calls via WhatsApp."
   ```
   Pourquoi ca marche : tout gerant de clinique DETESTE les appels telephoniques
   repetitifs. C'est un pain point universel.

2. **Le compliment + question** (marche bien en culture arabe/UAE)
   ```
   "Hi {{name}}, I came across {{clinic_name}} — I loved the work you're
   doing with exotic animals in Dubai. Quick question: do your clients
   book appointments via WhatsApp already, or mostly by phone?"
   ```
   Pourquoi ca marche : compliment sincere + question ouverte. Le gerant
   repond parce que c'est flatteur et facile a repondre.

3. **Le peer pressure** (quand tu as deja des clients)
   ```
   "Hi {{name}}, 3 clinics in Dubai are now using Vetolib for WhatsApp
   appointment booking. {{clinic_name}} came up as one that could benefit
   most — would a 15-min demo be worth your time this week?"
   ```
   Pourquoi ca marche : social proof local. Si des confreres l'utilisent,
   le gerant veut savoir pourquoi.

4. **Le contenu de valeur gratuit** (pour les leads froids)
   ```
   "Hi {{name}}, we just published a guide on how UAE vet clinics are
   reducing no-shows by 40% with automated WhatsApp reminders. Would you
   like me to send it? No sales pitch, just the data."
   ```
   Pourquoi ca marche : tu donnes avant de demander. Le guide contient
   naturellement des mentions de Vetolib.

#### Sequence WhatsApp UAE (copier-coller)

```
J0 (Dim 7h30) : Ice breaker #1 ou #2 (selon le contexte)
                 → Si reponse : engager la conversation, proposer demo
                 → Si pas de reponse : attendre J+3

J+3 (Mer 7h30) : Follow-up court
                  "Hi {{name}}, just following up — did you get a chance
                  to think about the WhatsApp booking question?
                  Happy to send a 90-sec video demo if easier."
                  → Si reponse : engager
                  → Si pas de reponse : attendre J+7

J+7 (Dim 7h30) : Dernier message
                  "Hi {{name}}, last message from me — I don't want to
                  be that annoying software person! If you ever want to
                  see how WhatsApp appointment booking works for vet
                  clinics, here's a link: {{demo_video_link}}.
                  Have a great week!"
                  → Si pas de reponse : STOP. Archiver. Re-contacter dans 3 mois.
```

**Regle absolue : jamais plus de 3 messages WhatsApp sans reponse. Au-dela = spam.**

---

### France : Email first

#### Pourquoi email (pas WhatsApp)

- En France, un WhatsApp Business non sollicite d'un inconnu = suspect/spam
- Les veterinaires francais gerent leur pro par email
- Les cabinets ont souvent une adresse email generique (cabinet@cliniquevet.fr)
- LinkedIn fonctionne bien pour les decision-makers en France

#### Quand contacter

| Jour | Qualite | Raison |
|---|---|---|
| **Lundi** | MAUVAIS | Retour de weekend, urgences accumulees, pas receptif |
| **Mardi** | EXCELLENT | Installe dans la semaine, receptif aux nouveautes |
| **Mercredi** | BON | Milieu de semaine, encore de l'energie |
| **Jeudi** | BON | Avant-dernier jour, peut planifier un call pour la semaine suivante |
| **Vendredi** | MOYEN | Fin de semaine, mais certains gerants lisent leurs emails le vendredi aprem |
| **Samedi/Dimanche** | INTERDIT | Ne pas envoyer (meme si programme) -- mauvais signal |

| Heure | Qualite | Raison |
|---|---|---|
| **7h-8h** | BON | Le gerant lit ses emails avec son cafe, avant les consultations |
| **8h-12h** | MAUVAIS | Consultations |
| **12h-14h** | MOYEN | Pause dejeuner |
| **14h-18h** | MAUVAIS | Consultations |
| **18h-19h** | BON | Fin de journee, le gerant traite son admin |
| **Apres 19h** | INTERDIT | Vie perso |

**Creneau optimal : Mardi ou Mercredi, 7h-8h heure Paris (UTC+1/+2)**

#### Qui contacter en France

| Type de structure | Qui contacter | Comment le trouver |
|---|---|---|
| **Cabinet independant (1-3 vets)** | Le veterinaire associe/titulaire | Annuaire Ordre des Veterinaires, LinkedIn |
| **Clinique (4-10 vets)** | Le gerant ou directeur de clinique | LinkedIn "Directeur clinique veterinaire" |
| **Groupe (IVC, Mon Veto, Argos)** | Directeur operations ou DSI | LinkedIn, site corporate du groupe |
| **Groupement (ClubVet, VetFamily)** | Direction du groupement | Site web du groupement, congres veto |

#### Ice breaker France

1. **La facture electronique** (TOPIQUE en 2026 -- obligation septembre 2026)
   ```
   Objet : Facturation electronique en septembre — votre logiciel est-il pret ?

   Bonjour {{name}},
   Avec l'obligation de reception des factures electroniques en septembre 2026,
   beaucoup de cabinets veterinaires se retrouvent a devoir changer de logiciel.

   Vetolib integre nativement la facturation electronique (Factur-X + Chorus Pro)
   et coute 3x moins cher que Vetocom. Seriez-vous ouvert a une demo de 15 minutes
   pour voir si ca pourrait vous simplifier la transition ?
   ```

2. **Le pain point recrutement**
   ```
   Objet : 840 diplomes/an pour 22 000 postes — comment faire plus avec moins

   Bonjour {{name}},
   Avec la penurie de veterinaires, chaque minute administrative en moins est une
   minute de soin en plus. Vetolib automatise la prise de RDV, les rappels, et la
   facturation — vos veterinaires ne touchent plus a l'admin.

   15 minutes pour vous montrer ? Je m'adapte a votre planning.
   ```

#### Sequence email France (copier-coller)

Voir `OUTREACH-TEMPLATES-2026.md` pour les templates detailles. Resume de la cadence :

```
J0 (Mar 7h)  : Cold email #1 (ice breaker facturation electronique ou recrutement)
J+3 (Ven 7h) : Follow-up #2 court ("Avez-vous eu le temps de voir mon email ?")
J+7 (Mar 7h) : Follow-up #3 avec contenu de valeur (guide PDF, etude de cas)
J+14 (Mar 7h): Dernier email ("Je ne vous embete plus — voici un lien si un jour
                vous cherchez une alternative a {{concurrent_actuel}}")
J+14         : STOP. Archiver. Re-contacter dans 6 mois OU quand une feature
               pertinente sort (ex: integration labo).
```

**Regle absolue : jamais plus de 4 emails sans reponse. Au-dela = mauvaise reputation domaine.**

---

## Annexe : Workflow n8n complet

```
┌──────────────────────────────────────────────────────────────────┐
│                        n8n WORKFLOWS                             │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. NEW SIGNUP                                                   │
│     PostHog "signup" event                                       │
│       → Create HubSpot contact (name, email, clinic, country)    │
│       → Create HubSpot deal (stage: "Trial started")             │
│       → Trigger Loops onboarding sequence                        │
│       → Post Slack #new-signups "New signup: {{clinic_name}}"    │
│       → Calculate initial lead score (demographic only)          │
│       → If score > 40: add tag "warm" in HubSpot                │
│                                                                  │
│  2. MILESTONE REACHED                                            │
│     PostHog "milestone_1/2/3" event                              │
│       → Update lead score in HubSpot                             │
│       → If score crosses 60: Slack #hot-leads alert              │
│       → Update HubSpot deal stage                                │
│                                                                  │
│  3. DEMO BOOKED                                                  │
│     Cal.com webhook                                              │
│       → Update HubSpot deal (stage: "Demo scheduled")            │
│       → Trigger Loops demo confirmation email                    │
│       → Schedule reminders (J-1, H-1)                            │
│       → Slack #demos "Demo booked: {{clinic_name}} at {{time}}"  │
│                                                                  │
│  4. TRIAL ENDING                                                 │
│     Daily cron (check trial_end_date - 5 days)                   │
│       → Trigger Loops trial-ending sequence (J-5, J-3, J-1, J0) │
│       → If lead score > 70 at J-1: Slack #urgent-upgrades        │
│                                                                  │
│  5. PAYMENT EVENT                                                │
│     Stripe webhook (invoice.paid / subscription.created)         │
│       → Update HubSpot deal (stage: "Won")                       │
│       → Slack #revenue "New paying customer: {{clinic_name}}"    │
│       → Schedule NPS survey trigger at J+30                      │
│       → Cancel any remaining trial-ending emails                 │
│                                                                  │
│  6. CHURN DETECTION                                              │
│     Daily cron (check last_login > 7 days for active users)      │
│       → Trigger Loops win-back sequence                          │
│       → Slack #churn-risk alert                                  │
│       → Update HubSpot deal (stage: "At risk")                   │
│                                                                  │
│  7. NPS RESPONSE                                                 │
│     PostHog survey webhook                                       │
│       → Route based on score (promoter/passive/detractor)        │
│       → Trigger appropriate Loops sequence                       │
│       → If detractor: Slack #urgent-churn alert                  │
│       → Store NPS history in HubSpot contact properties          │
│                                                                  │
│  8. METRICS SYNC (weekly)                                        │
│     Sunday 20:00 cron                                            │
│       → Pull PostHog data (signups, activations, usage)          │
│       → Pull Stripe data (MRR, churn, upgrades)                  │
│       → Pull HubSpot data (pipeline, deals)                      │
│       → Write to Google Sheets dashboard                         │
│       → Post Slack #weekly-metrics summary                       │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

## Annexe : Checklist de lancement

### Semaine 1 : Setup outils

- [ ] Creer compte HubSpot Free → configurer pipeline (Trial → Demo → Negotiation → Won → Lost)
- [ ] Creer compte Loops → importer templates email depuis ONBOARDING-EMAIL-SEQUENCE-2026.md
- [ ] Creer compte Crisp → installer widget sur vetolib.com (EN + AR)
- [ ] Configurer Cal.com → creneaux demo (dim-jeu 9-11h Dubai, mar-jeu 9-11h Paris)
- [ ] Configurer PostHog → events custom : signup, milestone_1/2/3, pricing_page_view
- [ ] Configurer PostHog Surveys → NPS survey template
- [ ] Creer workspace Slack → channels : #new-signups, #hot-leads, #demos, #revenue, #churn-risk, #weekly-metrics
- [ ] Installer n8n (Docker sur meme VPS) → creer les 8 workflows ci-dessus

### Semaine 2 : Contenu

- [ ] Creer la page Testimonial.to
- [ ] Creer le lien referral template (vetolib.com/r/{{slug}})
- [ ] Configurer les sequences Loops (onboarding, trial-ending, win-back, NPS routing)
- [ ] Preparer le guide PDF "How UAE vet clinics reduce no-shows by 40%"
- [ ] Configurer le popup exit-intent sur vetolib.com (PostHog feature flag)

### Semaine 3 : Premier outreach

- [ ] Importer les 61 leads UAE dans HubSpot (depuis UAE-VET-CLINIC-LEADS-2026.md)
- [ ] Envoyer les premiers messages WhatsApp (batch de 10, dim 7h30 Dubai)
- [ ] Envoyer les premiers cold emails France (batch de 10, mar 7h Paris)
- [ ] Monitorer les reponses et ajuster les templates

### Ongoing : Rituels hebdomadaires (30 min/semaine)

- [ ] Lundi 8h : Review dashboard Google Sheets (5 min)
- [ ] Lundi 8h : Repondre aux leads chauds dans HubSpot (10 min)
- [ ] Mercredi 8h : Check Slack #churn-risk, appeler si necessaire (10 min)
- [ ] Vendredi 18h : Review des metriques de la semaine, ajuster les sequences (5 min)
