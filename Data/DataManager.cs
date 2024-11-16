using System;
using System.Collections.Generic;
using System.Linq;
using Data.Database;
using Data.Objects;
using Microsoft.EntityFrameworkCore;

namespace Data;

public static class DataManager
{
    public static int Save(AssignmentDataSet dataSet, string name)
    {
        using var db = DbProvider.GetDbContext();
        var runId = 1; // todo create db sequence for this
        if (db.RunInfo.Any())
            runId = db.RunInfo.Max(x => x.RunId) + 1;

        var studentInfo = dataSet.InputData.StudentInfo.Values.ToList();
        var studentCourseRankings = dataSet.InputData.StudentCourseRanking.SelectMany(x => x.Value).ToList();
        var courseInfo = dataSet.InputData.CourseInfo.Values.ToList();
        var courseSchedule = dataSet.InputData.CourseSchedule.SelectMany(x => x.Value).ToList();

        studentInfo.ForEach(x => x.RunId = runId);
        studentCourseRankings.ForEach(x => x.RunId = runId);
        courseInfo.ForEach(x => x.RunId = runId);
        courseSchedule.ForEach(x => x.RunId = runId);
        dataSet.AssignmentResults.ForEach(x => x.RunId = runId);
        dataSet.RunInfo.RunId = runId;
        dataSet.RunInfo.Name = name;

        db.StudentInfo.AddRange(studentInfo);
        db.StudentCourseRanking.AddRange(studentCourseRankings);
        db.CourseInfo.AddRange(courseInfo);
        db.CourseSchedule.AddRange(courseSchedule);
        db.CourseAssignmentResult.AddRange(dataSet.AssignmentResults);
        db.RunInfo.Add(dataSet.RunInfo);

        db.SaveChanges();

        return runId;
    }

    public static RunInfo LatestRun()
    {
        using var db = DbProvider.GetDbContext();
        var id = db.RunInfo.Max(x => x.RunId);
        return db.RunInfo.Single(x => x.RunId == id);
    }

    public static List<RunInfo> AllRuns()
    {
        using var db = DbProvider.GetDbContext();
        return db.RunInfo.ToList();
    }

    public static AssignmentDataSet LoadDataSet(int id)
    {
        using var db = DbProvider.GetDbContext();
        var studentInfo = db.StudentInfo.Where(x => x.RunId == id).ToList();
        var studentCourseRankings = db.StudentCourseRanking.Where(x => x.RunId == id).ToList();
        var courseInfo = db.CourseInfo.Where(x => x.RunId == id).ToList();
        var courseSchedule = db.CourseSchedule.Where(x => x.RunId == id).ToList();
        var results = db.CourseAssignmentResult.Where(x => x.RunId == id).ToList();
        var run = db.RunInfo.Single(x => x.RunId == id);

        var input = new InputDataSet(courseInfo, courseSchedule, studentInfo, studentCourseRankings);

        return new AssignmentDataSet {
            InputData = input,
            AssignmentResults = results,
            RunInfo = run
        };
    }

    public static void UpdateRunName(int runId, string name)
    {
        using var db = DbProvider.GetDbContext();
        var run = db.RunInfo.Single(x => x.RunId == runId);
        run.Name = name;
        db.SaveChanges();
    }

    public static void UpdateRunNotes(int runId, string notes)
    {
        using var db = DbProvider.GetDbContext();
        var run = db.RunInfo.Single(x => x.RunId == runId);
        run.Notes = notes;
        db.SaveChanges();
    }

    public static void ClearApplicationData()
    {
        if (DbProvider.TryDeleteDatabase(true, out var backupLocation))
            Console.WriteLine($"Database deleted. A backup was saved at {backupLocation}.");
        else
            Console.WriteLine("Database delete failed.");
    }

    public static List<Settings> GetSettings()
    {
        using var db = DbProvider.GetDbContext();
        return db.Settings.ToList();
    }

    public static string GetSettingValue(string name)
    {
        using var db = DbProvider.GetDbContext();
        return db.Settings.Single(x => x.Name == name).Value;
    }
    
    public static void UpdateSetting(string name, string value)
    {
        using var db = DbProvider.GetDbContext();
        var row = db.Settings.Single(x => x.Name == name);
        row.Value = value;
        db.SaveChanges();
    }

    public static void ResetSettings()
    {
        using (var db = DbProvider.GetDbContext())
            db.Database.ExecuteSqlRaw($"TRUNCATE TABLE {nameof(Settings)}");

        DefaultSettings.PopulateSettingsTable();
    }
}