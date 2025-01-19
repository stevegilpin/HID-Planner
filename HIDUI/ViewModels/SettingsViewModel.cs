using System;
using System.Linq;
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
    
    private void PopulateSettings() {
        DbFileLocation = SettingsManager.GetDatabaseFileLocation();
        DefaultInputsFolder = SettingsManager.GetDefaultInputsFolder();
        DefaultOutputsFolder = SettingsManager.GetDefaultOutputFolder();
        CostFunctionExponent = SettingsManager.GetCostFunctionExponent().ToString();
        BadCourseRankCutoff = SettingsManager.GetBadCourseRankCutoff().ToString();
    }
    
    public bool TrySetDefaultInputFolderLocation(string path, out string errorMessage) {
        if (string.IsNullOrEmpty(path)) {
            errorMessage = "Cannot set default input folder path to empty string.";
            return false;
        } else {
            DefaultInputsFolder = path;
            SettingsManager.UpdateSetting("INPUTS_FOLDER", path);
            errorMessage = null;
            return true;
        }            
    }

    public bool TrySetDefaultOutputFolderLocation(string path, out string errorMessage) {
        if (string.IsNullOrEmpty(path)) {
            errorMessage = "Cannot set default output folder path to empty string.";
            return false;
        } else {
            DefaultOutputsFolder = path;
            SettingsManager.UpdateSetting("OUTPUT_FOLDER", path);
            errorMessage = null;
            return true;
        }            
    }

    public void SaveCostFunctionExponent() {
        if (CostFunctionExponent != "" && CostFunctionExponent.All(char.IsDigit)) {
            // int parseResult;
            if (int.TryParse(CostFunctionExponent, out _)) {
                SettingsManager.UpdateSetting("COST_EXPONENT", CostFunctionExponent);
            } else {
                // TODO
            }
        }
    }

    public void SaveBadCourseRankCutoff() {
        if (BadCourseRankCutoff != "" && BadCourseRankCutoff.All(char.IsDigit)) {
            // int parseResult;
            if (int.TryParse(BadCourseRankCutoff, out _)) {
                SettingsManager.UpdateSetting("BAD_RANK_CUTOFF", BadCourseRankCutoff);
            } else{
                // TODO
            }
        }
    }

    public void ClearDatabase()
    {
        DataManager.ClearApplicationData();
        Environment.Exit(0);
    }
}