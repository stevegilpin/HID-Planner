using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using Data.Database;
using Data.Objects;
using Microsoft.EntityFrameworkCore;

namespace HIDLib;

public static class SettingsManager
{
    private static class SettingNames
    {
        public const string DatabaseFileLocation = "DB_FILE_LOC";
        public const string InputsFolder = "INPUTS_FOLDER";
        public const string OutputFolder = "OUTPUT_FOLDER";
        public const string BadCourseRankCutoff = "BAD_RANK_CUTOFF";
        public const string CostFunctionExponent = "COST_EXPONENT";
    }

    public static string GetDatabaseFileLocation() => GetSettingValue(SettingNames.DatabaseFileLocation);
    public static string GetDefaultInputsFolder() => GetSettingValue(SettingNames.InputsFolder);
    public static string GetDefaultOutputFolder() => GetSettingValue(SettingNames.OutputFolder);
    public static int GetBadCourseRankCutoff() => int.Parse(GetSettingValue(SettingNames.BadCourseRankCutoff));
    public static int GetCostFunctionExponent() => int.Parse(GetSettingValue(SettingNames.CostFunctionExponent));

    public static string GetSettingValue(string name)
    {
        using var db = DbProvider.GetDbContext();
        return db.Settings.Single(x => x.Name == name).Value;
    }
    
    public static void UpdateSetting(string name, string value)
    {
        using var db = DbProvider.GetDbContext();
        var row = db.Settings.Single(x => x.Name == name);
        if (row.IsWritable == false)
            throw new Exception($"Cannot change value for setting: {name}");

        row.Value = value;
        db.SaveChanges();
    }

    public static void ResetSettings()
    {
        using (var db = DbProvider.GetDbContext())
            db.Database.ExecuteSqlRaw($"TRUNCATE TABLE {nameof(Settings)}");

        PopulateSettingsTable();
    }

    internal static void PopulateSettingsTable()
    {
        using var db = DbProvider.GetDbContext();
        db.AddRange(
            new Settings {
                Name = SettingNames.DatabaseFileLocation,
                Value = DbProvider.GetDbFilePath(),
                Description = "Location of the database file",
                IsWritable = false,
            },

            new Settings {
                Name = SettingNames.InputsFolder,
                Value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "sample data"),
                Description = "Default location for input selection dialogs",
                IsWritable = true,
            },
            new Settings {
                Name = SettingNames.OutputFolder,
                Value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "reports"),
                Description = "Default location for file outputs",
                IsWritable = true,
            },
            new Settings {
                Name = SettingNames.BadCourseRankCutoff,
                Value = 9.ToString(),
                Description = "Min. ranking for a course assignment to be considered 'bad'",
                IsWritable = true,
            },
            new Settings {
                Name = SettingNames.CostFunctionExponent,
                Value = 3.ToString(),
                Description = "Exponent for optimizer's cost function",
                IsWritable = true,
            }
        );

        db.SaveChanges();
    }
}