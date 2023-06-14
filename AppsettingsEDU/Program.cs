using AppsettingsEDU.Lib.Helper;
using AppsettingsEDU.Logging;
using AppsettingsEDU.Objects.Settings;
using AppsettingsEDU.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AppsettingsEDU
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var appConfig = AppSettingsHelper.GetAppConfigBuilder().Build();
            Log.Logger = LogInitializer.CreateLogger(appConfig);

            Log.Information("{AssemblyName} start", ThisAssembly.AssemblyName);

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<ISomeService, SomeService>();
                    services.AddTransient<ISomeSecondService, SomeSecondService>();

                    // Access the settings and get a typed object from the json values
                    services.Configure<SomeSettingsModel>(appConfig.GetSection("AppSettings:SomeSettingsModel"));

                    // Alternative way to inject the direct class object instead of IOptions
                    services.AddSingleton(appConfig.GetSection("AppSettings:SomeSettingsModel").Get<SomeSettingsModel>());
                })
                .UseSerilog()
                .Build();

            var service = ActivatorUtilities.GetServiceOrCreateInstance<SomeService>(host.Services);
            var secondService = ActivatorUtilities.GetServiceOrCreateInstance<SomeSecondService>(host.Services);

            try
            {
                await service.Run();
                await secondService.Run();

                await Console.Out.WriteLineAsync("Press any key to exit ...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception: {ex.Message}");
            }

            Log.Information("{AssemblyName} stop", ThisAssembly.AssemblyName);
            Log.CloseAndFlush();
        }
    }
}