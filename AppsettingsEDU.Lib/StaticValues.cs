using AppsettingsEDU.Lib.Helper;

namespace AppsettingsEDU.Lib
{
    /// <summary>
    /// This class helps simulate the change from old app.config to new appsettings.json for a case in the XRP project.
    /// </summary>
    public static class StaticValues
    {
        public static readonly string SomeStringValue = "Some string value";
        public static readonly string SomeConfigString = AppSettingsHelper.GetValue<string>("MySetting");
    }
}