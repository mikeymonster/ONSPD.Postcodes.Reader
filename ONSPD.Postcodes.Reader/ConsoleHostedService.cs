using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ONSPD.Postcodes.Reader.Model.Enums;
using ONSPD.Postcodes.Reader.Services;

namespace ONSPD.Postcodes.Reader
{
    internal sealed class ConsoleHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IPostcodeService _postcodeService;

        private int? _exitCode;

        public ConsoleHostedService(
            ILogger<ConsoleHostedService> logger,
            IHostApplicationLifetime appLifetime,
            IPostcodeService postcodeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            _postcodeService = postcodeService ?? throw new ArgumentNullException(nameof(postcodeService));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var args = Environment.GetCommandLineArgs();
            _logger.LogDebug($"Starting with arguments: {string.Join(" ", args[1..])}");

            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {                        
                        if (args.Contains("--loadPostcodes"))
                        {
                            Console.WriteLine("Loading postcodes...");
                            var count = await _postcodeService.LoadPostcodes();
                            Console.WriteLine($"{count} postcodes found");
                        }

                        if (args.Contains("--searchDistance"))
                        {
                            var searchResults = await _postcodeService.Search("CV1 2WT", "OX%");
                            var searchResults2 = await _postcodeService.Search("CV1 2WT", "OX%", SearchMethod.Haversine);
                        }

                        _exitCode = 0;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception!");
                        _exitCode = 1;
                    }
                    finally
                    {
                        // Stop the application once the work is done
                        _appLifetime.StopApplication();
                    }
                }, cancellationToken);
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Exiting with return code: {_exitCode}");
            // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
            return Task.CompletedTask;
        }
    }
}
