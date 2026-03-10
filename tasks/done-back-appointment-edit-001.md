# todo-back-appointment-edit-001.md — Modification de rendez-vous

**Module** : Agenda
**Dépendances** : aucune
**Priorité** : BLOQUANT (PO review #6)

---

## Contexte

Impossible de modifier un RDV existant (changer date/heure, vétérinaire, motif). Une clinique doit pouvoir reprogrammer.

## Périmètre

### Backend
- `PATCH /api/appointments/{id}` — modifier un RDV avec statut SCHEDULED uniquement
- Champs modifiables : scheduledAt, vetId, reason, notes
- Vérifier les conflits horaires sur le nouveau créneau
- RDV en CHECKED_IN/IN_PROGRESS/COMPLETED/CANCELLED → 400

### Frontend
- Bouton "Reschedule" sur la page détail (visible uniquement si SCHEDULED)
- Dialog avec formulaire pré-rempli (date, heure, vétérinaire)
- Confirmation avant soumission

### BDD
- Scénario : modifier date/heure d'un RDV SCHEDULED → succès
- Scénario : modifier un RDV CHECKED_IN → 400
- Scénario : modifier vers un créneau occupé → conflit

## Critère
```
□ PATCH endpoint fonctionnel
□ Seuls les RDV SCHEDULED sont modifiables
□ Conflit horaire vérifié sur nouveau créneau
□ Bouton Reschedule + dialog frontend
□ 3 scénarios BDD GREEN
□ Renommer en done
```
