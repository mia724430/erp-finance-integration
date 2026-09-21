using Integration.Data;
using Integration.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("Missing 'ConnectionStrings:MySql' configuration.");

builder.Services.AddDbContext<IntegrationDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IntegrationDbContext>().Database.Migrate();
}

host.Run();
