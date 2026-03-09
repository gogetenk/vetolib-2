# tasks/todo-agenda-001.md
**Module** : Agenda  
**Status** : [TODO]  
**Dépendances** : todo-auth-001.md (auth middleware requis)  
**Gherkins** : features/agenda/appointments.feature

## Description
Implémenter la création de rendez-vous avec détection de conflits.

Endpoints : `POST /api/v1/appointments`, `GET /api/v1/appointments`

Inclure : détection de chevauchement, validation horaires d'ouverture, validation date passée.
