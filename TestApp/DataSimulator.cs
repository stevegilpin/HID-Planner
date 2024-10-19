using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using HIDLib;
using Data.Objects;
using HIDLib.EndPoints;
using HIDLib.HTML;
using HIDLib.ReportGenerators;

namespace TestApp;

public static class DataSimulator
{
    public static readonly string SampleDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "sample data");
    public static readonly string ReportsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "reports");

    public static void TestRunScheduling() {
        var studentInfoFilePath = Path.Combine(SampleDataFolder, "StudentInfoSample.csv");
        var rankingFilePath = Path.Combine(SampleDataFolder, "StudentCourseRankingSample.csv");
        var scheduleFilePath = Path.Combine(SampleDataFolder, "CourseScheduleSample.csv");
        var courseInfoFilePath = Path.Combine(SampleDataFolder, "CourseInfoSample.csv");

        var runInputs = new AssignmentRunInputs {
            RunDescription = "Test run",
            CourseInfoFilePath = courseInfoFilePath,
            CourseScheduleFilePath = scheduleFilePath,
            StudentInfoFilePath = studentInfoFilePath,
            StudentRankingsFilePath = rankingFilePath
        };

        RunScheduling(courseInfoFilePath, scheduleFilePath, studentInfoFilePath, rankingFilePath);
    }

    public static void RunScheduling(string courseInfoFilePath, string scheduleFilePath, string studentInfoFilePath, string rankingFilePath)
    {
        var runInputs = new AssignmentRunInputs {
            RunDescription = "Manual test run",
            CourseInfoFilePath = courseInfoFilePath,
            CourseScheduleFilePath = scheduleFilePath,
            StudentInfoFilePath = studentInfoFilePath,
            StudentRankingsFilePath = rankingFilePath
        };

        var runResult = AssignmentRunController.RunAssignments(runInputs);
        if (runResult.ValidInputs == false)
            Console.WriteLine(runResult.InputValidationError);
        else if (runResult.SuccessfulAssignment == false)
            Console.WriteLine(runResult.CourseAssignmentError);
        else
            Console.WriteLine($"Results saved with ID {runResult.RunId}");
    }

    public static void CreateReports()
    {
        var latest = DataManager.LatestRun();
        if (ReportGenController.TryGenerateReports(latest.RunId, ReportsFolder, out var error) == false)
            Console.WriteLine(error);
    }
}