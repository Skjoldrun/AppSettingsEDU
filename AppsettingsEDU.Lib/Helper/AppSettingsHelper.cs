using Microsoft.Extensions.Configuration;

namespace AppsettingsEDU.Lib.Helper
{
    public class AppSettingsHelper
    {
        private static AppSettingsHelper? _appSettings;

        public object? AppSettingValue { private get; set; }

        public AppSettingsHelper(IConfiguration config, Type T, string Key)
        {
            AppSettingValue = config.GetValue(T, Key);
        }

        private static AppSettingsHelper GetSettings(Type T, string Key)
        {
            IConfiguration configuration = InitAppConfigBuilder().Build();
            var settings = new AppSettingsHelper(configuration.GetSection("AppSettings"), T, Key);

            return settings;
        }

        /// <summary>
        /// Gets the value from the AppSettings section by type T.
        /// </summary>
        /// <typeparam name="T">expected type of the setting</typeparam>
        /// <param name="Key">key name of the setting</param>
        /// <returns>typed settings value</returns>
        public static T GetValue<T>(string Key)
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException(nameof(Key));

            Type type;
            if (default(T) == null)
                type = typeof(string);
            else
                type = default(T)!.GetType();

            _appSettings = GetSettings(type, Key);
            return (T)_appSettings.AppSettingValue;
        }

        /// <summary>
        /// This methods is usually in the Program.cs or Startup.cs.
        /// If you locate this here you can access the AppSettings from anywhere else as well.
        /// </summary>
        /// <returns>The built appsettings object</returns>
        public static IConfigurationBuilder InitAppConfigBuilder()
        {
            var appConfigBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // this path works if started from PATH env var
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            return appConfigBuilder;
        }
    }
}