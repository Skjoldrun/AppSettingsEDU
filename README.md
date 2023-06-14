# AppSettingsEDU

Appsettings.json files are used to configure your software. You should put all configuration values in one of the multiple appsettings locations described below.

## default appsettings.json

The default appsettings.json file usually contains the default configuration. This may be the production configuration or the default one if you don't change settings for different environments.

Loading this file is done in the application startup. See the following example for a console application:

```csharp
private static async Task Main(string[] args)
        {
            var appConfig = BuildAppConfig();
            // ... 
        }

        public static IConfiguration BuildAppConfig()
        {
            var appConfig = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // this path works if started from PATH env var
                .AddJsonFile("appsettings.json")
                .Build();
            return appConfig;
        }
```
***Note:***
*The code above sets the basedirectory to the current exe directory. This ensures that the executable even finds its appsettings if it is configured to be called from everywhere through the PATH environment variable on a system.*

You access its values in services through Dependency Injection of the IConfiguration object in the service constructor and then by calling the Method `.GetValue<Type>(<SettingsName>)` or load a typed section by `.GetSection(<SectionName>)`:


## ConnectionStrings

Getting the ConnectionStrings settings has some conventions and therefore some special accessing methods. If the ConnectionStrings Section is defined in proper casing and naming, you can access them with the call: `.GetConnectionString("Default")`


## strongly typed settings objects

### IOptions 

You can read settings sections and convert them into strongly typed objects, like you would do with a json serializer. 

First define the classes to hold your settings data and insert the settings in your appsettings.json file.

Then you can add the using `Microsoft.Extensions.Options` and configure the access and deserializing from the appsettings with `services.Configure<SomeSettingsModel>(appConfig.GetSection("AppSettings:SomeSettingsModel"));` in the `Host.CreateDefaultBuilder().ConfigureServices()` section:

```csharp
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<ISomeService, SomeService>();

        services.Configure<SomeSettingsModel>(appConfig.GetSection("AppSettings:SomeSettingsModel"));
    })
    .UseSerilog()
    .Build();
```

This registers the data for you to access via Dependency Injection in the class you need this data. Add the `IOptions<SomeSettingsModel> someSettingsModel` dependency through constructor:

```csharp
private readonly IConfiguration _config;
private readonly SomeSettingsModel _someSettingsModel;
private readonly ILogger<SomeService> _logger;

public SomeService(IConfiguration config, IOptions<SomeSettingsModel> someSettingsModel, ILogger<SomeService> logger)
{
    _config = config;
    _someSettingsModel = someSettingsModel.Value;
    _logger = logger ?? NullLogger<SomeService>.Instance;
}
```

Then access the object in code with strong types:

```csharp
_logger.LogInformation("Access the values from strongly typed settings ...");

string? modelSetting = _someSettingsModel.ModelSetting;
SomeSubSettingsModel? someSubSettingsModel = _someSettingsModel.SomeSubSettingsModel;

_logger.LogInformation("Value from the model string: {modelSetting}", modelSetting);
_logger.LogInformation("Values from the settings model subsettings: {settingsString}, {settingsInt}",
    someSubSettingsModel.MyModelSubSettingString, someSubSettingsModel.MyModelSubSettingInt);
```

### Alternative way for typed objects from settings

The alternative is to register a singleton of this settings object right away in the process:

```csharp
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
```

This can then be used in a service directly:

```Csharp
public SomeSecondService(IConfiguration config, SomeSettingsModel someSettingsModel, ILogger<SomeService> logger = null)
{
    _config = config;
    _someSettingsModel = someSettingsModel; // alternative way to get this settings as typed object without IOptions
    _logger = logger ?? NullLogger<SomeService>.Instance;
}

public async Task Run()
{
    string? modelSetting = _someSettingsModel.ModelSetting;
    SomeSubSettingsModel? someSubSettingsModel = _someSettingsModel.SomeSubSettingsModel;

    _logger.LogInformation("Value from the model string: {modelSetting}", modelSetting);
    _logger.LogInformation("Values from the settings model subsettings: {settingsString}, {settingsInt}",
        someSubSettingsModel.MyModelSubSettingString, someSubSettingsModel.MyModelSubSettingInt);
}
```


## appsettings.Development.json

There is a way to have seperate development (or other environment dependent) settings files, which overwrite the default settings.

Create an appsettings.Development.json file, add the lauchSettings.json in the project properties (see screenshot below) and add an environment variable called `"DOTNET_ENVIRONMENT": "Development"`.

![lauchSettings.json](/img/launchSettings.png)

Then add a call to optionally load this file on startup, if this env var is set:

```csharp
public static IConfiguration BuildAppConfig()
{
    var appConfig = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", true) // load appsettings.Development.json
        .Build();
    return appConfig;
}
```

Now you can have extra settings only for a development environment that overwrite your default settings without changing the calls where you access the settings in code.


### Set the environment variable in OS

You can use the `SETX` command to set an environment variable in the OS from command line. The user variables don't need further privileges, but system variables need administrator privileges.

```batch
@ECHO OFF

REM Set the environment variable DOTNET_ENVIRONMENT to a stage value
REM /m sets the variable on system level, default is user level
REM SETX <variableName> <Value> [<Params>]

SETX DOTNET_ENVIRONMENT Development /m
```

![OS env var](/img/OS-Environment-Variable.png)


## Read directly from appsettings.json

We had the opportunity to read values from app.config (XML) in .NET Framework with the ConfigurationManager class directly. This is not longer possible for the appsettings.json settings by default, because the design should be, that you inject the config by DI nowadays. But in some older projects you just can't rebuilt all classes to use DI and may be need to access appsettings.json on the fly. 

I have created a helper class to mimic the old ConfigurationManager class for this:

```csharp
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
```

This class allows you to access the appsettings.json values in teh `AppSettings` section from anywhere in the code and also offers the `InitAppConfigBuilder()` method to have one place for reading and adding the json files in the project. This class can be in a library project (needs `Microsoft.Extensions.Hosting` NuGet package) and helps initializing the COnfigrationBuilder on startup besides the static accessing of the json files.

Usage, even with typed results:

```csharp
string? stringResult = AppSettingsHelper.GetValue<string>("MySetting");
_logger.LogInformation("String Value from settings: {result}", stringResult);

int intResult = AppSettingsHelper.GetValue<int>("SomeInt");
_logger.LogInformation("String Value from settings: {result}", intResult);

bool boolResult = AppSettingsHelper.GetValue<bool>("SomeBool");
_logger.LogInformation("String Value from settings: {result}", boolResult);

decimal decimalResult = AppSettingsHelper.GetValue<decimal>("Somedecimal");
_logger.LogInformation("String Value from settings: {result}", decimalResult);
```

