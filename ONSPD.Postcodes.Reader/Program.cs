using System;
using System.IO;
using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ONSPD.Postcodes.Reader;
using ONSPD.Postcodes.Reader.Data;
using ONSPD.Postcodes.Reader.Services;

await Host.CreateDefaultBuilder(args)
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .ConfigureLogging(_ =>
    {
        // Add any 3rd party loggers - replace _ above with "logging" parameter
    })
    .ConfigureServices((hostContext, services) =>
    {
        var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

        UpgradeDatabase(connectionString);

        services
            .AddHostedService<ConsoleHostedService>()
            .AddScoped<IDataRepository>(s => new DataRepository(connectionString))
            .AddScoped<IPostcodeReaderService, PostcodeReaderService>();
    })
    .RunConsoleAsync();

void UpgradeDatabase(string connectionString)
{
    EnsureDatabase.For.SqlDatabase(connectionString);

    var upgrader = DeployChanges.To
        .SqlDatabase(connectionString, null)
        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .WithTransaction()
        .LogToConsole()
        .Build();

    if (upgrader.IsUpgradeRequired())
    {
        upgrader.PerformUpgrade();
    }
}