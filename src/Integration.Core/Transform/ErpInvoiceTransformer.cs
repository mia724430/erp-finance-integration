using System.Globalization;
using Integration.Core.Models;

namespace Integration.Core.Transform;

/// <summary>
/// Maps a raw ErpInvoice to the Xero-format target, applying basic cleaning
/// (whitespace, casing, currency codes) along the way.
/// </summary>
public static class ErpInvoiceTransformer
{
    public static XeroInvoice ToXeroInvoice(ErpInvoice erpInvoice)
    {
        return new XeroInvoice
        {
            InvoiceNumber = erpInvoice.OrderId,
            ContactName = ToTitleCase(erpInvoice.CustomerName),
            Total = Math.Round(erpInvoice.Amount, 2),
            IssueDate = erpInvoice.OrderDate,
            CurrencyCode = erpInvoice.Currency.Trim().ToUpperInvariant()
        };
    }

    private static string ToTitleCase(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized);
    }
}
