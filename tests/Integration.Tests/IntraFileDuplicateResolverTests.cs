using Integration.Core.Models;
using Integration.Core.Transform;

namespace Integration.Tests;

public class IntraFileDuplicateResolverTests
{
    private static XeroInvoice MakeInvoice(string invoiceNumber = "ORD-1", string contactName = "Kiwi Freight Ltd",
        decimal total = 100.00m, string currencyCode = "NZD") => new()
    {
        InvoiceNumber = invoiceNumber,
        ContactName = contactName,
        Total = total,
        IssueDate = new DateTime(2026, 1, 1),
        CurrencyCode = currencyCode
    };

    [Fact]
    public void KeepsAllInvoicesWhenNoDuplicatesExist()
    {
        var invoices = new List<XeroInvoice> { MakeInvoice("ORD-1"), MakeInvoice("ORD-2") };

        var (kept, skipped) = IntraFileDuplicateResolver.Resolve(invoices);

        Assert.Equal(["ORD-1", "ORD-2"], kept.Select(x => x.InvoiceNumber));
        Assert.Empty(skipped);
    }

    [Fact]
    public void KeepsFirstAndSkipsIdenticalDuplicates()
    {
        var first = MakeInvoice("ORD-1");
        var identicalDuplicate = MakeInvoice("ORD-1");
        var invoices = new List<XeroInvoice> { first, identicalDuplicate };

        var (kept, skipped) = IntraFileDuplicateResolver.Resolve(invoices);

        Assert.Single(kept);
        Assert.Same(first, kept[0]);
        Assert.Single(skipped);
        Assert.Same(identicalDuplicate, skipped[0]);
    }

    [Fact]
    public void SkipsMultipleIdenticalDuplicatesOfTheSameInvoiceNumber()
    {
        var invoices = new List<XeroInvoice>
        {
            MakeInvoice("ORD-1"), MakeInvoice("ORD-1"), MakeInvoice("ORD-1")
        };

        var (kept, skipped) = IntraFileDuplicateResolver.Resolve(invoices);

        Assert.Single(kept);
        Assert.Equal(2, skipped.Count);
    }

    [Theory]
    [InlineData("Different Co", 100.00, "NZD")]
    [InlineData("Kiwi Freight Ltd", 999.99, "NZD")]
    [InlineData("Kiwi Freight Ltd", 100.00, "AUD")]
    public void ThrowsWhenSameInvoiceNumberHasDifferentContent(string contactName, decimal total, string currencyCode)
    {
        var invoices = new List<XeroInvoice>
        {
            MakeInvoice("ORD-1"),
            MakeInvoice("ORD-1", contactName: contactName, total: total, currencyCode: currencyCode)
        };

        var ex = Assert.Throws<ConflictingInvoiceException>(() => IntraFileDuplicateResolver.Resolve(invoices));

        Assert.Equal("ORD-1", ex.InvoiceNumber);
        Assert.Contains("ORD-1", ex.Message);
    }

    [Fact]
    public void ThrowsOnConflictEvenAfterEarlierIdenticalDuplicatesOfTheSameNumber()
    {
        var invoices = new List<XeroInvoice>
        {
            MakeInvoice("ORD-1"),
            MakeInvoice("ORD-1"),
            MakeInvoice("ORD-1", contactName: "Different Co")
        };

        Assert.Throws<ConflictingInvoiceException>(() => IntraFileDuplicateResolver.Resolve(invoices));
    }
}
