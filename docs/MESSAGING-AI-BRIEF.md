# Brief fonctionnel -- Messagerie clinique avec triage IA

**Auteur** : Product Owner Vetolib
**Date** : 2026-03-09
**Statut** : Draft -- en attente validation stakeholders
**Module cible** : Messaging (nouveau module)

---

## 1. Probleme utilisateur

### Situation actuelle

Les cliniques veterinaires UAE n'ont aucun canal de communication structure avec les proprietaires d'animaux. Trois problemes majeurs :

**1.1 Communication non tracee**
Les proprietaires contactent la clinique par WhatsApp personnel des praticiens, telephone, ou en se presentant physiquement. Aucune de ces interactions n'est enregistree dans le dossier patient. Quand un praticien different prend le relais, il n'a pas le contexte des echanges precedents.

**1.2 Pas de priorisation**
Un message "mon chat ne mange plus depuis 3 jours" arrive au meme niveau qu'un "quels sont vos horaires vendredi ?". Le praticien ou la receptionniste doit lire chaque message pour determiner l'urgence, ce qui represente en moyenne 45 minutes par jour et par praticien (estimation basee sur des entretiens cliniques Dubai, Q4 2025).

**1.3 Pas de lien avec le dossier medical**
Meme quand un proprietaire envoie une photo post-operatoire ou decrit des symptomes par message, cette information est perdue. Le praticien doit manuellement la retranscrire dans le dossier -- ce qu'il ne fait presque jamais.

### Impact business

- Temps veterinaire gaspille sur du triage manuel (facturable perdu)
- Risque medical : message urgent noye dans les messages administratifs
- Experience proprietaire degradee : pas de reponse ou reponse tardive
- Cliniques qui perdent des clients au profit de concurrents plus reactifs

---

## 2. Vision produit

Un systeme de messagerie integre a Vetolib ou :

1. Le **proprietaire** envoie un message depuis un portail web (pas d'app native au MVP) en decrivant sa demande et en selectionnant l'animal concerne
2. L'**IA analyse** le message et attribue automatiquement une categorie de triage et un niveau de priorite
3. Le **bon interlocuteur** (vet, receptionniste, admin) recoit le message dans une inbox priorisee avec le contexte patient
4. L'IA propose des **suggestions de reponse** que le praticien peut valider, modifier, ou ignorer
5. Toute la conversation est **liee au dossier patient** et consultable depuis la fiche de l'animal

### Principes directeurs

- **Humain dans la boucle** : l'IA trie et suggere, mais ne repond jamais automatiquement aux proprietaires. Chaque message sortant est valide par un humain.
- **Zero friction proprietaire** : pas de compte a creer au MVP (lien magique par email), pas d'app a installer.
- **Multi-tenant** : chaque clinique a ses conversations isolees (clinicId). Un proprietaire qui frequente deux cliniques a deux fils de conversation separes.
- **Bilingue natif** : interface et messages en anglais et arabe, detection automatique de la langue du message entrant.

---

## 3. Parcours utilisateurs detailles

### 3.1 Parcours Owner (proprietaire d'animal)

**Entree dans le systeme**

1. Le proprietaire recoit un email de la clinique apres une consultation avec un lien "Contacter votre clinique"
2. Il clique sur le lien (token unique, valide 90 jours, renouvelable)
3. Il arrive sur le portail messagerie de la clinique (brande avec le nom/logo de la clinique)

**Envoi d'un message**

4. Ecran d'accueil : liste de ses conversations passees + bouton "Nouveau message"
5. Il clique "Nouveau message"
6. Il selectionne l'animal concerne (liste pre-remplie depuis ses patients enregistres)
7. Il selectionne une categorie approximative parmi :
   - "Mon animal a un probleme de sante"
   - "Je souhaite prendre ou modifier un rendez-vous"
   - "Question sur une facture ou un paiement"
   - "Suivi apres une operation ou un traitement"
   - "Autre"
8. Il redige son message en texte libre (max 2000 caracteres)
9. Il peut joindre jusqu'a 3 photos (max 5 Mo chacune, formats JPG/PNG)
10. Il envoie le message
11. Ecran de confirmation : "Votre message a ete envoye. Temps de reponse estime : X" (le delai affiche depend de la categorie detectee par l'IA)

**Reception de la reponse**

12. Le proprietaire recoit une notification par email : "La clinique X a repondu a votre message"
13. Il clique sur le lien dans l'email et revient sur le portail
14. Il voit la reponse du praticien et peut continuer la conversation dans le meme fil

**Contraintes**

- Le proprietaire ne voit jamais le triage IA ni la priorite assignee
- Le proprietaire ne peut pas envoyer de message a un praticien specifique -- la clinique gere le routing
- Si le proprietaire n'a pas d'animal enregistre, il ne peut utiliser que la categorie "Autre" (le message sera route vers la receptionniste)

### 3.2 Parcours Receptionist

**Inbox receptionniste**

1. La receptionniste se connecte a Vetolib comme d'habitude
2. Dans la barre laterale, un nouvel onglet "Messages" apparait avec un badge indiquant le nombre de messages non traites
3. Elle clique et voit son inbox filtree : uniquement les messages categorises "Demande RDV" et "Question administrative"
4. Les messages sont tries par priorite puis par anciennete (FIFO a priorite egale)

**Traitement d'un message**

5. Elle clique sur un message
6. Panneau droit : le message du proprietaire, avec en dessous le contexte patient (nom animal, espece, dernier RDV, factures en cours)
7. Sous le message, l'IA propose 1 a 3 suggestions de reponse courtes
8. La receptionniste peut :
   - Cliquer sur une suggestion pour la pre-remplir dans le champ de reponse, puis la modifier et envoyer
   - Ecrire une reponse libre
   - Utiliser un template de reponse rapide (liste configurable par la clinique)
   - **Transferer** le message a un vet si elle estime que le triage IA s'est trompe
   - **Convertir en RDV** : un bouton cree directement un rendez-vous dans l'agenda avec les infos pre-remplies (patient, owner, motif)
9. Apres envoi de la reponse, le message passe en "Traite" et sort de l'inbox

**Re-categorisation**

10. Si la receptionniste recoit un message medical par erreur de triage, elle clique "Transferer au veterinaire"
11. Le message apparait dans l'inbox du vet avec mention "Transfere par [Receptionniste]"

### 3.3 Parcours Vet (veterinaire)

**Inbox veterinaire**

1. Le vet se connecte et voit le badge Messages
2. Son inbox contient :
   - Les urgences medicales (priorite haute, fond rouge)
   - Les questions medicales non urgentes
   - Les suivis post-operatoires des patients dont il est le vet referent
   - Les messages transferes par la receptionniste
3. Les urgences sont toujours en haut, independamment de la date

**Traitement d'un message medical**

4. Il clique sur un message
5. Panneau droit : message du proprietaire + contexte medical complet (dernier examen, prescriptions en cours, allergies connues, historique vaccination)
6. Si la conversation est longue (plus de 5 messages), un resume automatique IA est affiche en haut : "Resume : proprietaire rapporte vomissements depuis 48h, chat male 3 ans, vaccins a jour, pas d'antecedents chirurgicaux"
7. L'IA propose des suggestions de reponse adaptees au contexte medical
8. Le vet peut :
   - Repondre (validation/modification suggestion ou texte libre)
   - **Ajouter une note interne** visible uniquement par l'equipe, pas par le proprietaire
   - **Creer un RDV urgent** directement depuis le message
   - **Ajouter au dossier medical** : un bouton rattache le contenu du message (texte + photos) comme note dans le dossier medical du patient
9. Apres reponse, le message passe en "Traite"

**Urgences**

10. En cas d'urgence medicale detectee par l'IA, le vet recoit une notification push (navigateur) en plus du badge
11. Si aucun vet ne consulte le message dans les 10 minutes, une escalade est declenchee : notification a tous les vets de la clinique

### 3.4 Parcours Admin

**Inbox admin**

1. L'admin voit tous les messages de la clinique (vue globale) + son inbox specifique (feedback/reclamations)
2. Vue globale avec filtres : par statut (non lu, en cours, traite), par categorie, par praticien assigne, par date
3. Dashboard statistiques : temps moyen de reponse, volume par categorie, taux de re-categorisation IA

**Gestion**

4. L'admin peut :
   - Reassigner un message a un autre praticien
   - Configurer les templates de reponses rapides
   - Configurer les horaires de messagerie (heures ou les messages "non urgents" ne declenchent pas de notification)
   - Consulter les performances IA (taux de triage correct)
   - Gerer le consentement messagerie (voir quels proprietaires ont accepte/refuse)

---

## 4. Categories de triage

### Matrice de triage

| Categorie | Priorite | Routing | SLA reponse | Notification |
|---|---|---|---|---|
| Urgence medicale | CRITIQUE | Vet (tous si pas de reponse 10min) | 15 min | Push immediate |
| Suivi post-operatoire | HAUTE | Vet referent du dossier | 2 heures | Push |
| Question medicale non urgente | NORMALE | Vet disponible (round-robin ou assigne) | 8 heures (jour ouvre) | Badge inbox |
| Demande de RDV | NORMALE | Receptionniste | 4 heures (jour ouvre) | Badge inbox |
| Question administrative | BASSE | Receptionniste | 24 heures (jour ouvre) | Badge inbox |
| Feedback / Reclamation | BASSE | Admin | 48 heures | Badge inbox |

### Signaux de detection par categorie

**Urgence medicale**
- Mots-cles : saignement, ne respire plus, convulsions, empoisonnement, accident, inconscient, ne bouge plus
- Mots-cles arabes equivalents
- Photos montrant du sang, blessure visible (analyse image future -- hors scope MVP, uniquement texte au MVP)
- Indicateur temporel : "depuis quelques minutes", "vient de"

**Suivi post-operatoire**
- Patient avec une chirurgie dans les 14 derniers jours
- Mots-cles : cicatrice, points de suture, convalescence, apres l'operation, medication post-op
- Message envoye par un proprietaire dont l'animal a un suivi actif

**Question medicale non urgente**
- Description de symptomes sans indicateur d'urgence immediate
- Mots-cles : depuis quelques jours, mange moins, gratte, tousse, changement de comportement
- Demande de conseil medical general

**Demande de RDV**
- Mots-cles : rendez-vous, disponibilite, prendre RDV, quand, prochain creneau
- Demande de modification ou annulation de RDV existant

**Question administrative**
- Mots-cles : facture, prix, tarif, horaires, adresse, parking, assurance, paiement
- Demande de document (certificat, attestation)

**Feedback / Reclamation**
- Mots-cles : satisfait, insatisfait, reclamation, complaint, merci, probleme avec
- Notation (si le proprietaire utilise un score)

### Gestion des cas ambigus

- Si le score de confiance IA est inferieur a 70%, le message est envoye a la receptionniste avec mention "Triage incertain -- merci de verifier la categorie"
- La receptionniste peut confirmer ou re-categoriser
- Les re-categorisations alimentent le modele pour amelioration continue

---

## 5. Fonctionnalites detaillees

### 5.1 Inbox priorisee par role

- Chaque role voit uniquement les messages qui le concernent (voir matrice de triage section 4)
- Tri par defaut : priorite descendante, puis anciennete ascendante
- Filtres disponibles : statut (non lu / en cours / traite), categorie, patient, date
- Recherche full-text dans les messages
- Badge non lu dans la barre laterale, mis a jour en temps reel
- Le RECEPTIONIST ne voit PAS les messages medicaux (coherent avec la regle d'acces Vetolib : pas de dossiers medicaux)
- L'ASSISTANT voit les messages medicaux en lecture seule (coherent avec lecture seule sur dossiers medicaux)

### 5.2 Suggestions de reponse IA

- A chaque message entrant, l'IA genere 1 a 3 suggestions de reponse
- Les suggestions tiennent compte de :
  - La categorie du message
  - L'historique de conversation
  - Le dossier patient (derniere visite, prescriptions en cours)
  - La langue du message entrant (reponse dans la meme langue)
- Le praticien peut : cliquer pour pre-remplir, modifier, ou ignorer completement
- L'IA ne repond JAMAIS directement au proprietaire -- validation humaine obligatoire
- Les suggestions sont generees en arriere-plan pour ne pas ralentir l'affichage du message

### 5.3 Resume automatique de conversation longue

- Declenche automatiquement quand une conversation depasse 5 messages
- Affiche en haut du fil de conversation, dans un encadre distinct
- Format : resume factuel en 3-5 phrases, sans interpretation medicale
- Le resume est regenere a chaque nouveau message
- Le praticien peut replier/deplier le resume

### 5.4 Lien automatique au dossier patient

- Quand le proprietaire selectionne un animal a l'etape d'envoi, le lien est automatique
- Si le message arrive sans selection d'animal (categorie "Autre"), le systeme tente un matching automatique : nom owner + nom animal mentionne dans le texte
- En cas de doute, la receptionniste peut lier manuellement le message a un patient
- Depuis le message : lien cliquable vers la fiche patient complete
- Depuis la fiche patient : onglet "Messages" affichant toutes les conversations liees

### 5.5 Historique de conversation persistant

- Toutes les conversations sont conservees indefiniment (pas de suppression automatique)
- Un fil de conversation = un sujet. Le proprietaire peut ouvrir un nouveau fil pour un nouveau sujet
- Les notes internes (visibles uniquement par l'equipe) sont intercalees dans le fil avec un marquage visuel distinct
- Chaque message porte un horodatage en timezone Asia/Dubai

### 5.6 Notifications temps reel

- **Proprietaire** : notification email uniquement (pas de push au MVP -- pas d'app native)
- **Equipe clinique** (dans Vetolib) :
  - Badge inbox mis a jour en temps reel (WebSocket ou SSE)
  - Notification push navigateur pour les urgences medicales
  - Escalade automatique si urgence non consultee apres 10 minutes
- Configuration par clinique :
  - Heures de notification (ex : pas de notification non urgente entre 22h et 7h)
  - Les urgences declenchent TOUJOURS une notification, quelle que soit l'heure

### 5.7 Templates de reponses rapides

- L'admin de la clinique configure des templates de reponse
- Exemples par defaut :
  - "Merci pour votre message. Nous avons bien note votre demande de rendez-vous. Un creneau vous sera propose sous peu."
  - "Si les symptomes s'aggravent, n'hesitez pas a vous presenter directement a la clinique."
  - "Votre facture est disponible dans votre espace. Souhaitez-vous la recevoir par email ?"
- Les templates sont disponibles en EN et AR
- Le praticien peut inserer un template et le personnaliser avant envoi

### 5.8 Actions contextuelles depuis un message

- **Convertir en RDV** : cree un rendez-vous pre-rempli dans le module Agenda (patient, owner, motif extrait du message)
- **Ajouter au dossier** : rattache le message (texte + photos) comme note dans le dossier medical du patient (module MedicalRecords)
- **Transferer** : reassigne a un autre role ou un praticien specifique
- **Marquer comme spam** : masque le message (admin peut consulter les spams)

---

## 6. Contraintes marche UAE

### 6.1 Bilinguisme EN/AR

- Interface du portail proprietaire : entierement traduite EN et AR
- Interface inbox equipe Vetolib : suit la langue de l'interface utilisateur (deja geree par Vetolib)
- Messages : le proprietaire ecrit dans la langue de son choix. L'IA detecte la langue et genere les suggestions de reponse dans la meme langue
- Les templates de reponse existent en version EN et AR
- Layout RTL (right-to-left) pour l'arabe sur le portail proprietaire

### 6.2 PDPL UAE (Personal Data Protection Law)

- **Consentement explicite** : le proprietaire doit accepter les conditions d'utilisation de la messagerie avant d'envoyer son premier message
- Le consentement est enregistre avec horodatage et version des conditions
- **Droit d'acces** : le proprietaire peut telecharger l'integralite de ses conversations
- **Droit de suppression** : le proprietaire peut demander la suppression de ses messages. La clinique a 30 jours pour traiter la demande. Les notes internes de l'equipe ne sont PAS supprimees (elles appartiennent au dossier medical)
- **Retention** : pas de suppression automatique, mais l'admin peut configurer une politique de retention (ex : archivage apres 2 ans)
- **Chiffrement** : messages chiffres au repos (AES-256) et en transit (TLS 1.3)
- **Localisation des donnees** : les donnees restent dans la region UAE (contrainte d'hebergement)

### 6.3 Heures de bureau vs urgences

- Chaque clinique configure ses heures de messagerie (ex : dimanche-jeudi 8h-20h, vendredi 8h-12h, samedi ferme -- calendrier UAE)
- En dehors des heures de messagerie :
  - Les messages non urgents : le proprietaire recoit un auto-reponse "Votre message sera traite a l'ouverture de la clinique" (seule exception a la regle "pas de reponse automatique" -- c'est un accuse de reception, pas une reponse medicale)
  - Les urgences medicales : notification envoyee quand meme au vet de garde (si configure)
- Le vet de garde est configurable dans le module Agenda (lien avec le planning)

### 6.4 Jours feries UAE

- Le systeme integre le calendrier des jours feries UAE (Eid Al Fitr, Eid Al Adha, National Day, etc.)
- Les jours feries sont traites comme des jours fermes sauf configuration contraire
- Les SLA de reponse ne comptent que les heures ouvrees

### 6.5 Integration future WhatsApp Business API

- Hors scope du MVP, mais l'architecture doit prevoir un canal abstrait
- Au MVP, le seul canal est le portail web
- Futur : un message WhatsApp arrive dans la meme inbox, meme triage, meme workflow
- Le proprietaire ne doit pas avoir deux conversations separees s'il utilise le portail ET WhatsApp

---

## 7. Hors scope initial (MVP)

Les elements suivants sont explicitement exclus du MVP :

| Element | Raison | Horizon |
|---|---|---|
| WhatsApp Business API | Cout d'integration + certification Meta | V2 (6 mois post-MVP) |
| SMS | Canal secondaire, email suffit au MVP | V2 |
| App mobile native proprietaire | Le portail web responsive suffit au MVP | V3 |
| Appels video / telemedecine | Reglementation UAE pas encore claire pour la telemedecine veterinaire | V3+ |
| Paiement dans la messagerie | Complexite payment gateway + reglementation | V2 |
| Analyse d'images IA | Fiabilite insuffisante pour le medical, risque de responsabilite | V3+ apres validation clinique |
| Chatbot autonome (reponse sans humain) | Risque medical, responsabilite juridique | Jamais pour le medical, eventuellement pour l'admin en V3 |
| Multi-clinique pour un meme proprietaire | Un proprietaire = un portail par clinique au MVP | V2 |
| Traduction automatique des messages | Le praticien doit lire dans la langue originale au MVP | V2 |

---

## 8. Metriques de succes

### Metriques primaires (KPI)

| Metrique | Baseline (avant) | Objectif 3 mois | Objectif 6 mois |
|---|---|---|---|
| Temps moyen premiere reponse (heures ouvrees) | Non mesure (estime 8h+) | 2h | 1h |
| % messages correctement tries par l'IA | N/A | 75% | 85% |
| % urgences repondues en moins de 15 min | Non mesure | 80% | 95% |
| Taux d'adoption proprietaires (% qui utilisent le portail) | 0% | 20% | 40% |

### Metriques secondaires

| Metrique | Description |
|---|---|
| NPS proprietaires messagerie | Enquete trimestrielle, objectif > 40 |
| Temps moyen de traitement par message | Cible : moins de 3 minutes par message avec les suggestions IA |
| Taux d'utilisation des suggestions IA | % de reponses ou le praticien utilise/modifie une suggestion (vs texte libre) -- objectif > 50% |
| Taux de re-categorisation | % de messages ou le triage IA est corrige par l'humain -- objectif < 25% a 3 mois, < 15% a 6 mois |
| Volume messages par clinique par jour | Suivi de la charge, dimensionnement infra |
| Taux de conversion message vers RDV | % de conversations qui debouchent sur un rendez-vous pris |

### Collecte des metriques

- Les metriques de triage et de temps de reponse sont calculees automatiquement par le systeme
- Le NPS est collecte via un lien en fin de conversation ("Cette conversation vous a-t-elle ete utile ?")
- L'admin de la clinique a acces a un dashboard metriques (ecran dedie dans Vetolib)
- Vetolib (editeur) a acces aux metriques agregees anonymisees pour ameliorer le modele IA

---

## Annexe A : Glossaire

| Terme | Definition |
|---|---|
| Owner | Proprietaire d'un animal, client de la clinique |
| Patient | L'animal enregistre dans Vetolib |
| Fil / Thread | Une conversation sur un sujet donne entre un owner et la clinique |
| Triage | Classification automatique d'un message par l'IA |
| Routing | Attribution du message au bon role/praticien |
| SLA | Temps de reponse cible pour une categorie de message |
| Template | Modele de reponse pre-redigee, personnalisable |
| Note interne | Message visible uniquement par l'equipe clinique, pas par le proprietaire |
| Escalade | Mecanisme de notification elargie si un message urgent reste sans reponse |

## Annexe B : Liens avec les modules existants

| Module Vetolib | Interaction avec Messaging |
|---|---|
| Auth | Roles et permissions : qui voit quoi dans l'inbox. Le portail owner utilise un mecanisme d'auth separe (magic link, pas JWT classique) |
| MedicalRecords | Lien message-patient. Action "Ajouter au dossier". Contexte medical affiche dans l'inbox vet |
| Agenda | Action "Convertir en RDV". Vet de garde pour les urgences hors heures |
| Billing | Contexte factures affiche pour les questions admin. Pas d'interaction directe au MVP |

## Annexe C : Risques identifies

| Risque | Impact | Mitigation |
|---|---|---|
| Triage IA incorrect sur une urgence | Medical (animal en danger) | Seuil de confiance bas pour les urgences : en cas de doute, classer en urgence plutot que sous-classer. Faux positifs preferables aux faux negatifs |
| Adoption faible des proprietaires | Business (investissement sans ROI) | Onboarding en clinique : la receptionniste montre le portail au proprietaire apres chaque consultation |
| Surcharge de messages non pertinents | Operationnel (bruit dans l'inbox) | Limite de messages par jour par owner (5/jour). Anti-spam basique |
| Responsabilite medicale sur les suggestions IA | Juridique | Disclaimer clair : "suggestion generee par IA, a valider par le veterinaire". Pas de conseil medical direct |
| Cout IA (tokens LLM) | Financier | Monitoring du cout par message. Alerte si le cout moyen depasse un seuil. Caching des suggestions similaires |
