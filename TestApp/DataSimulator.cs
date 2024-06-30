using System;
using System.IO;
using System.Linq;
using Database;
using Database.Objects;
using HIDLib;
using HIDLib.DataImport;

namespace TestApp;

public static class DataSimulator
{
    public static readonly string SampleDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "sample data");
    private const string EVENT_ID = "2024";

    public static void PopulateStudentInfo()
    {
        using var db = DbProvider.GetDbContext();
        if (db.StudentInfo.Any())
            return;
        
        var filePath = Path.Combine(SampleDataFolder, "StudentInfoSample.csv");
        var rows = CsvReader.ReadStudentInfo(filePath);
        
        Console.WriteLine($"Adding {rows.Count} StudentInfo rows");
        db.StudentInfo.AddRange(rows);
        db.SaveChanges();
    }

    public static void PopulateStudentRankings()
    {
        using var db = DbProvider.GetDbContext();
        if (db.StudentCourseRanking.Any())
            return;

        var inputId = 1;
        if (db.StudentRankingInputDescription.Any())
            inputId = db.StudentRankingInputDescription.Max(x => x.InputId) + 1;

        var filePath = Path.Combine(SampleDataFolder, "StudentCourseRankingSample.csv");
        var rows = CsvReader.ReadStudentCourseRankings(filePath, inputId);
        Console.WriteLine($"Adding {rows.Count} StudentCourseRanking rows");

        db.StudentRankingInputDescription.Add(new() {
            InputId = inputId,
            Description = $"Imported from {filePath}",
            EventId = EVENT_ID,
        });
        db.StudentCourseRanking.AddRange(rows);
        db.SaveChanges();
    }

    public static void PopulateCourseInfo()
    {
        using var db = DbProvider.GetDbContext();
        if (db.CourseScheduleInputDescription.Any())
            return;

        const int inputId = 1;
        
        var scheduleFilePath = Path.Combine(SampleDataFolder, "CourseScheduleSample.csv");
        var infoFilePath = Path.Combine(SampleDataFolder, "CourseInfoSample.csv");
        var courseInfoRows = CsvReader.ReadCourseInfo(infoFilePath, inputId);
        var scheduleRows = CsvReader.ReadCourseSchedule(scheduleFilePath, inputId);
        Console.WriteLine($"Adding {courseInfoRows.Count} {nameof(courseInfoRows)} rows");
        Console.WriteLine($"Adding {scheduleRows.Count} {nameof(CourseSchedule)} rows");

        db.CourseScheduleInputDescription.Add(new() {
            InputId = inputId,
            Description = $"Imported from {scheduleFilePath} and {infoFilePath}",
            EventId = EVENT_ID,
        });
        db.CourseInfo.AddRange(courseInfoRows);
        db.CourseSchedule.AddRange(scheduleRows);
        db.SaveChanges();
    }

    public static void RunScheduling()
    {
        var scheduleId = DbController.Courses.GetLatestScheduleInputId();
        var rankingId = DbController.StudentRankings.GetLatestRankingInputId();
        var output = SchedulingSolver.RunSolve(rankingId, scheduleId);
        DbController.Scheduling.SaveSchedulingOutput(output, "Test solve");
    }
}