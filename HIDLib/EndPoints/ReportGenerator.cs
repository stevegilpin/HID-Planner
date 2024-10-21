using Data.Objects;
using HIDLib.HTML;
using HIDLib.ReportGenerators;

namespace HIDLib.EndPoints;

public static class ReportGenerator
{
    public static void WriteRawData(AssignmentDataSet data, string outputFolder)
    {
        CsvUtil.WriteResultsToCsv(data, outputFolder);
    }
    
    public static void GenerateReports(AssignmentDataSet data, string outputFolder)
    {
        var generators = new IHtmlGenerator[] {
            new ProblematicAssignmentsReport(data),
            new ClassRosterReport(data),
            new StudentScheduleGridReport(data),
            new AssignmentAnalysisReport(data),
            new StudentScheduleOverviewReport(data),
        };

        foreach (var g in generators)
            g.CreateDoc().WriteToFile(outputFolder);
    }
}