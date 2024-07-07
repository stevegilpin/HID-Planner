using System;
using System.IO;
using System.Linq;
using Data;
using DataAccess;
using Controllers;

namespace TestApp;

public static class DataSimulator
{
    public static readonly string SampleDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "sample data");
    private const string EVENT_ID = "2024";

    public static void PopulateStudentInfo()
    {
        var filePath = Path.Combine(SampleDataFolder, "StudentInfoSample.csv");
        var rows = CsvReader.ReadStudentInfo(filePath);
        
        Console.WriteLine($"Adding {rows.Count} StudentInfo rows");
        DataManager.Students.SaveStudentInfo(rows);
    }

    public static void PopulateStudentRankings()
    {
        var filePath = Path.Combine(SampleDataFolder, "StudentCourseRankingSample.csv");
        var rows = CsvReader.ReadStudentCourseRankings(filePath);
        Console.WriteLine($"Adding {rows.Count} StudentCourseRanking rows");
        DataManager.Students.SaveRankingInput(rows, EVENT_ID, $"Imported from {filePath}");
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
}