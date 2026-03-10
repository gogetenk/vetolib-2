# todo-front-messaging-inbox-001.md — Page inbox staff (receptionist/vet)

**Module** : Frontend (Messaging)
**Dependances** : todo-front-messaging-msw-001
**Priorite** : HAUTE
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**[Branchement ulterieur]** : wire-messaging-inbox

---

## Objectif

Creer la page inbox messaging pour le staff (receptionist, vet, admin, assistant) avec filtrage par role et tri par priorite.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 4.2 (Staff Messaging)

## Implementation

### 1. Page inbox

Route : `/[locale]/messages`

Layout :
- Liste des conversations a gauche (ou en pleine page sur mobile)
- Filtres en haut : status (Open, InProgress, Resolved, Closed), category, date range
- Barre de recherche (texte)
- Chaque conversation affiche : sujet, categorie (badge couleur), status, dernier message preview, date, nom de l'owner

### 2. Filtrage par role

- **Receptionist** : voit AppointmentRequest, Administrative, Other uniquement
- **Vet** : voit MedicalUrgency, PostOperativeFollowUp, MedicalQuestion
- **Admin** : voit tout
- **Assistant** : voit non-medical (read-only, pas de boutons d'action)

Le filtrage est gere cote API (le MSW handler le simule), le frontend envoie le role dans la requete ou le backend le deduit du JWT.

### 3. Tri par priorite

Ordre : MedicalUrgency (rouge) > PostOperativeFollowUp > MedicalQuestion > AppointmentRequest > Administrative > Feedback > Other

A priorite egale, tri par date (plus ancien en premier pour SLA).

### 4. Badge unread count

- Badge dans le sidebar "Messages (N)" avec le nombre de conversations non lues
- Mis a jour en temps reel via SSE (tache separee, pour l'instant mock statique)

### 5. Composants

- `ConversationList.tsx` : liste des conversations
- `ConversationListItem.tsx` : item avec badge categorie + status
- `ConversationFilters.tsx` : filtres status, category, search
- `MessagesPage.tsx` : page principale qui compose tout

### 6. UI

- Badge couleur par categorie : MedicalUrgency = rouge, PostOperativeFollowUp = orange, MedicalQuestion = bleu, AppointmentRequest = vert, Administrative = gris, Feedback = violet, Other = gris
- Status : Open = bleu, InProgress = jaune, Resolved = vert, Closed = gris
- Conversations MedicalUrgency : fond rouge clair pour alerter visuellement
- `data-testid` sur tous les elements interactifs

## Critere

```
[] Page /[locale]/messages creee
[] Liste des conversations avec preview
[] Filtres status + category + search
[] Tri par priorite puis date
[] Filtrage par role (receptionist vs vet vs admin vs assistant)
[] Badge unread count dans le sidebar
[] Badges couleur par categorie
[] data-testid sur tous les boutons et filtres
[] Responsive (mobile + desktop)
[] npm run dev fonctionne
[] Renommer en done
```
