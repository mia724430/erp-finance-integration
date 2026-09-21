using Integration.Core.Models;
using Integration.Core.Transform;

namespace Integration.Tests;

public class ErpInvoiceTransformerTests
{
    [Fact]
    public void MapsFieldsToCorrectXeroFields()
    {
        var erpInvoice = new ErpInvoice
        {
            OrderId = "ORD-12345",
            CustomerName = "Northbridge Supplies",
            Amount = 1234.50m,
            OrderDate = new DateTime(2026, 9, 21),
            Currency = "NZD"
        };

        var result = ErpInvoiceTransformer.ToXeroInvoice(erpInvoice);

        Assert.Equal("ORD-12345", result.InvoiceNumber);
        Assert.Equal("Northbridge Supplies", result.ContactName);
        Assert.Equal(1234.50m, result.Total);
        Assert.Equal(new DateTime(2026, 9, 21), result.IssueDate);
        Assert.Equal("NZD", result.CurrencyCode);
    }

    [Theory]
    [InlineData(" Alpine Traders ", "Alpine Traders")]
    [InlineData("  Coastal Imports", "Coastal Imports")]
    [InlineData("harbourview logistics", "Harbourview Logistics")]
    [InlineData("RED ROCK TRADING", "Red Rock Trading")]
    public void NormalizesCustomerNameWhitespaceAndCasing(string rawName, string expected)
    {
        var erpInvoice = new ErpInvoice { CustomerName = rawName };

        var result = ErpInvoiceTransformer.ToXeroInvoice(erpInvoice);

        Assert.Equal(expected, result.ContactName);
    }

    [Theory]
    [InlineData("nzd", "NZD")]
    [InlineData("Aud", "AUD")]
    [InlineData(" nzd ", "NZD")]
    public void NormalizesCurrencyToUppercase(string rawCurrency, string expected)
    {
        var erpInvoice = new ErpInvoice { Currency = rawCurrency };

        var result = ErpInvoiceTransformer.ToXeroInvoice(erpInvoice);

        Assert.Equal(expected, result.CurrencyCode);
    }

    [Fact]
    public void RoundsAmountToTwoDecimalPlaces()
    {
        var erpInvoice = new ErpInvoice { Amount = 12.996m };

        var result = ErpInvoiceTransformer.ToXeroInvoice(erpInvoice);

        Assert.Equal(13.00m, result.Total);
    }

    [Fact]
    public void AlreadyCleanRecordIsUnchanged()
    {
        var erpInvoice = new ErpInvoice
        {
            OrderId = "ORD-99999",
            CustomerName = "Tui Manufacturing",
            Amount = 500.00m,
            OrderDate = new DateTime(2026, 1, 1),
            Currency = "NZD"
        };

        var result = ErpInvoiceTransformer.ToXeroInvoice(erpInvoice);

        Assert.Equal(erpInvoice.OrderId, result.InvoiceNumber);
        Assert.Equal(erpInvoice.CustomerName, result.ContactName);
        Assert.Equal(erpInvoice.Amount, result.Total);
        Assert.Equal(erpInvoice.OrderDate, result.IssueDate);
        Assert.Equal(erpInvoice.Currency, result.CurrencyCode);
    }
}
