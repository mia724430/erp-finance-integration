using ERP.Simulator;

namespace Integration.Tests;

public class ErpInvoiceGeneratorTests
{
    [Fact]
    public void GeneratesOneInvoicePerOrderNumberInOrder()
    {
        var orderNumbers = new List<int> { 100001, 100002, 100003 };

        var invoices = ErpInvoiceGenerator.Generate(orderNumbers, new Random(42));

        Assert.Equal(["ORD-100001", "ORD-100002", "ORD-100003"], invoices.Select(x => x.OrderId));
    }

    [Fact]
    public void GeneratedOrderIdsAreUniqueWhenInputOrderNumbersAreUnique()
    {
        var orderNumbers = Enumerable.Range(100001, 50).ToList();

        var invoices = ErpInvoiceGenerator.Generate(orderNumbers, new Random());

        var distinctOrderIds = invoices.Select(x => x.OrderId).Distinct().Count();
        Assert.Equal(orderNumbers.Count, distinctOrderIds);
    }

    [Fact]
    public void ReturnsNoInvoicesForAnEmptyOrderNumberList()
    {
        var invoices = ErpInvoiceGenerator.Generate([], new Random());

        Assert.Empty(invoices);
    }
}
