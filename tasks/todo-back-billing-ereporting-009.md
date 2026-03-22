# todo-back-billing-ereporting-009 -- E-reporting B2C

**Module** : Billing
**Priorite** : Critique
**Prerequis** : `todo-back-billing-einvoicing-gateway-007`
**Skills** : `ardalis-result`, `cqrs-mediatr`
**Etude** : `docs/studies/E-INVOICING-FRANCE-STUDY-2026.md`

---

## Contexte

80-90% des factures veterinaires sont des factures B2C (proprietaires d'animaux particuliers). Ces factures ne passent pas par l'e-invoicing mais sont soumises au **e-reporting** : transmission periodique des donnees de transaction a l'administration fiscale.

## Obligations

- **Donnees transmises** : montant HT, TVA, TTC par taux de TVA, nombre de transactions
- **Frequence** :
  - Regime reel normal : J+10, J+20, J+30 du mois
  - Regime reel simplifie : mensuel
  - Micro-entreprises : bimestriel
- **E-reporting de paiement** : pour les prestations de services, declarer aussi les encaissements (TVA exigible a l'encaissement)

## Travail a faire

- [ ] Creer le concept `EReportingPeriod` dans le Domain (agregation des transactions B2C par periode)
- [ ] Creer un query `AggregateEReportingDataQuery` qui calcule les totaux par taux de TVA pour une periode donnee
- [ ] Creer un command `SubmitEReportingCommand` qui appelle `IEInvoicingGateway.SubmitEReportingAsync`
- [ ] Creer un background job qui declenche automatiquement la soumission selon la frequence configuree
- [ ] Ajouter un endpoint admin `POST /api/v1/billing/ereporting/submit` pour declenchement manuel
- [ ] Creer un endpoint `GET /api/v1/billing/ereporting/periods` pour consulter l'historique des soumissions
- [ ] Stocker l'historique des soumissions e-reporting (date, statut, montants)

## E-reporting de paiement

- [ ] Quand une facture de service passe au statut `Paid`, enregistrer l'encaissement
- [ ] Inclure les encaissements dans la transmission e-reporting periodique

## Critere de completion

- [ ] Les donnees B2C sont agregees correctement par periode et par taux de TVA
- [ ] La soumission periodique fonctionne (background job)
- [ ] L'historique des soumissions est consultable
- [ ] Les encaissements de prestations de services sont reportes
- [ ] Les cliniques non-FR ne sont pas impactees
- [ ] `dotnet build` + `dotnet test` GREEN avant commit
