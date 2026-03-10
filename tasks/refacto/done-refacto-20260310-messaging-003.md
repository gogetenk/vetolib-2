# todo-refacto-20260310-messaging-003 -- ResponseTemplate.Category should be MessageCategory? not string?
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/Messaging/Vetolib.Messaging/Application/Domain/ResponseTemplate.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging/Infrastructure/ResponseTemplateConfiguration.cs`
- `src/backend/Modules/Messaging/Vetolib.Messaging.Contracts/ResponseTemplateDto.cs`
**Violation** : Spec MESSAGING-SPEC.md section 8 defines `Category?` as "(optional -- template can be general or category-specific)". The domain uses `string? Category` instead of `MessageCategory? Category`. This loses type safety and allows invalid category values.
**Correction attendue** :
1. Change `ResponseTemplate.Category` from `string?` to `MessageCategory?`
2. Update `Create()` and `Update()` factory/method signatures accordingly
3. Update `ResponseTemplateDto.Category` from `string?` to `MessageCategory?`
4. Update EF configuration to use `HasConversion<string>()` for the enum
**Critere** : `grep -r "string? Category" Modules/Messaging/ --include="*.cs"` returns 0 results (excluding migrations)
