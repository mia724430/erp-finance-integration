namespace Integration.Core.Models;

/// <summary>
/// Raw invoice/order record as exported by the (simulated) ERP system.
/// </summary>
public class ErpInvoice
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Currency { get; set; } = "NZD";
}
