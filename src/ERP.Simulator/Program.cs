using CsvHelper;
using ERP.Simulator;
using Integration.Core;
using Integration.Core.Csv;
using System.Globalization;

var random = new Random();
var batchSize = random.Next(5, 16);
var invoices = ErpInvoiceGenerator.Generate(batchSize, random);

var fileName = $"erp-export-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
var filePath = Path.Combine(PipelineFolders.Incoming, fileName);

using (var writer = new StreamWriter(filePath))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    csv.Context.RegisterClassMap<ErpInvoiceMap>();
    csv.WriteRecords(invoices);
}

Console.WriteLine($"Generated {invoices.Count} invoice(s) -> {filePath}");
