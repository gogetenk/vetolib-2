# todo-refacto-20260310-audit-005 -- patients.ts utilise fetch brut au lieu de apiClient
**Priorite** : importante
**Fichiers concernes** :
- `src/frontend/src/lib/api/patients.ts` (lignes 102-120)

**Violation** : Les fonctions `importPatientsCsv()` et `downloadImportTemplate()` utilisent `fetch()` directement avec gestion manuelle des headers Authorization au lieu de passer par `apiClient` de `lib/api/client.ts`. Cela contourne le mecanisme de refresh token automatique et la gestion d'erreurs centralisee.
**Correction attendue** : Refactoriser `importPatientsCsv()` et `downloadImportTemplate()` pour utiliser `apiClient()` ou `apiGet()` de `client.ts`. Pour le FormData, `apiClient` supporte deja les options custom.
**Critere** : [] `grep -n "await fetch(" src/frontend/src/lib/api/patients.ts` retourne 0 resultats
