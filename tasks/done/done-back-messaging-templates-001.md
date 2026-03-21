## done-back-messaging-templates-001

Tâche : CRUD pour les templates de réponse rapide du module Messaging.

### Implémenté

- Query : ListTemplates (filtre optionnel par catégorie, policy ClinicStaff)
- Commands : CreateTemplate, UpdateTemplate, DeleteTemplate (policy AdminOnly)
- 4 endpoints sous /api/v1/messaging/templates
- Templates bilingues EN + AR (ContentEn + ContentAr obligatoires)
- Tous les handlers retournent Result<T>
- .ToMinimalApiResult() sur tous les endpoints

### Fix inclus

Correction de l'erreur de compilation pré-existante dans CreateOwnerConversationHandler :
le handler retournait Result<Guid> mais le command déclarait IRequest<Result<CreateOwnerConversationResponse>>.

### Build

dotnet build Vetolib.Api.csproj --configuration Release : 0 erreur
