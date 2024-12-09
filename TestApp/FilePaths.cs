using HIDLib;

namespace TestApp;

public static class FilePaths
{
    public static string SampleDataFolder => SettingsManager.GetDefaultInputsFolder();
    public static string ReportsFolder => SettingsManager.GetDefaultOutputFolder();
}