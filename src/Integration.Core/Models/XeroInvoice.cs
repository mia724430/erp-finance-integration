namespace Integration.Core.Models;

/// <summary>
/// Target format matching Xero's invoice fields, produced after transforming an ErpInvoice.
/// </summary>
public class XeroInvoice
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime IssueDate { get; set; }
    public string CurrencyCode { get; set; } = "NZD";
}
