using AppsettingsEDU.Lib;
using AppsettingsEDU.Lib.Helper;
using AppsettingsEDU.Objects.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AppsettingsEDU.Services
{
    public class SomeService : ISomeService
    {
        private readonly IConfiguration _config;
        private readonly SomeSettingsModel _someSettingsModel;
        private readonly ILogger<SomeService> _logger;

        public SomeService(IConfiguration config, IOptions<SomeSettingsModel> someSettingsModel, ILogger<SomeService> logger)
        {
            _config = config;
            _someSettingsModel = someSettingsModel.Value;
            _logger = logger ?? NullLogger<SomeService>.Instance;
        }

        public async Task Run()
        {
            _logger.LogInformation("The current environment is {DOTNET_ENVIRONMENT}", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production");
            await Console.Out.WriteLineAsync();

            _logger.LogInformation("This Service accesses the appsettings and prints out the values.");

            AccessSomeSettings();
            AccessTypedSettingsObjects();
            AccessConnectionStrings();
            AccessSettingsWithHelperClass();
            AccessStaticDataClassWithValuesFromConfig();
        }

        private void AccessSomeSettings()
        {
            _logger.LogInformation("Access the values in top level ...");

            string? mySetting = _config.GetValue<string>("AppSettings:MySetting");
            string? subSetting = _config.GetValue<string>("AppSettings:MainSetting:SubSetting");

            _logger.LogInformation("Value from 'MySetting': {mySetting}", mySetting);
            _logger.LogInformation("Value from 'SubSetting' in 'MainSetting': {subSetting}", subSetting);

            _logger.LogInformation("################################");
        }

        private void AccessTypedSettingsObjects()
        {
            _logger.LogInformation("Access the values from strongly typed settings ...");

            string? modelSetting = _someSettingsModel.ModelSetting;
            SomeSubSettingsModel? someSubSettingsModel = _someSettingsModel.SomeSubSettingsModel;

            _logger.LogInformation("Value from the model string: {modelSetting}", modelSetting);
            _logger.LogInformation("Values from the settings model subsettings: {settingsString}, {settingsInt}",
                someSubSettingsModel.MyModelSubSettingString, someSubSettingsModel.MyModelSubSettingInt);

            _logger.LogInformation("################################");
        }

        private void AccessConnectionStrings()
        {
            _logger.LogInformation("Access the connection strings ...");

            string? connectionString = _config.GetConnectionString("Default");

            _logger.LogInformation("Value from Default ConnectionString: {connectionString}", connectionString);

            _logger.LogInformation("################################");
        }

        private void AccessSettingsWithHelperClass()
        {
            _logger.LogInformation("Access the settings with a helper class from a library ...");

            string? stringResult = AppSettingsHelper.GetValue<string>("MySetting");
            _logger.LogInformation("String Value from settings: {result}", stringResult);

            int intResult = AppSettingsHelper.GetValue<int>("SomeInt");
            _logger.LogInformation("String Value from settings: {result}", intResult);

            bool boolResult = AppSettingsHelper.GetValue<bool>("SomeBool");
            _logger.LogInformation("String Value from settings: {result}", boolResult);

            decimal decimalResult = AppSettingsHelper.GetValue<decimal>("Somedecimal");
            _logger.LogInformation("String Value from settings: {result}", decimalResult);

            _logger.LogInformation("################################");
        }

        public void AccessStaticDataClassWithValuesFromConfig() // wonderful naming, huh?
        {
            _logger.LogInformation("Access some static data holding class with internal call of the helper class from a library ...");

            _logger.LogInformation("String Value from static class is : {SomeConfigString}", StaticValues.SomeConfigString);

            _logger.LogInformation("################################");
        }
    }
}