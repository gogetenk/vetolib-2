# Task: Unit tests for Notifications consumers

**Module**: Notifications
**Type**: test
**Priority**: high (SonarCloud quality gate)

## Context
SonarCloud quality gate fails at 59.9% coverage on new code (needs 80%).
These MassTransit consumers changed in PR #14 but have no unit tests.

## Files to cover
1. `Consumers/AppointmentReminderConsumer.cs`
2. `Consumers/InvoiceSentConsumer.cs`
3. `Consumers/SendMagicLinkConsumer.cs`
4. `Consumers/UserInvitedConsumer.cs`

## Acceptance criteria
- [ ] Unit tests for each consumer using NSubstitute mocks for ISender/ILogger
- [ ] Test: valid message → sends MediatR command
- [ ] Test: invalid message → handles gracefully
- [ ] All tests GREEN locally before PR
- [ ] Add Vetolib.Notifications project reference to Vetolib.Tests.Unit.csproj if missing

## Skills
`ardalis-result`, `cqrs-mediatr`
