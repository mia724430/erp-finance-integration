using Integration.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Integration.Data;

public class IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : DbContext(options)
{
    public DbSet<XeroInvoice> Invoices => Set<XeroInvoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationDbContext).Assembly);
    }
}
