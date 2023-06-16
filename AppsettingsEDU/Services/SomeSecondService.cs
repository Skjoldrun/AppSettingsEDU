using AppsettingsEDU.Lib.Helper;
using AppsettingsEDU.Objects.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppsettingsEDU.Services
{
    public class SomeSecondService : ISomeSecondService
    {
        private readonly IConfiguration _config;
        private readonly SomeSettingsModel _someSettingsModel;
        private readonly ILogger<SomeService> _logger;

        public SomeSecondService(IConfiguration config, SomeSettingsModel someSettingsModel, ILogger<SomeService> logger = null)
        {
            _config = config;
            _someSettingsModel = someSettingsModel; // alternative way to get this settings as typed object without IOptions
            _logger = logger ?? NullLogger<SomeService>.Instance;
        }

        public async Task Run()
        {
            AccessTypedModelSettings();
            AccessConnectionStrings();
        }

        private void AccessTypedModelSettings()
        {
            _logger.LogInformation("This is the second service demonstrating injection of typed configuration objects insted of IOptions...");

            string? modelSetting = _someSettingsModel.ModelSetting;
            SomeSubSettingsModel? someSubSettingsModel = _someSettingsModel.SomeSubSettingsModel;

            _logger.LogInformation("Value from the model string: {modelSetting}", modelSetting);
            _logger.LogInformation("Values from the settings model subsettings: {settingsString}, {settingsInt}",
                someSubSettingsModel.MyModelSubSettingString, someSubSettingsModel.MyModelSubSettingInt);

            _logger.LogInformation("################################");
        }

        private void AccessConnectionStrings()
        {
            string connString = AppSettingsHelper.GetConnectionString("Default");
            _logger.LogInformation("Value from the default connectionString: {connString}", connString);

            _logger.LogInformation("################################");
        }
    }
}