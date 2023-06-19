using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AppsettingsEDU.Lib.Helper
{
    public class AppSettingsHelper
    {
        public const string AspNetVarVarName = "ASPNETCORE_ENVIRONMENT";
        public const string DotNetEnvVarName = "DOTNET_ENVIRONMENT";

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

            IConfiguration configuration = GetAppConfigBuilder().Build();
            var result = configuration
                .GetSection("AppSettings")
                .GetValue(type, Key);

            return (T)result;
        }

        /// <summary>
        /// Gets the connectionString by Key.
        /// </summary>
        /// <param name="Key">Key of the connectionString</param>
        /// <returns>connectionString</returns>
        public static string GetConnectionString(string Key)
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentNullException(nameof(Key));

            IConfiguration configuration = GetAppConfigBuilder().Build();
            var result = configuration
                .GetSection("ConnectionStrings")
                .GetValue(typeof(string), Key)
                .ToString();

            return result ?? string.Empty;
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