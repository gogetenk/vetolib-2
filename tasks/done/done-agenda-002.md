# tasks/todo-agenda-002.md
**Module** : Agenda  
**Status** : [TODO]  
**Dépendances** : todo-agenda-001.md  
**Gherkins** : features/agenda/appointments.feature (scénarios statuts + disponibilités)

## Description
Implémenter les transitions de statut et l'API de disponibilités.

Endpoints : `PATCH /api/v1/appointments/{id}/status`, `GET /api/v1/appointments/availability`

Inclure : machine à états des statuts, permissions par rôle sur les transitions.
