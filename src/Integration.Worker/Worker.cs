using System.Globalization;
using CsvHelper;
using Integration.Core;
using Integration.Core.Csv;
using Integration.Core.Models;
using Integration.Core.Transform;

namespace Integration.Worker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ProcessIncomingFiles();
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private void ProcessIncomingFiles()
    {
        var files = Directory.GetFiles(PipelineFolders.Incoming, "*.csv");

        foreach (var filePath in files)
        {
            var invoices = ReadInvoices(filePath);
            var fileName = Path.GetFileName(filePath);

            logger.LogInformation("Parsed {Count} invoice(s) from {File}", invoices.Count, fileName);

            foreach (var invoice in invoices)
            {
                var xeroInvoice = ErpInvoiceTransformer.ToXeroInvoice(invoice);
                logger.LogInformation(
                    "Transformed {OrderId} -> {InvoiceNumber} | {ContactName} | {Total} {CurrencyCode}",
                    invoice.OrderId, xeroInvoice.InvoiceNumber, xeroInvoice.ContactName, xeroInvoice.Total, xeroInvoice.CurrencyCode);
            }

            File.Move(filePath, Path.Combine(PipelineFolders.Processed, fileName), overwrite: true);
        }
    }

    private static List<ErpInvoice> ReadInvoices(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<ErpInvoiceMap>();
        return csv.GetRecords<ErpInvoice>().ToList();
    }
}
