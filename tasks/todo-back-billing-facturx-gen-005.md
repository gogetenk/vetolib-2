# todo-back-billing-facturx-gen-005 -- Generateur Factur-X (PDF/A-3 + XML CII)

**Module** : Billing
**Priorite** : Critique
**Prerequis** : `todo-back-billing-multi-tax-001`, `todo-back-billing-invoice-fields-003`
**Skills** : `ardalis-result`, `cqrs-mediatr`
**Etude** : `docs/studies/E-INVOICING-FRANCE-STUDY-2026.md`

---

## Contexte

La facturation electronique francaise exige le format Factur-X : un PDF/A-3 contenant un fichier XML CII (Cross-Industry Invoice) embarque en piece jointe.

Le generateur PDF actuel (`InvoicePdfGenerator`) utilise QuestPDF pour produire un PDF simple. Il faut le faire evoluer (ou creer un nouveau generateur) pour produire du Factur-X.

## Objectif

Quand une clinique francaise genere une facture, le PDF produit est un Factur-X valide au profil EN16931 (Comfort).

## Approche technique

### Bibliotheques .NET

- **XML CII** : generer le XML manuellement (template + serialisation) ou utiliser `ZUGFeRD-csharp` (NuGet: `s2industries.ZUGFeRD`)
- **PDF/A-3** : QuestPDF ne supporte pas nativement PDF/A-3. Options :
  - Convertir le PDF QuestPDF en PDF/A-3 via `iTextSharp` ou `PdfSharp`
  - Utiliser `itext7` pour generer directement un PDF/A-3 avec XML embarque
  - Utiliser la lib `Factur-X.NET` (NuGet) qui fait le packaging

### Architecture

```
IInvoicePdfGenerator (Contracts)
  |-- StandardPdfGenerator (existant, pour UAE/PL)
  |-- FacturXPdfGenerator (nouveau, pour FR)
```

Le handler resout le bon generateur selon le `CountryCode` de la clinique.

## Travail a faire

- [ ] Ajouter les NuGet necessaires (`s2industries.ZUGFeRD` ou equivalent)
- [ ] Creer `IInvoicePdfGenerator` dans les Contracts
- [ ] Refactorer `InvoicePdfGenerator` existant pour implementer l'interface
- [ ] Creer `FacturXPdfGenerator` :
  - Generer le XML CII EN16931 a partir de `InvoiceDto`
  - Generer le PDF/A-3 via QuestPDF + conversion
  - Embarquer le XML dans le PDF comme piece jointe (XMP metadata)
- [ ] Creer un service factory qui resout le bon generateur selon le pays
- [ ] Mettre a jour le handler `GenerateInvoicePdfHandler`
- [ ] TU : valider le XML CII genere contre le schema XSD Factur-X
- [ ] TU : verifier que le PDF contient bien la piece jointe XML

## Critere de completion

- [ ] Les cliniques FR recoivent un Factur-X valide (PDF/A-3 + XML CII EN16931)
- [ ] Les cliniques UAE recoivent toujours un PDF simple (pas de regression)
- [ ] Le XML CII passe la validation XSD
- [ ] `dotnet build` + `dotnet test` GREEN avant commit

## References

- Specification Factur-X : https://factur-x.org/
- Schema CII : UN/CEFACT D16B
- Profil EN16931 : norme europeenne de facturation electronique
- NuGet s2industries.ZUGFeRD : generation XML ZUGFeRD/Factur-X
