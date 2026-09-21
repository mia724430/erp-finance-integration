using Integration.Core.Models;

namespace Integration.Core.Transform;

/// <summary>
/// Splits a batch of transformed invoices into new ones to insert and
/// duplicates to skip, given the invoice numbers already present in the
/// database. Kept separate from the actual database call so this logic is
/// unit-testable without a live database.
/// </summary>
public static class InvoiceDeduplicator
{
    public static (List<XeroInvoice> New, List<XeroInvoice> Duplicates) Partition(
        IEnumerable<XeroInvoice> invoices, IReadOnlySet<string> existingInvoiceNumbers)
    {
        var newInvoices = new List<XeroInvoice>();
        var duplicates = new List<XeroInvoice>();

        foreach (var invoice in invoices)
        {
            if (existingInvoiceNumbers.Contains(invoice.InvoiceNumber))
            {
                duplicates.Add(invoice);
            }
            else
            {
                newInvoices.Add(invoice);
            }
        }

        return (newInvoices, duplicates);
    }
}
