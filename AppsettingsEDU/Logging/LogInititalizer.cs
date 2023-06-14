using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AppsettingsEDU.Logging
{
    public static class LogInitializer
    {
        /// <summary>
        /// Creates the logger with settings from appconfig and enrichments from code.
        /// </summary>
        /// <param name="appConfig">appConfig built from appsettings.json</param>
        /// <returns>Logger with inline and app.config settings</returns>
        public static Serilog.ILogger CreateLogger(IConfiguration appConfig)
        {
            return new LoggerConfiguration()
                .ReadFrom.Configuration(appConfig)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        /// <summary>
        /// Creates a logger of type T for injecting it manually into a constructor.
        /// </summary>
        /// <typeparam name="T">type of class to inject the logger to</typeparam>
        /// <returns>ILogger instance of type T for the class constructor of type T</returns>
        public static ILogger<T> CreateLoggerForInjection<T>()
        {
            // Create a MS extensions ILogger instance of the serilog logger
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddSerilog(Log.Logger);

            var logger = loggerFactory.CreateLogger<T>();

            return logger;
        }
    }
}