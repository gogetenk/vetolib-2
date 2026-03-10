# todo-refacto-20260309-back-007 — MustChangePassword non expose dans UserDto
**Priorite** : mineure
**Fichiers concernes** :
- `src/backend/Modules/Auth/Vetolib.Auth.Contracts/UserDto.cs`
- `src/backend/Modules/Auth/Vetolib.Auth/Application/Domain/User.cs` (ligne 166, methode ToDto)
**Violation** : Le champ `MustChangePassword` existe sur le domaine User mais n'est pas propage dans `UserDto`. Le frontend ne peut pas savoir qu'un utilisateur invite doit changer son mot de passe. Ce n'est pas une violation d'architecture mais un oubli fonctionnel detecte lors de l'audit.
**Correction attendue** : Ajouter `bool MustChangePassword` a `UserDto` et le mapper dans `User.ToDto()`.
**Critere** : `UserDto` contient `MustChangePassword` et le mapping est correct.
