using System;
using System.IO;

namespace HIDLib;

public static class FilePaths
{
    public static string SampleDataFolder => SettingsManager.GetDefaultInputsFolder();
    public static string ReportsFolder => SettingsManager.GetDefaultOutputFolder();
}