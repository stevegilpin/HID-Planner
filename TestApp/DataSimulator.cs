using System;
using System.IO;
using System.Linq;
using Data;
using HIDLib;
using Data.Objects;
using HIDLib.ReportGenerators;

namespace TestApp;

public static class DataSimulator
{
    public static readonly string SampleDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "sample data");
    public static readonly string ReportsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "reports");
    private const string EVENT_ID = "2024";

    public static void PopulateStudentInfo()
    {
        var infoFilePath = Path.Combine(SampleDataFolder, "StudentInfoSample.csv");
        var infoRows = CsvReader.ReadStudentInfo(infoFilePath);
        var rankingFilePath = Path.Combine(SampleDataFolder, "StudentCourseRankingSample.csv");
        var rankingRows = CsvReader.ReadStudentCourseRankings(rankingFilePath);
        
        Console.WriteLine($"Adding {infoRows.Count} StudentInfo rows");
        Console.WriteLine($"Adding {rankingRows.Count} StudentCourseRanking rows");
        DataManager.Students.SaveStudentInfo(infoRows, rankingRows, EVENT_ID, $"Imported from {rankingFilePath}");
    }

    public static void PopulateCourseInfo()
    {
        var scheduleFilePath = Path.Combine(SampleDataFolder, "CourseScheduleSample.csv");
        var infoFilePath = Path.Combine(SampleDataFolder, "CourseInfoSample.csv");
        var courseInfoRows = CsvReader.ReadCourseInfo(infoFilePath);
        var scheduleRows = CsvReader.ReadCourseSchedule(scheduleFilePath);
        Console.WriteLine($"Adding {courseInfoRows.Count} {nameof(courseInfoRows)} rows");
        Console.WriteLine($"Adding {scheduleRows.Count} {nameof(CourseSchedule)} rows");

        var inputDescription = $"Imported from {scheduleFilePath} and {infoFilePath}";
        DataManager.Courses.SaveCourseInfo(courseInfoRows, scheduleRows, EVENT_ID, inputDescription);
    }

    public static void RunScheduling()
    {
        var scheduleId = DataManager.Courses.GetLatestScheduleInputId();
        var rankingId = DataManager.Students.GetLatestRankingInputId();
        var output = SchedulingSolver.RunSolve(rankingId, scheduleId);
        DataManager.Scheduling.SaveSchedulingOutput(output.ToDbRunRow("Test solve"), output.ToDbResultRows());
    }

    public static HtmlDoc CreateReport()
    {
        Console.WriteLine($"Saving the class roster report to {ReportsFolder}");
        var latestRun = DataManager.Scheduling.LatestRun();
        var data = DataManager.Scheduling.LoadSavedData(latestRun.RunId);
        var report = ClassRosterReport.Create(data);
        report.WriteToFile(ReportsFolder);
        return report;
    }
}