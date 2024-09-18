using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AppsettingsEDU.Lib.Helper
{
    public class AppSettingsHelper
    {
        public const string AspNetVarVarName = "ASPNETCORE_ENVIRONMENT";
        public const string DotNetEnvVarName = "DOTNET_ENVIRONMENT";
        public const string ConnectionStringsSectionName = "ConnectionStrings";
        public const string AppSettingsSectionName = "AppSettings";

        /// <summary>
        /// Switches the optional environment variable name for adding the appsetting.<ENVIRONMENT>.json.
        /// </summary>
        private static string GetEnvVarName()
        {
            return Environment.GetEnvironmentVariable(AspNetVarVarName) ??
                Environment.GetEnvironmentVariable(DotNetEnvVarName) ??
                string.Empty;
        }

        /// <summary>
        /// Gets the value from the AppSettings section by type T.
        /// </summary>
        /// <typeparam name="T">expected type of the setting, cannot be null</typeparam>
        /// <param name="Key">key name of the setting</param>
        /// <returns>typed settings value, or default if not found</returns>
        public static T GetValue<T>(string Key) where T : notnull
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException(nameof(Key));

            Type type = typeof(T).IsValueType ? typeof(T) : typeof(string);

            IConfiguration configuration = GetAppConfigBuilder().Build();
            var result = configuration
                .GetSection(AppSettingsSectionName)
                .GetValue(type, Key);

            if (result == null)
                return default!;

            return (T)result;
        }

        /// <summary>
        /// Gets the connectionString by Key.
        /// </summary>
        /// <param name="Key">Key name of the connectionString</param>
        /// <returns>connectionString</returns>
        public static string GetConnectionString(string Key)
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException(nameof(Key));

            IConfiguration configuration = GetAppConfigBuilder().Build();
            var result = configuration
                .GetSection(ConnectionStringsSectionName)
                .GetValue<string>(Key) ?? string.Empty;

            return result;
        }

        /// <summary>
        /// This methods is usually in the Program.cs or Startup.cs.
        /// If you locate this here you can access the AppSettings from anywhere else as well.
        /// </summary>
        /// <returns>configurationBuilder object</returns>
        public static IConfigurationBuilder GetAppConfigBuilder()
        {
            var envVarName = GetEnvVarName();
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(basePath))
                throw new InvalidOperationException("Base path of the entry assembly could not be determined.");

            var appConfigBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(envVarName)}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            return appConfigBuilder;
        }
    }
}