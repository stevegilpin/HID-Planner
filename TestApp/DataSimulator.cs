using System;
using System.IO;
using Data;
using HIDLib;
using HIDLib.EndPoints;
using HIDLib.HTML;
using HIDLib.ReportGenerators;

namespace TestApp;

public static class DataSimulator
{
    public static void TestRunScheduling() {
        var studentInfoFilePath = Path.Combine(FilePaths.SampleDataFolder, "StudentInfoSample.csv");
        var rankingFilePath = Path.Combine(FilePaths.SampleDataFolder, "StudentCourseRankingSample.csv");
        var scheduleFilePath = Path.Combine(FilePaths.SampleDataFolder, "CourseScheduleSample.csv");
        var courseInfoFilePath = Path.Combine(FilePaths.SampleDataFolder, "CourseInfoSample.csv");
        RunScheduling(courseInfoFilePath, scheduleFilePath, studentInfoFilePath, rankingFilePath);
    }

    public static void RunScheduling(string courseInfoFilePath, string scheduleFilePath, string studentInfoFilePath, string rankingFilePath)
    {
        var controller = new SchedulingController { RunName = "Manual test run" };

        if (controller.TrySetFilePaths([courseInfoFilePath, scheduleFilePath, studentInfoFilePath, rankingFilePath], out var error) == false) {
            Console.WriteLine(error);
            return;
        }

        var message = controller.RunScheduling();
        Console.WriteLine(message);
    }

    public static void CreateReports()
    {
        var latest = DataManager.LatestRun();
        var data = DataManager.LoadDataSet(latest.RunId);
        var generators = new IHtmlGenerator[] {
            new ProblematicAssignmentsReport(data),
            new ClassRosterReport(data),
            new StudentScheduleGridReport(data),
            new AssignmentAnalysisReport(data),
            new StudentScheduleOverviewReport(data),
        };

        foreach (var g in generators)
            g.CreateDoc().WriteToFile(FilePaths.ReportsFolder);
        
        ReportGenerator.WriteRawData(data, FilePaths.ReportsFolder);

        Console.WriteLine($"Data written to {FilePaths.ReportsFolder}");
    }
}