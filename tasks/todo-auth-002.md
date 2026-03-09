# tasks/todo-auth-002.md
**Module** : Auth  
**Status** : [TODO]  
**Dépendances** : todo-auth-001.md (login doit exister)  
**Gherkins** : features/auth/login.feature (scénarios multi-tenant)

## Description
Implémenter la gestion des utilisateurs par clinique.

Endpoints : `GET /api/v1/auth/me`, `POST /api/v1/users`, `GET /api/v1/users`

Inclure : middleware d'autorisation par rôle, Global Query Filter multi-tenant sur DbContext.
