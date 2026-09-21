namespace Integration.Core.Transform;

/// <summary>
/// Thrown when the same InvoiceNumber appears more than once within a single
/// batch (e.g. one CSV file) with different content -- ambiguous data that
/// shouldn't be silently resolved by picking one version over the other.
/// </summary>
public sealed class ConflictingInvoiceException(string invoiceNumber)
    : Exception($"Invoice {invoiceNumber} appears more than once in this file with different content " +
                "(amount/customer/date/currency); refusing to guess which one is correct.")
{
    public string InvoiceNumber { get; } = invoiceNumber;
}
