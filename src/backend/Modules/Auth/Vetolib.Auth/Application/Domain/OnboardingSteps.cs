namespace Vetolib.Auth.Application.Domain;

internal static class OnboardingSteps
{
    // Admin — 5 steps
    public const string InviteTeamMember = "invite_team_member";
    public const string AddFirstPatient = "add_first_patient";
    public const string BookFirstAppointment = "book_first_appointment";
    public const string CreateFirstInvoice = "create_first_invoice";
    public const string ExploreDashboard = "explore_dashboard";

    // Vet — 4 steps
    public const string ViewAppointments = "view_appointments";
    public const string OpenPatientRecord = "open_patient_record";
    public const string AddMedicalRecord = "add_medical_record";
    public const string WritePrescription = "write_prescription";

    // Receptionist — 4 steps
    public const string BookAppointment = "book_appointment";
    public const string CheckInPatient = "check_in_patient";
    public const string CreateInvoice = "create_invoice";
    public const string SendInvoice = "send_invoice";

    // Assistant — 3 steps
    public const string BrowsePatients = "browse_patients";
    public const string ViewMedicalRecord = "view_medical_record";
    public const string CheckTodaySchedule = "check_today_schedule";

    public static IReadOnlyList<string> GetStepsForRole(string role)
    {
        return role.ToUpperInvariant() switch
        {
            "ADMIN" => [InviteTeamMember, AddFirstPatient, BookFirstAppointment, CreateFirstInvoice, ExploreDashboard],
            "VET" => [ViewAppointments, OpenPatientRecord, AddMedicalRecord, WritePrescription],
            "RECEPTIONIST" => [BookAppointment, CheckInPatient, CreateInvoice, SendInvoice],
            "ASSISTANT" => [BrowsePatients, ViewMedicalRecord, CheckTodaySchedule],
            _ => [InviteTeamMember, AddFirstPatient, BookFirstAppointment, CreateFirstInvoice, ExploreDashboard]
        };
    }

    public static string GetLabelForStep(string stepId) => stepId switch
    {
        InviteTeamMember => "Invite a team member",
        AddFirstPatient => "Add your first patient",
        BookFirstAppointment => "Book your first appointment",
        CreateFirstInvoice => "Create your first invoice",
        ExploreDashboard => "Explore the dashboard",
        ViewAppointments => "View your appointments",
        OpenPatientRecord => "Open a patient record",
        AddMedicalRecord => "Add a medical record",
        WritePrescription => "Write a prescription",
        BookAppointment => "Book an appointment",
        CheckInPatient => "Check in a patient",
        CreateInvoice => "Create an invoice",
        SendInvoice => "Send an invoice",
        BrowsePatients => "Browse patients",
        ViewMedicalRecord => "View a medical record",
        CheckTodaySchedule => "Check today's schedule",
        _ => stepId
    };

    public static string GetLinkForStep(string stepId) => stepId switch
    {
        InviteTeamMember => "/settings/team",
        AddFirstPatient => "/patients/new",
        BookFirstAppointment => "/appointments/new",
        CreateFirstInvoice => "/billing/new",
        ExploreDashboard => "/dashboard",
        ViewAppointments => "/appointments",
        OpenPatientRecord => "/patients",
        AddMedicalRecord => "/patients",
        WritePrescription => "/patients",
        BookAppointment => "/appointments/new",
        CheckInPatient => "/appointments",
        CreateInvoice => "/billing/new",
        SendInvoice => "/billing",
        BrowsePatients => "/patients",
        ViewMedicalRecord => "/patients",
        CheckTodaySchedule => "/appointments",
        _ => "/dashboard"
    };
}
