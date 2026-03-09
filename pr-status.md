# pr-status.md — État des Pull Requests

> Fichier de coordination entre agents QA, PR Reviewer, PO.
> L'orchestrateur lit ce fichier pour savoir quoi lancer.

## Statuts possibles
- `[DEV_DONE]` → PR ouverte, en attente de QA et Review (parallèle)
- `[QA_FAILED]` → Tests rouges, retour au dev
- `[QA_DONE]` → Tests verts, vidéo attachée, en attente PO
- `[REVIEW_CHANGES]` → Comments PR Reviewer, en attente corrections dev
- `[REVIEW_APPROVED]` → Review technique OK
- `[PO_REJECTED]` → PO a refusé, retour au dev avec motif
- `[PO_APPROVED]` → PO a validé, prête pour merge humain
- `[MERGED]` → Mergée

---

*Aucune PR pour l'instant.*

---

## Template
### PR #{num} — [{statut}]
**Tâche** : tasks/{id}.md  
**Module** : Auth / Agenda / MedicalRecords / Billing  
**Branche** : feat/{module}-{task-id}  
**Lien PR** : {url}  
**Vidéo démo** : {url ou "en attente"}  
**Ouvert le** : {timestamp}  
**Dernière activité** : {timestamp}  
