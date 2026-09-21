using CsvHelper;
using ERP.Simulator;
using Integration.Core;
using Integration.Core.Csv;
using Serilog;
using System.Globalization;

using var log = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(PipelineFolders.Logs, "simulator-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

var random = new Random();
var batchSize = random.Next(5, 16);
var orderNumbers = OrderSequence.Reserve(batchSize);
var invoices = ErpInvoiceGenerator.Generate(orderNumbers, random);

var fileName = $"erp-export-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
var filePath = Path.Combine(PipelineFolders.Incoming, fileName);

using (var writer = new StreamWriter(filePath))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    csv.Context.RegisterClassMap<ErpInvoiceMap>();
    csv.WriteRecords(invoices);
}

log.Information("Generated {Count} invoice(s) -> {FilePath}", invoices.Count, filePath);
