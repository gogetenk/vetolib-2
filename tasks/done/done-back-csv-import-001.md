# todo-back-csv-import-001.md — Import CSV patients/owners

**Module** : MedicalRecords
**Dépendances** : aucune
**Priorité** : MOYENNE (post-MVP, rétention)
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`

---

## Objectif

Permettre aux cliniques d'importer leurs patients et owners existants depuis un fichier CSV (migration depuis Excel/ancien logiciel).

## Implémentation

### Backend

1. **POST /api/v1/patients/import** (Admin ou Vet)
   - Accepte un fichier CSV multipart
   - Format attendu : `PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone`
   - Parsing avec CsvHelper (NuGet)
   - Création batch : Owner (si email pas déjà existant) + Patient + PatientOwner
   - Retourne un rapport : `{ imported: 45, skipped: 3, errors: ["Row 12: missing species"] }`

2. **Validation par ligne**
   - Species obligatoire
   - PatientName obligatoire
   - OwnerEmail optionnel (match par nom si absent)
   - Lignes invalides = skipped (pas de rollback global)

3. **Template CSV téléchargeable**
   - GET /api/v1/patients/import/template → fichier CSV avec headers + 2 lignes exemple

### Frontend

4. **Page /[locale]/patients avec bouton "Import CSV"**
   - Dialog avec drag-and-drop zone
   - Preview des 5 premières lignes
   - Bouton "Import" → progress bar → rapport résultat
   - i18n EN + AR

## Critère

```
□ POST /api/v1/patients/import fonctionne avec CSV
□ Rapport imported/skipped/errors retourné
□ Template CSV téléchargeable
□ UI import avec preview + rapport
□ Tests BDD : import OK, lignes invalides skippées, email dupliqué
□ Renommer en done
```
