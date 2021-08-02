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
        .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        configurationBuilder
            .SetBasePath(context.HostingEnvironment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
    })
    .ConfigureLogging(_ =>
    {
        // Add any 3rd party loggers - replace _ above with "logging" parameter
    })
    .ConfigureServices((hostContext, services) =>
    {
        var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
        var x = hostContext.Configuration.GetValue<string>("PostcodesFilePath");

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
        .WithExecutionTimeout(TimeSpan.FromMinutes(2))
        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .WithTransaction()
        .LogToConsole()
        .Build();

    if (upgrader.IsUpgradeRequired())
    {
        upgrader.PerformUpgrade();
    }
}