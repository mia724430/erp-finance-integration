using CsvHelper.Configuration;
using Integration.Core.Models;

namespace Integration.Core.Csv;

/// <summary>
/// Column layout for ErpInvoice CSV files, shared by ERP.Simulator (writer) and Integration.Worker (reader)
/// so both sides agree on header names, column order, and date/number formats.
/// </summary>
public sealed class ErpInvoiceMap : ClassMap<ErpInvoice>
{
    public ErpInvoiceMap()
    {
        Map(m => m.OrderId).Name("OrderId").Index(0);
        Map(m => m.CustomerName).Name("CustomerName").Index(1);
        Map(m => m.Amount).Name("Amount").Index(2).TypeConverterOption.Format("F2");
        Map(m => m.OrderDate).Name("OrderDate").Index(3).TypeConverterOption.Format("yyyy-MM-dd");
        Map(m => m.Currency).Name("Currency").Index(4);
    }
}
