using System;
using System.Linq;
using Database;
using HIDLib.DataImport;

namespace TestApp;

public static class DataSimulator
{
    public static void PopulateCourseInfo()
    {
        using var db = DbProvider.GetDbContext();
        if (db.CourseInfo.Any())
            return;

        var rows = CsvReader.ReadCourseInfo(@"C:\sql\CourseInfoSample.csv", "2024");
        Console.WriteLine($"Adding {rows.Count} CourseInfo rows");
        db.CourseInfo.AddRange(rows);
        db.SaveChanges();
    }

    public static void PopulateStudentInfo()
    {
        using var db = DbProvider.GetDbContext();
        if (db.StudentInfo.Any())
            return;
        
        var rows = CsvReader.ReadStudentInfo(@"C:\sql\StudentInfoSample.csv");
        Console.WriteLine($"Adding {rows.Count} StudentInfo rows");
        db.StudentInfo.AddRange(rows);
        db.SaveChanges();
    }
}