using System;
using System.IO;
using System.Linq;
using Database;
using Database.Objects;
using HIDLib.DataImport;

namespace TestApp;

public static class DataSimulator
{
    public static readonly string SampleDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HID", "sample data");
    private const string EVENT_ID = "2024";

    public static void PopulateCourseInfo()
    {
        using var db = DbProvider.GetDbContext();
        if (db.CourseInfo.Any())
            return;

        var filePath = Path.Combine(SampleDataFolder, "CourseInfoSample.csv");
        var rows = CsvReader.ReadCourseInfo(filePath, EVENT_ID);
        Console.WriteLine($"Adding {rows.Count} CourseInfo rows");
        db.CourseInfo.AddRange(rows);
        db.SaveChanges();
    }

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

    public static void PopulateCourseSchedule()
    {
        using var db = DbProvider.GetDbContext();
        if (db.CourseScheduleInputDescription.Any())
            return;

        const int inputId = 1;
        
        var scheduleFilePath = Path.Combine(SampleDataFolder, "CourseScheduleSample.csv");
        var capacityFilePath = Path.Combine(SampleDataFolder, "CourseCapacitySample.csv");
        var scheduleRows = CsvReader.ReadCourseSchedule(scheduleFilePath, inputId);
        var capacityRows = CsvReader.ReadCourseCapacity(capacityFilePath, inputId);
        Console.WriteLine($"Adding {scheduleRows.Count} {nameof(CourseSchedule)} rows");
        Console.WriteLine($"Adding {capacityRows.Count} {nameof(CourseCapacity)} rows");

        db.CourseScheduleInputDescription.Add(new() {
            InputId = inputId,
            Description = $"Imported from {scheduleFilePath} and {capacityFilePath}",
            EventId = EVENT_ID,
        });
        db.CourseSchedule.AddRange(scheduleRows);
        db.CourseCapacity.AddRange(capacityRows);
        db.SaveChanges();
    }
}