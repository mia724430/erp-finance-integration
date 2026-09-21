using Integration.Core;
using Integration.Data;
using Integration.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(PipelineFolders.Logs, "worker-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7));

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
