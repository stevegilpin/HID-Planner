using System;
using System.Collections.Generic;
using Data;
using Data.Objects;
using HIDLib.EndPoints;

namespace Controllers;

public class ReportGenController
{
    public static List<RunInfo> GetAllSchedulingRuns() => DataManager.AllRuns();
    public static RunInfo LatestSchedulingRun() => DataManager.LatestRun();

    public static string GenerateReports(int assignmentRunId, string outputFolder)
    {
        AssignmentDataSet data;
        try {
            data = DataManager.LoadDataSet(assignmentRunId);
        }
        catch (Exception e) {
            return $"Error loading data for run ID {assignmentRunId}:{Environment.NewLine}{e.Message}";
        }

        try {
            ReportGenerator.WriteRawData(data, outputFolder);
        }
        catch (Exception e) {
            return $"Error saving raw results{Environment.NewLine}{e.Message}";
        }

        try {
            ReportGenerator.GenerateReports(data, outputFolder);
        }
        catch (Exception e) {
            return $"Error saving reports:{Environment.NewLine}{e.Message}";
        }

        return $"Saved reports to {outputFolder}";
    }

    public static bool TryGetAnalytics(int assignmentRunId, out SchedulingRunAnalyticData analytics, out string errorMessage)
    {
        AssignmentDataSet data;
        try {
            data = DataManager.LoadDataSet(assignmentRunId);
        }
        catch (Exception e) {
            errorMessage = $"Error loading data for run ID {assignmentRunId}:{Environment.NewLine}{e.Message}";
            analytics = null;
            return false;
        }

        try {
            analytics = SchedulingRunSelector.GetAnalyticData(data);
            errorMessage = null;
            return true;
        }
        catch (Exception e) {
            analytics = null;
            errorMessage = $"Error creating analytics:{Environment.NewLine}{e.Message}";
            return false;
        }
    }
}