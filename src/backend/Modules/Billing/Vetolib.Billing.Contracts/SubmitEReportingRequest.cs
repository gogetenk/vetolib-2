namespace Vetolib.Billing.Contracts;

public record SubmitEReportingRequest(DateOnly PeriodStart, DateOnly PeriodEnd);
