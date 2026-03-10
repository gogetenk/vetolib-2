# todo-refacto-20260310-audit-002 -- throw new dans Preferences SystemDefaults
**Priorite** : critique
**Fichiers concernes** :
- `src/backend/Modules/Preferences/Vetolib.Preferences/Application/Domain/SystemDefaults.cs`

**Violation** : Regle 1 CLAUDE.md -- "Zero exception pour le control flow business". `GetDefault()` et `GetCategory()` lancent `throw new InvalidOperationException` quand une PreferenceKey n'a pas de valeur par defaut.
**Correction attendue** : Convertir en `Result<string> GetDefault(PreferenceKey key)` et `Result<PreferenceCategory> GetCategory(PreferenceKey key)` retournant `Result.NotFound()` quand la cle n'existe pas, au lieu de throw.
**Critere** : [] `grep -r "throw new" Modules/Preferences/ --include="*.cs"` ne retourne plus de resultats
