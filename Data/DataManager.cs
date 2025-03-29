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

    public static List<Rooms> AllRooms()
    {
        using var db = DbProvider.GetDbContext();
        return db.Rooms.ToList();
    }

    public static List<Instructor> AllInstructors()
    {
        using var db = DbProvider.GetDbContext();
        return db.Instructor.ToList();
    }

    public static List<Supervisor> AllSupervisors()
    {
        using var db = DbProvider.GetDbContext();
        return db.Supervisor.ToList();
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

    public static string ClearApplicationData()
    {
        var backupLocation = DbProvider.DeleteDatabase();
        Console.WriteLine($"Database deleted. A backup was saved at {backupLocation}.");
        return backupLocation;
    }

    public static void UpdateRooms(List<Rooms> rooms)
    {
        using var db = DbProvider.GetDbContext();
        db.Rooms.ExecuteDelete();
        db.Rooms.AddRange(rooms);
        db.SaveChanges();
    }

    public static void UpdateSupervisors(List<Supervisor> supervisors)
    {
        using var db = DbProvider.GetDbContext();
        db.Supervisor.ExecuteDelete();
        db.Supervisor.AddRange(supervisors);
        db.SaveChanges();
    }

    public static void UpdateInstructors(List<Instructor> instructors)
    {
        using var db = DbProvider.GetDbContext();
        db.Instructor.ExecuteDelete();
        db.Instructor.AddRange(instructors);
        db.SaveChanges();
    }
}