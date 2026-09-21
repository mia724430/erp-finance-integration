using Integration.Core.Models;

namespace ERP.Simulator;

/// <summary>
/// Produces fake ERP invoice/order records. Deliberately a little messy (stray
/// whitespace, inconsistent currency casing) to mirror real ERP exports and give
/// the Day 4 transform/cleaning step something real to do.
/// </summary>
public static class ErpInvoiceGenerator
{
    private static readonly string[] CustomerNames =
    [
        "Kiwi Freight Ltd", "Northbridge Supplies", " Alpine Traders ", "harbourview logistics",
        "Southern Cross Retail", "Pacific Rim Distribution", "Everest Wholesale Co",
        "Tui Manufacturing", "  Coastal Imports", "Red Rock Trading"
    ];

    private static readonly string[] CurrencyCodes = ["NZD", "NZD", "NZD", "AUD", "nzd"];

    public static List<ErpInvoice> Generate(IReadOnlyList<int> orderNumbers, Random random)
    {
        var invoices = new List<ErpInvoice>(orderNumbers.Count);

        foreach (var orderNumber in orderNumbers)
        {
            invoices.Add(new ErpInvoice
            {
                OrderId = $"ORD-{orderNumber}",
                CustomerName = CustomerNames[random.Next(CustomerNames.Length)],
                Amount = Math.Round((decimal)(random.NextDouble() * 4950 + 50), 2),
                OrderDate = DateTime.Today.AddDays(-random.Next(0, 14)),
                Currency = CurrencyCodes[random.Next(CurrencyCodes.Length)]
            });
        }

        return invoices;
    }
}
