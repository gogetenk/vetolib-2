# todo-back-billing-einvoicing-gateway-007 -- Interface et integration PDP/PPF

**Module** : Billing
**Priorite** : Critique
**Prerequis** : `todo-back-billing-facturx-gen-005`
**Skills** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`
**Etude** : `docs/studies/E-INVOICING-FRANCE-STUDY-2026.md`

---

## Contexte

Les factures Factur-X doivent etre transmises a l'administration fiscale via une PDP (Plateforme de Dematerialisation Partenaire) ou le PPF (Portail Public de Facturation). Vetolib agit comme OD (Operateur de Dematerialisation).

## Objectif

Implementer l'abstraction `IEInvoicingGateway` et une premiere implementation (PPF/Chorus Pro ou mock).

## Travail a faire

### Contracts (public)

- [ ] Creer `IEInvoicingGateway` :
  ```csharp
  public interface IEInvoicingGateway
  {
      Task<Result<EInvoiceSubmissionResult>> SubmitInvoiceAsync(EInvoicePayload payload, CancellationToken ct);
      Task<Result<EInvoiceStatus>> GetStatusAsync(string platformInvoiceId, CancellationToken ct);
      Task<Result> SubmitEReportingAsync(EReportingPayload payload, CancellationToken ct);
  }
  ```
- [ ] Creer les DTOs : `EInvoicePayload`, `EInvoiceSubmissionResult`, `EInvoiceStatus`, `EReportingPayload`
- [ ] Creer l'enum `EInvoicingPlatformStatus` : `Submitted`, `Accepted`, `Rejected`, `Refused`, `Received`

### Runtime (internal)

- [ ] Implementer `ChorusProGateway` (ou `MockEInvoicingGateway` pour les tests)
- [ ] Ajouter un `EInvoicingStatus` sur l'entite `Invoice` (nullable, null pour les cliniques non-FR)
- [ ] Creer le command `SubmitToEInvoicingCommand` + handler
- [ ] Creer le endpoint `POST /api/v1/invoices/{id}/submit-einvoicing`
- [ ] Creer un background job pour le polling des statuts (ou webhook receiver)
- [ ] Migration EF Core pour le champ `EInvoicingStatus`

### Integration avec le cycle de vie

Le flux est :
1. Facture creee (Draft)
2. Facture finalisee (Sent) --> declenche automatiquement la soumission e-invoicing si clinique FR
3. PDP retourne un statut (Deposee, Acceptee, Rejetee)
4. Le statut est stocke sur l'Invoice

## Critere de completion

- [ ] L'interface `IEInvoicingGateway` est dans les Contracts
- [ ] Un mock gateway fonctionne pour les tests
- [ ] Le endpoint de soumission existe et retourne le bon statut
- [ ] Le statut e-invoicing est visible dans le DTO
- [ ] Les cliniques non-FR ne sont pas impactees
- [ ] `dotnet build` + `dotnet test` GREEN avant commit

## Question ouverte

Le choix de la PDP partenaire n'est pas encore fait. L'implementation initiale sera un mock ou le PPF gratuit. A discuter avec le PO : `questions/billing-pdp-choice-001.md`
