# todo-front-messaging-owner-portal-001.md — Portail owner (magic link)

**Module** : Frontend (Messaging)
**Dependances** : todo-front-messaging-msw-001
**Priorite** : HAUTE
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**[Branchement ulterieur]** : wire-messaging-portal

---

## Objectif

Creer le portail web owner accessible via magic link. Le portail est brande avec le nom de la clinique, responsive (mobile-first), et supporte RTL (arabe).

## Spec de reference

`docs/MESSAGING-SPEC.md` section 4.1 (Owner Portal)

## Implementation

### 1. Pages

**Portal landing** : `/portal/{clinicSlug}`
- Affiche le nom et logo de la clinique
- Liste des conversations existantes de l'owner
- Bouton "New Message" (data-testid="new-message-btn")
- Si token expire : message "This link has expired. Please contact your clinic to receive a new one."

**Consent screen** : `/portal/{clinicSlug}/consent`
- Affiche les termes et conditions de la messagerie
- Checkbox "I accept" + bouton "Continue" (data-testid="accept-consent-btn")
- Redirige vers /new apres acceptation
- Premiere visite uniquement

**New message** : `/portal/{clinicSlug}/new`
- Selecteur de pet (dropdown) -- si pas de pets, seule categorie "Other" disponible
- Selecteur de categorie (liste descriptive, pas les enums techniques)
  - "My pet has a health problem" -> MedicalUrgency/MedicalQuestion
  - "Post-surgery follow-up" -> PostOperativeFollowUp
  - "I'd like to book an appointment" -> AppointmentRequest
  - "Administrative question" -> Administrative
  - "Feedback about the clinic" -> Feedback
  - "Other" -> Other
- Textarea avec compteur de caracteres (2000 max, data-testid="message-input")
- Upload photos : max 3, JPG/PNG, 5 MB chacune (data-testid="photo-upload")
  - Preview des photos avec bouton remove
  - Erreur si > 3 photos ou > 5 MB
- Bouton "Send" (data-testid="send-message-btn")
- Confirmation apres envoi avec estimation du temps de reponse

**Conversation view** : `/portal/{clinicSlug}/conversations/{id}`
- Thread de messages chronologique
- Pas de notes internes visibles
- Zone de reponse en bas (textarea + send)
- Indication "Conversation closed" si status Closed (pas de zone de reponse)

**Export** : `/portal/{clinicSlug}/export`
- Bouton "Download my messages" (data-testid="download-export-btn")
- Telecharge un fichier texte avec toutes les conversations

### 2. Layout portail

- Pas de navigation vers l'app Vetolib principale
- Header simple avec nom de la clinique + selecteur de langue (EN/AR)
- RTL layout quand AR est selectionne
- Mobile-first : la plupart des owners utilisent leur telephone

### 3. Magic link authentication

- Le token est dans le query string de l'URL (`?token=...`)
- Le token est stocke en sessionStorage pour les requetes API suivantes
- Header `X-Portal-Token` envoye avec chaque requete

### 4. Limites et erreurs

- Si 5 messages envoyes aujourd'hui : afficher "You have reached the daily message limit. Please try again tomorrow."
- Si message > 2000 chars : bouton Send desactive + compteur rouge
- Si photo > 5 MB : erreur "File too large (max 5 MB)"
- Si > 3 photos : erreur "Maximum 3 photos per message"

### 5. Composants

- `PortalLayout.tsx` : layout brande clinique
- `PortalLanding.tsx` : page d'accueil avec liste conversations
- `ConsentScreen.tsx` : ecran consentement
- `NewMessageForm.tsx` : formulaire nouveau message
- `PortalConversation.tsx` : vue conversation owner
- `PhotoUpload.tsx` : composant upload avec preview
- `PetSelector.tsx` : selecteur de pet
- `CategorySelector.tsx` : selecteur de categorie (labels user-friendly)

## Critere

```
[] 5 pages portal creees avec routing
[] Magic link auth via query string + sessionStorage
[] Consentement obligatoire avant premier message
[] Selecteur de pet + categorie
[] Textarea avec compteur 2000 chars
[] Upload photos (max 3, 5 MB, JPG/PNG) avec preview
[] Export conversations en texte
[] RTL support pour l'arabe
[] Mobile-first responsive
[] Token expire : message d'erreur clair
[] Limite 5 messages/jour affichee
[] data-testid sur tous les elements interactifs
[] npm run dev fonctionne
[] Renommer en done
```
