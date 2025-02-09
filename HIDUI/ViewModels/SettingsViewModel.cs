using System;
using Data;
using HIDLib;

namespace HIDUI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public string DbFileLocation { get; set; } = string.Empty;
    public string DefaultInputsFolder { get; set; } = string.Empty;
    public string DefaultOutputsFolder { get; set; } = string.Empty;
    public string CostFunctionExponent { get; set; } = string.Empty;
    public string BadCourseRankCutoff { get; set; } = string.Empty;
    
    public void PopulateSettings() {
        DbFileLocation = SettingsManager.GetDatabaseFileLocation();
        DefaultInputsFolder = SettingsManager.GetDefaultInputsFolder();
        DefaultOutputsFolder = SettingsManager.GetDefaultOutputFolder();
        CostFunctionExponent = SettingsManager.GetCostFunctionExponent().ToString();
        BadCourseRankCutoff = SettingsManager.GetBadCourseRankCutoff().ToString();

        OnPropertyChanged(nameof(DbFileLocation));
        OnPropertyChanged(nameof(DefaultInputsFolder));
        OnPropertyChanged(nameof(DefaultOutputsFolder));
        OnPropertyChanged(nameof(CostFunctionExponent));
        OnPropertyChanged(nameof(BadCourseRankCutoff));
    }
    
    public void SetDefaultInputFolderLocation(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        DefaultInputsFolder = path;
        SettingsManager.UpdateSetting("INPUTS_FOLDER", path);
        PopulateSettings();
    }

    public void SetDefaultOutputFolderLocation(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        DefaultOutputsFolder = path;
        SettingsManager.UpdateSetting("OUTPUT_FOLDER", path);
        PopulateSettings();
    }

    public void SaveCostFunctionExponent() {
        if (int.TryParse(CostFunctionExponent, out _)) {
            SettingsManager.UpdateSetting("COST_EXPONENT", CostFunctionExponent);
            PopulateSettings();
        } else
            throw new Exception($"Cannot parse {CostFunctionExponent} as an integer.");
    }

    public void SaveBadCourseRankCutoff() {
        if (int.TryParse(BadCourseRankCutoff, out _)) {
            SettingsManager.UpdateSetting("BAD_RANK_CUTOFF", BadCourseRankCutoff);
            PopulateSettings();
        } else
            throw new Exception($"Cannot parse {BadCourseRankCutoff} as an integer.");
    }

    public string ClearDatabase() => DataManager.ClearApplicationData();
}