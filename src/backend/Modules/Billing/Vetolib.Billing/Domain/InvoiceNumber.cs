using Ardalis.Result;

namespace Vetolib.Billing.Domain;

internal sealed class InvoiceNumber
{
    public string Value { get; }

    private InvoiceNumber(string value) => Value = value;

    public static Result<InvoiceNumber> Create(int year, int sequenceNumber)
    {
        if (year < 2000)
            return Result<InvoiceNumber>.Invalid(
                new ValidationError(nameof(year), "Year must be >= 2000"));

        if (sequenceNumber < 1)
            return Result<InvoiceNumber>.Invalid(
                new ValidationError(nameof(sequenceNumber), "Sequence number must be >= 1"));

        var value = $"INV-{year}-{sequenceNumber:D3}";
        return Result<InvoiceNumber>.Success(new InvoiceNumber(value));
    }

    public static InvoiceNumber FromString(string value) => new(value);

    public override string ToString() => Value;
}
