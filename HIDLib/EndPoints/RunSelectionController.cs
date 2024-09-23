using System;
using System.Collections.Generic;
using Data;
using Data.Objects;

namespace HIDLib.EndPoints;

public static class RunSelectionController
{
    public static List<RunInfo> GetAllRuns() => DataManager.AllRuns();
    public static RunInfo LatestRun() => DataManager.LatestRun();

    public static bool TryGetAnalytics(int assignmentRunId, int worstAssignmentThreshold, out RunSelectionAnalytics analytics, out string errorMessage)
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

        throw new NotImplementedException();
    }
}

public class RunSelectionAnalytics
{
    
}