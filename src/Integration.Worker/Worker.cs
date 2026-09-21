using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Integration.Core;
using Integration.Core.Csv;
using Integration.Core.Models;
using Integration.Core.Transform;
using Integration.Data;

namespace Integration.Worker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessIncomingFilesAsync(stoppingToken);
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessIncomingFilesAsync(CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(PipelineFolders.Incoming, "*.csv");

        foreach (var filePath in files)
        {
            var invoices = ReadInvoices(filePath);
            var fileName = Path.GetFileName(filePath);

            logger.LogInformation("Parsed {Count} invoice(s) from {File}", invoices.Count, fileName);

            var xeroInvoices = invoices.Select(ErpInvoiceTransformer.ToXeroInvoice).ToList();

            foreach (var xeroInvoice in xeroInvoices)
            {
                logger.LogInformation(
                    "Transformed -> {InvoiceNumber} | {ContactName} | {Total} {CurrencyCode}",
                    xeroInvoice.InvoiceNumber, xeroInvoice.ContactName, xeroInvoice.Total, xeroInvoice.CurrencyCode);
            }

            await SaveToDatabaseAsync(xeroInvoices, cancellationToken);
            await WriteOutputJsonAsync(xeroInvoices, fileName, cancellationToken);

            File.Move(filePath, Path.Combine(PipelineFolders.Processed, fileName), overwrite: true);
        }
    }

    private async Task SaveToDatabaseAsync(List<XeroInvoice> xeroInvoices, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();

        dbContext.Invoices.AddRange(xeroInvoices);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Saved {Count} invoice(s) to MySQL", xeroInvoices.Count);
    }

    private static async Task WriteOutputJsonAsync(List<XeroInvoice> xeroInvoices, string sourceFileName, CancellationToken cancellationToken)
    {
        var outputFileName = Path.ChangeExtension(sourceFileName, ".json");
        var outputPath = Path.Combine(PipelineFolders.Output, outputFileName);

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, xeroInvoices, JsonOptions, cancellationToken);
    }

    private static List<ErpInvoice> ReadInvoices(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<ErpInvoiceMap>();
        return csv.GetRecords<ErpInvoice>().ToList();
    }
}
