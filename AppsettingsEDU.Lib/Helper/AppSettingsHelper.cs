using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AppsettingsEDU.Lib.Helper
{
    public class AppSettingsHelper
    {
        public const string AspNetVarVarName = "ASPNETCORE_ENVIRONMENT";
        public const string DotNetEnvVarName = "DOTNET_ENVIRONMENT";

        private static AppSettingsHelper _appSettings;

        public object AppSettingValue { private get; set; }

        public AppSettingsHelper(IConfiguration config, Type T, string Key)
        {
            AppSettingValue = config.GetValue(T, Key);
        }

        private static AppSettingsHelper GetSettings(Type T, string Key)
        {
            IConfiguration configuration = GetAppConfigBuilder().Build();
            var settings = new AppSettingsHelper(configuration.GetSection("AppSettings"), T, Key);

            return settings;
        }

        /// <summary>
        /// Switches the optional environment variable name for adding the appsetting.<ENVIRONMENT>.json.
        /// </summary>
        private static string GetEnvVarName()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AspNetVarVarName)))
                return AspNetVarVarName;
            else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(DotNetEnvVarName)))
                return DotNetEnvVarName;
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets the value from the 'AppSettings' section by type T.
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
                type = default(T).GetType();

            _appSettings = GetSettings(type, Key);
            return (T)_appSettings.AppSettingValue;
        }

        /// <summary>
        /// This methods is usually in the Program.cs or Startup.cs.
        /// If you locate this here you can access the AppSettings from anywhere else as well.
        /// </summary>
        /// <returns>configurationBuilder object</returns>
        public static IConfigurationBuilder GetAppConfigBuilder()
        {
            var envVarName = GetEnvVarName();
            var appConfigBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(envVarName)}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return appConfigBuilder;
        }
    }
}