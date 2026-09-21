using Integration.Core.Models;
using Integration.Core.Transform;

namespace Integration.Tests;

public class InvoiceDeduplicatorTests
{
    [Fact]
    public void SeparatesNewInvoicesFromDuplicates()
    {
        var invoices = new List<XeroInvoice>
        {
            new() { InvoiceNumber = "ORD-1" },
            new() { InvoiceNumber = "ORD-2" },
            new() { InvoiceNumber = "ORD-3" }
        };
        var existing = new HashSet<string> { "ORD-2" };

        var (newInvoices, duplicates) = InvoiceDeduplicator.Partition(invoices, existing);

        Assert.Equal(["ORD-1", "ORD-3"], newInvoices.Select(x => x.InvoiceNumber));
        Assert.Equal(["ORD-2"], duplicates.Select(x => x.InvoiceNumber));
    }

    [Fact]
    public void ReturnsAllAsNewWhenNoneExistYet()
    {
        var invoices = new List<XeroInvoice> { new() { InvoiceNumber = "ORD-1" } };

        var (newInvoices, duplicates) = InvoiceDeduplicator.Partition(invoices, new HashSet<string>());

        Assert.Single(newInvoices);
        Assert.Empty(duplicates);
    }

    [Fact]
    public void ReturnsAllAsDuplicatesWhenAllAlreadyExist()
    {
        var invoices = new List<XeroInvoice> { new() { InvoiceNumber = "ORD-1" } };
        var existing = new HashSet<string> { "ORD-1" };

        var (newInvoices, duplicates) = InvoiceDeduplicator.Partition(invoices, existing);

        Assert.Empty(newInvoices);
        Assert.Single(duplicates);
    }
}
