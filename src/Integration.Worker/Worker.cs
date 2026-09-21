using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Integration.Core;
using Integration.Core.Csv;
using Integration.Core.Models;
using Integration.Core.Transform;
using Integration.Data;
using Microsoft.EntityFrameworkCore;

namespace Integration.Worker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessIncomingFilesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error during poll cycle; will retry on the next cycle");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessIncomingFilesAsync(CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(PipelineFolders.Incoming, "*.csv");

        foreach (var filePath in files)
        {
            var fileName = Path.GetFileName(filePath);

            try
            {
                await ProcessFileAsync(filePath, fileName, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process {File}; moving to failed folder (no auto-retry)", fileName);

                try
                {
                    File.Move(filePath, Path.Combine(PipelineFolders.Failed, fileName), overwrite: true);
                }
                catch (Exception moveEx)
                {
                    logger.LogError(moveEx,
                        "Failed to move {File} to the failed folder; it will remain in incoming and be retried next cycle",
                        fileName);
                }
            }
        }
    }

    private async Task ProcessFileAsync(string filePath, string fileName, CancellationToken cancellationToken)
    {
        var invoices = ReadInvoices(filePath);
        logger.LogInformation("Parsed {Count} invoice(s) from {File}", invoices.Count, fileName);

        var xeroInvoices = invoices.Select(ErpInvoiceTransformer.ToXeroInvoice).ToList();

        var (kept, skippedDuplicates) = IntraFileDuplicateResolver.Resolve(xeroInvoices);

        foreach (var duplicate in skippedDuplicates)
        {
            logger.LogWarning(
                "Skipping duplicate row for invoice {InvoiceNumber} within {File} (identical to an earlier row in the same file)",
                duplicate.InvoiceNumber, fileName);
        }

        foreach (var xeroInvoice in kept)
        {
            logger.LogInformation(
                "Transformed -> {InvoiceNumber} | {ContactName} | {Total} {CurrencyCode}",
                xeroInvoice.InvoiceNumber, xeroInvoice.ContactName, xeroInvoice.Total, xeroInvoice.CurrencyCode);
        }

        await SaveToDatabaseAsync(kept, cancellationToken);
        await WriteOutputJsonAsync(kept, fileName, cancellationToken);

        File.Move(filePath, Path.Combine(PipelineFolders.Processed, fileName), overwrite: true);
    }

    private async Task SaveToDatabaseAsync(List<XeroInvoice> xeroInvoices, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();

        var invoiceNumbers = xeroInvoices.Select(x => x.InvoiceNumber).ToList();
        var existingInvoiceNumbers = await dbContext.Invoices
            .Where(x => invoiceNumbers.Contains(x.InvoiceNumber))
            .Select(x => x.InvoiceNumber)
            .ToHashSetAsync(cancellationToken);

        var (newInvoices, duplicates) = InvoiceDeduplicator.Partition(xeroInvoices, existingInvoiceNumbers);

        foreach (var duplicate in duplicates)
        {
            logger.LogWarning(
                "Skipping duplicate invoice {InvoiceNumber} (already saved to MySQL) -- likely a reprocessed file",
                duplicate.InvoiceNumber);
        }

        if (newInvoices.Count > 0)
        {
            dbContext.Invoices.AddRange(newInvoices);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Saved {Count} invoice(s) to MySQL ({DuplicateCount} duplicate(s) skipped)",
            newInvoices.Count, duplicates.Count);
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
