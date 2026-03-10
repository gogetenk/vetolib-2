# todo-front-messaging-conversation-001.md — Composant conversation detail

**Module** : Frontend (Messaging)
**Dependances** : todo-front-messaging-inbox-001
**Priorite** : HAUTE
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**[Branchement ulterieur]** : wire-messaging-conversation

---

## Objectif

Creer le composant de detail d'une conversation : thread de messages, panneau AI suggestions, panneau contexte patient, boutons d'action.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 4.2 (Conversation detail), section 2.3 (AI Role), section 2.8 (Internal Notes)

## Implementation

### 1. Page conversation detail

Route : `/[locale]/messages/{id}`

Layout 3 colonnes (desktop) :
- **Gauche** : thread de messages (chronologique)
- **Droite haut** : panneau contexte patient (si conversation liee a un patient)
- **Droite bas** : panneau AI suggestions (1-3 suggestions cliquables)

Sur mobile : layout vertical (thread, puis contexte, puis suggestions).

### 2. Thread de messages

- Messages owner : bulle a gauche, fond clair
- Messages staff/vet : bulle a droite, fond colore
- Messages systeme (auto-acknowledgment) : centre, fond gris, texte italique
- Internal notes : fond jaune, label "Internal note", jamais visible par l'owner
- Timestamps relatifs ("2 hours ago") + hover pour la date complete
- Attachments : preview image cliquable (lightbox simple)

### 3. Resume AI (conversations > 5 messages)

- Card collapsible en haut du thread
- Texte 3-5 phrases factuel
- Indicateur "Loading summary..." pendant la generation
- Visible uniquement par VetOrAdmin

### 4. Panneau AI suggestions

- 1-3 suggestions affichees comme des cartes cliquables
- Cliquer une suggestion pre-remplit le textarea de reponse
- Disclaimer constant en bas du panneau : "This response was pre-drafted by AI. It will be reviewed and validated by a veterinarian before sending."
- Indicateur "Loading suggestions..." pendant la generation

### 5. Panneau contexte patient

- **Receptionist** : nom du pet, espece, dernier RDV, factures impayees
- **Vet/Admin** : + dernier examen, prescriptions en cours, allergies, vaccinations
- Si pas de patient lie : afficher "No patient linked"

### 6. Zone de reponse

- Textarea avec compteur de caracteres (max 2000)
- Bouton "Send" (data-testid="send-reply-btn")
- Bouton "Add internal note" (data-testid="add-note-btn") -- VetOrAdmin only
- Bouton "Attach to medical record" (data-testid="add-to-record-btn") -- VetOrAdmin only

### 7. Boutons d'action (toolbar)

- "Transfer" (data-testid="transfer-btn") -- dialog pour choisir role/user
- "Convert to appointment" (data-testid="convert-appointment-btn")
- "Mark as spam" (data-testid="mark-spam-btn")
- "Resolve" / "Close" / "Reopen" selon le status actuel
- "Reassign" (AdminOnly, data-testid="reassign-btn")
- Assistant : aucun bouton d'action visible

### 8. Composants

- `ConversationDetail.tsx` : page principale
- `MessageThread.tsx` : liste des messages
- `MessageBubble.tsx` : bulle individuelle
- `AiSuggestionsPanel.tsx` : panneau suggestions
- `PatientContextPanel.tsx` : panneau contexte patient
- `ConversationSummary.tsx` : resume AI collapsible
- `ReplyComposer.tsx` : textarea + boutons
- `ConversationActions.tsx` : toolbar d'actions

## Critere

```
[] Page /[locale]/messages/{id} creee
[] Thread de messages avec bulles differenciees (owner, staff, system, internal note)
[] Resume AI collapsible pour conversations > 5 messages
[] Panneau AI suggestions avec 1-3 cartes cliquables
[] Panneau contexte patient filtre par role
[] Zone de reponse avec compteur caracteres
[] Boutons d'action (transfer, convert, spam, resolve/close/reopen)
[] Disclaimer AI constant affiche
[] data-testid sur tous les elements interactifs
[] Responsive
[] npm run dev fonctionne
[] Renommer en done
```
