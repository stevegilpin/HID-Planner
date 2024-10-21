using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers;
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
        RunScheduling(courseInfoFilePath, scheduleFilePath, studentInfoFilePath, rankingFilePath);
    }

    public static void RunScheduling(string courseInfoFilePath, string scheduleFilePath, string studentInfoFilePath, string rankingFilePath)
    {
        var controller = new SchedulingController {
            InputDescription = "Manual test run",
            CourseInfoFilePath = courseInfoFilePath,
            CourseScheduleFilePath = scheduleFilePath,
            StudentInfoFilePath = studentInfoFilePath,
            StudentCourseRankingsFilePath = rankingFilePath
        };

        var message = controller.RunScheduling();
        Console.WriteLine(message);
    }

    public static void CreateReports()
    {
        var latest = DataManager.LatestRun();
        var message = ReportGenController.GenerateReports(latest.RunId, ReportsFolder);
        Console.WriteLine(message);
    }
}