using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Objects;
using HIDLib.EndPoints;
using HIDLib.HTML;
using HIDLib.ReportGenerators;

namespace Controllers;

public class ReportGenController
{
    public static List<RunInfo> GetAllSchedulingRuns() => DataManager.AllRuns();

    public static List<HtmlDoc> GetHtmlDocs(AssignmentDataSet data)
    {
        var generators = new IHtmlGenerator[] {
            new ProblematicAssignmentsReport(data),
            new ClassRosterReport(data),
            new StudentScheduleGridReport(data),
            new AssignmentAnalysisReport(data),
            new StudentScheduleOverviewReport(data),
        };

        return generators.Select(x => x.CreateDoc()).ToList();
    }

    public static void ExportData(List<HtmlDoc> htmlDocs, AssignmentDataSet data, string outputFolder)
    {
        ReportGenerator.WriteRawData(data, outputFolder);
        foreach (var doc in htmlDocs)
            doc.WriteToFile(outputFolder);
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