# Question -- messaging-clinic-attachment-001

**Module** : Messaging
**Bloquant** : Non (V1.1 feature candidate)

## Probleme

La spec MESSAGING-SPEC.md prevoit que les owners peuvent attacher des photos (max 3, JPG/PNG, 5MB chacune). L'entite `MessageAttachment` existe et est fonctionnelle pour les messages owner.

Cependant, il n'y a pas de mecanisme pour que le staff (vet, receptionniste) attache des documents lorsqu'il repond a un owner. Cas d'usage concrets :
- Vet partage des resultats d'analyses de laboratoire
- Vet partage un rapport de radiographie
- Receptionniste partage un devis ou un recapitulatif de soins
- Vet partage une ordonnance (PDF)

C'est une fonctionnalite que Doctolib propose et qui est attendue par les cliniques.

## Options

**Option A** : Ajouter l'attachement de documents aux reponses staff dans le MVP
- Impact : modifier SendReplyCommand, ajouter upload endpoint, modifier le frontend ReplyComposer
- Risque : complexite supplementaire, formats de fichiers a valider (PDF, JPG, PNG, DOCX ?)

**Option B** : Reporter en V1.1 (fast follow post-MVP)
- Impact : aucun changement au MVP
- Risque : les cliniques devront partager les documents par email separement en attendant

**Option C** : V1.1 mais limiter aux PDF et images
- Impact : scope reduit, validation simplifiee
- Risque : moindre que Option A

## Recommandation PO

Option C en V1.1. Le MVP se concentre sur la messagerie texte. L'ajout de documents cote staff est un excellent candidat pour le fast follow. Limiter a PDF + images evite les problemes de securite avec les formats Office. L'architecture MessageAttachment est deja prete pour supporter les deux sens (owner -> clinic et clinic -> owner).

-> Escalade humain requise : non
