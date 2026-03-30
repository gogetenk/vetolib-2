# todo-test-validators-014.md -- Add unit tests for untested FluentValidation validators

**Module** : All modules
**Priority** : Haute
**Dependencies** : aucune

## Context

Only 16% of validators have unit tests (11/67). This is a critical gap — validators are the first line of defense against bad input.

## Scope

Add unit tests for all untested validators across all modules. Priority order:
1. Auth validators (RegisterClinic, Login, ChangePassword, InviteUser)
2. Agenda validators (CreateAppointment, UpdateAppointment, CreateConsultationType)
3. Billing validators (CreateInvoice, UpdateInvoiceStatus)
4. Breeding validators (all new — CreateLitter, RecordHeatCycle, CreatePregnancy, etc.)
5. MedicalRecords validators (CreatePatient, UpdatePatient, AddWeightEntry)
6. Messaging validators
7. Stock validators

Each validator test should cover: valid input passes, each invalid field fails with correct message.

## Completion criteria
- [ ] All validators have at least 1 test for valid input + 1 test per validation rule
- [ ] `dotnet build` + `dotnet test` GREEN
