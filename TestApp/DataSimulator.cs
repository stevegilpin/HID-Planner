using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using HIDLib;
using Data.Objects;
using HIDLib.HTML;
using HIDLib.ReportGenerators;

namespace TestApp;

public static class DataSimulator
{
    public static readonly string SampleDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "sample data");
    public static readonly string ReportsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "reports");

    private static InputDataSet LoadInputs()
    {
        var studentInfoFilePath = Path.Combine(SampleDataFolder, "StudentInfoSample.csv");
        var infoRows = CsvUtil.ReadStudentInfo(studentInfoFilePath);
        var rankingFilePath = Path.Combine(SampleDataFolder, "StudentCourseRankingSample.csv");
        var rankingRows = CsvUtil.ReadStudentCourseRankings(rankingFilePath);

        var scheduleFilePath = Path.Combine(SampleDataFolder, "CourseScheduleSample.csv");
        var courseInfoFilePath = Path.Combine(SampleDataFolder, "CourseInfoSample.csv");
        var courseInfoRows = CsvUtil.ReadCourseInfo(courseInfoFilePath);
        var scheduleRows = CsvUtil.ReadCourseSchedule(scheduleFilePath);

        Console.WriteLine($"Read {infoRows.Count} StudentInfo rows");
        Console.WriteLine($"Read {rankingRows.Count} StudentCourseRanking rows");
        Console.WriteLine($"Read {courseInfoRows.Count} {nameof(courseInfoRows)} rows");
        Console.WriteLine($"Read {scheduleRows.Count} {nameof(CourseSchedule)} rows");

        return new InputDataSet(courseInfoRows, scheduleRows, infoRows, rankingRows);
    }

    public static void RunScheduling()
    {
        var input = LoadInputs();
        // todo run input consistency check
        var output = SchedulingSolver.RunSolve(input);
        DataManager.Save(output, $"Default inputs loaded from {SampleDataFolder}");
    }

    public static void CreateReports()
    {
        Console.WriteLine($"Saving reports to {ReportsFolder}");
        var latestRun = DataManager.LatestRun();
        var data = DataManager.LoadDataSet(latestRun.RunId);

        var generators = new IHtmlGenerator[] {
            new ProblematicAssignmentsReport(data),
            new ClassRosterReport(data),
            new StudentScheduleGridReport(data),
            new AssignmentAnalysisReport(data),
            new StudentScheduleOverviewReport(data),
        };

        foreach (var g in generators)
            g.CreateDoc().WriteToFile(ReportsFolder);
        Console.WriteLine("Done");
    }

    public static void DumpResultsFromLatestRun()
    {
        var latestRun = DataManager.LatestRun();
        var data = DataManager.LoadDataSet(latestRun.RunId);
        Console.WriteLine($"Writing raw data to {ReportsFolder}");
        CsvUtil.WriteResultsToCsv(data, ReportsFolder);
        Console.WriteLine("Done");
    }
}