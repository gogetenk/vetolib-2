# todo-refacto-20260309-ai-004 -- TriageResult.Create public visibility leak
**Priorite** : mineure
**Fichiers concernes** :
- `src/backend/Modules/AI/Vetolib.AI/Application/Domain/TriageResult.cs` (ligne 31)
**Violation** : La methode `Create`, `Accept`, `Override` sur `TriageResult` sont `public` alors que la classe est `internal`. Cela compile car les methodes sont de facto internes via la classe, mais cela ne respecte pas l'intention d'isolation stricte. Si un refacto futur rend la classe public par erreur, ces methodes seront exposees.
**Correction attendue** : Aucune action urgente -- la classe etant `internal`, la visibilite effective est correcte. A harmoniser si le module evolue.
**Critere** : Les methodes de domaine sont marquees `internal` ou la classe reste `internal`.
