using AppsettingsEDU.Lib.Helper;

namespace AppsettingsEDU.Lib
{
    public static class StaticValues
    {
        public static readonly string SomeStringValue = "Some string value";
        public static readonly string SomeConfigString = AppSettingsHelper.GetValue<string>("MySetting");
    }
}