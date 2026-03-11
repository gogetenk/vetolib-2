using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Queries.GetOnboardingState;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class GetOnboardingStateHandlerTests
{
    private static readonly Guid FixedClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private AuthDbContext BuildContext()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(FixedClinicId);

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthDbContext(options, clinicContext, _publisher);
    }

    private static User CreateAdminUser(Guid clinicId, string email = "admin@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Admin1234!", UserRole.Admin);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    private static User CreateVetUser(Guid clinicId, string email = "vet@desertpaws.ae")
    {
        var result = User.Create(clinicId, email, "Vet12345!", UserRole.Vet, vetLicenseNumber: "LIC-001");
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    private void SetupSenderCountsZero()
    {
        _sender.Send(Arg.Any<GetPatientCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _sender.Send(Arg.Any<GetAppointmentCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _sender.Send(Arg.Any<GetInvoiceCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
    }

    private void SetupSenderCounts(int patients = 0, int appointments = 0, int invoices = 0)
    {
        _sender.Send(Arg.Any<GetPatientCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(patients));
        _sender.Send(Arg.Any<GetAppointmentCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(appointments));
        _sender.Send(Arg.Any<GetInvoiceCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(invoices));
    }

    [Fact]
    public async Task Handle_NewAdminUser_CreatesOnboardingStateAndReturnsInitialSteps()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        SetupSenderCountsZero();

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Steps.Should().HaveCount(5); // Admin has 5 steps
        result.Value.Steps.Should().OnlyContain(s => !s.IsCompleted);
        result.Value.Progress.Completed.Should().Be(0);
        result.Value.Progress.Total.Should().Be(5);
        result.Value.IsCompleted.Should().BeFalse();
        result.Value.WelcomeBannerVisible.Should().BeTrue();
        result.Value.ChecklistVisible.Should().BeTrue();

        // Verify the state was persisted in the DB
        var savedState = await context.OnboardingStates.FirstOrDefaultAsync(o => o.UserId == user.Id);
        savedState.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ExistingState_ReturnsCurrentProgress()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Pre-create the onboarding state with one step completed
        var stateResult = OnboardingState.Create(user.Id, "Admin", FixedClinicId);
        stateResult.IsSuccess.Should().BeTrue();
        var state = stateResult.Value;
        state.CompleteStep("invite_team_member");
        context.OnboardingStates.Add(state);
        await context.SaveChangesAsync();

        SetupSenderCountsZero();

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Progress.Completed.Should().Be(1);
        result.Value.Progress.Total.Should().Be(5);
        result.Value.Steps.First(s => s.StepId == "invite_team_member").IsCompleted.Should().BeTrue();
        result.Value.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PatientCountGreaterThanZero_AutoCompletesAddFirstPatientStep()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        SetupSenderCounts(patients: 1, appointments: 0, invoices: 0);

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var addPatientStep = result.Value.Steps.First(s => s.StepId == "add_first_patient");
        addPatientStep.IsCompleted.Should().BeTrue();
        result.Value.Progress.Completed.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AppointmentCountGreaterThanZero_AutoCompletesBookFirstAppointmentStep()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        SetupSenderCounts(patients: 0, appointments: 1, invoices: 0);

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var bookStep = result.Value.Steps.First(s => s.StepId == "book_first_appointment");
        bookStep.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvoiceCountGreaterThanZero_AutoCompletesCreateFirstInvoiceStep()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        SetupSenderCounts(patients: 0, appointments: 0, invoices: 1);

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var invoiceStep = result.Value.Steps.First(s => s.StepId == "create_first_invoice");
        invoiceStep.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AllAutoCompletableStepsDone_DoesNotMarkFullyCompleted_WhenManualStepsRemain()
    {
        // Arrange — Admin has 5 steps; 3 are auto-completed by counts, 2 require manual action
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        SetupSenderCounts(patients: 1, appointments: 1, invoices: 1);

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert — 3 auto-completed (add_first_patient, book_first_appointment, create_first_invoice)
        // 2 remain: invite_team_member + explore_dashboard
        result.IsSuccess.Should().BeTrue();
        result.Value.Progress.Completed.Should().Be(3);
        result.Value.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AllStepsComplete_MarksIsCompletedTrue()
    {
        // Arrange — pre-complete all admin steps manually
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var stateResult = OnboardingState.Create(user.Id, "Admin", FixedClinicId);
        var state = stateResult.Value;
        state.CompleteStep("invite_team_member");
        state.CompleteStep("explore_dashboard");
        context.OnboardingStates.Add(state);
        await context.SaveChangesAsync();

        // Auto-complete the remaining 3 via counts
        SetupSenderCounts(patients: 1, appointments: 1, invoices: 1);

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Progress.Completed.Should().Be(5);
        result.Value.Progress.Total.Should().Be(5);
        result.Value.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        // Arrange — empty context, no user seeded
        using var context = BuildContext();

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_VetUser_ReturnsVetSpecificSteps()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateVetUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Vet steps do not include patient/appointment/invoice counts
        _sender.Send(Arg.Any<GetPatientCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _sender.Send(Arg.Any<GetAppointmentCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));
        _sender.Send(Arg.Any<GetInvoiceCountQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Steps.Should().HaveCount(4); // Vet has 4 steps
        result.Value.Steps.Select(s => s.StepId).Should().BeEquivalentTo(
            new[] { "view_appointments", "open_patient_record", "add_medical_record", "write_prescription" });

        // Vet steps are not auto-completed by patient/appointment/invoice counts
        result.Value.Steps.Should().OnlyContain(s => !s.IsCompleted);
    }

    [Fact]
    public async Task Handle_ExistingStateDismissed_ReturnsCorrectVisibilityFlags()
    {
        // Arrange
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var stateResult = OnboardingState.Create(user.Id, "Admin", FixedClinicId);
        var state = stateResult.Value;
        state.DismissBanner();
        state.DismissChecklist();
        context.OnboardingStates.Add(state);
        await context.SaveChangesAsync();

        SetupSenderCountsZero();

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WelcomeBannerVisible.Should().BeFalse();
        result.Value.ChecklistVisible.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AutoCompletionAlreadyDone_DoesNotDuplicateStep()
    {
        // Arrange — step already in CompletedSteps, count also > 0
        using var context = BuildContext();
        var user = CreateAdminUser(FixedClinicId);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var stateResult = OnboardingState.Create(user.Id, "Admin", FixedClinicId);
        var state = stateResult.Value;
        state.CompleteStep("add_first_patient"); // already marked
        context.OnboardingStates.Add(state);
        await context.SaveChangesAsync();

        SetupSenderCounts(patients: 5, appointments: 0, invoices: 0);

        var handler = new GetOnboardingStateHandler(context, _sender);
        var query = new GetOnboardingStateQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Steps.Count(s => s.StepId == "add_first_patient").Should().Be(1);
        result.Value.Steps.First(s => s.StepId == "add_first_patient").IsCompleted.Should().BeTrue();
        result.Value.Progress.Completed.Should().Be(1);
    }
}
