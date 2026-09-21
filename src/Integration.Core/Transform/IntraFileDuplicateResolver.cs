using Integration.Core.Models;

namespace Integration.Core.Transform;

/// <summary>
/// Resolves duplicate InvoiceNumbers within a single batch (e.g. a batch
/// parsed from one CSV file), before it ever reaches InvoiceDeduplicator or
/// the database. A row that's identical to an earlier row with the same
/// InvoiceNumber is dropped -- kept once, in first-seen order. A row that
/// shares an InvoiceNumber with an earlier row but differs in any other
/// field is a genuine data conflict: this throws rather than silently
/// picking one version, so the caller can fail the whole file.
/// </summary>
public static class IntraFileDuplicateResolver
{
    public static (List<XeroInvoice> Kept, List<XeroInvoice> SkippedDuplicates) Resolve(IEnumerable<XeroInvoice> invoices)
    {
        var kept = new List<XeroInvoice>();
        var skippedDuplicates = new List<XeroInvoice>();
        var firstSeen = new Dictionary<string, XeroInvoice>();

        foreach (var invoice in invoices)
        {
            if (!firstSeen.TryGetValue(invoice.InvoiceNumber, out var original))
            {
                firstSeen[invoice.InvoiceNumber] = invoice;
                kept.Add(invoice);
                continue;
            }

            if (IsSameContent(original, invoice))
            {
                skippedDuplicates.Add(invoice);
            }
            else
            {
                throw new ConflictingInvoiceException(invoice.InvoiceNumber);
            }
        }

        return (kept, skippedDuplicates);
    }

    private static bool IsSameContent(XeroInvoice a, XeroInvoice b) =>
        a.ContactName == b.ContactName &&
        a.Total == b.Total &&
        a.IssueDate == b.IssueDate &&
        a.CurrencyCode == b.CurrencyCode;
}
