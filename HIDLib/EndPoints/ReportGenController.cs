using System;
using Data;
using Data.Objects;
using HIDLib.HTML;
using HIDLib.ReportGenerators;

namespace HIDLib.EndPoints;

public static class ReportGenController
{
    public static bool TryGenerateReports(int assignmentRunId, string outputFolder, out string errorMessage)
    {
        AssignmentDataSet data;
        try {
            data = DataManager.LoadDataSet(assignmentRunId);
        }
        catch (Exception e) {
            errorMessage = $"Error loading data for run ID {assignmentRunId}:{Environment.NewLine}{e.Message}";
            return false;
        }

        var generators = new IHtmlGenerator[] {
            new ProblematicAssignmentsReport(data),
            new ClassRosterReport(data),
            new StudentScheduleGridReport(data),
            new AssignmentAnalysisReport(data),
            new StudentScheduleOverviewReport(data),
        };

        try {
            CsvUtil.WriteResultsToCsv(data, outputFolder);
        }
        catch (Exception e) {
            errorMessage = $"Error saving raw results{Environment.NewLine}{e.Message}";
            return false;
        }

        try {
            foreach (var g in generators)
                g.CreateDoc().WriteToFile(outputFolder);
        }
        catch (Exception e) {
            errorMessage = $"Error saving reports:{Environment.NewLine}{e.Message}";
            return false;
        }

        Console.WriteLine($"Saved reports to {outputFolder}");

        errorMessage = null;
        return true;
    }
}