# todo-back-attachments-001.md — IFileStorage abstraction + Azure Blob implementation

**Module** : Messaging
**Dependencies** : none
**Priority** : high

## Objective
Create the `IFileStorage` interface in Messaging.Contracts and implement Azure Blob Storage + local filesystem for dev.

## Scope
1. `IFileStorage` interface: Upload(stream, filename, contentType) → url, Delete(url), GetPresignedUrl(path, expiry)
2. `AzureBlobFileStorage` implementation (Azure.Storage.Blobs)
3. `LocalFileStorage` implementation for dev/tests
4. DI registration in ModuleServiceRegistrar
5. Config via appsettings (connection string, container name)

## Completion criteria
- [ ] IFileStorage in Contracts
- [ ] Azure + Local implementations
- [ ] Unit tests for LocalFileStorage
- [ ] Build GREEN
