using System;
using System.IO;
using System.Runtime.InteropServices;
using Database.Objects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Database;

public static class DbProvider
{
    private static readonly string DB_FOLDER;
    private const string DB_FILE_NAME = "HID_Planner_db";

    static DbProvider()
    {
        DB_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HID Planner");
        Directory.CreateDirectory(DB_FOLDER);
    }
    
    public static HidDbContext GetDbContext()
    {
        var dbFilePath = GetDbFilePath();
        if (File.Exists(dbFilePath) == false)
            CreateDatabase();

        var optionBuilder = new DbContextOptionsBuilder();
        optionBuilder.UseSqlite(GetConnectionString());
        return new HidDbContext(optionBuilder.Options);
    }

    private static string GetDbFilePath()
    {
        return Path.Combine(DB_FOLDER, DB_FILE_NAME);
    }

    private static string GetConnectionString() => $"Data Source={GetDbFilePath()}";

    private static void CreateDatabase()
    {
        Console.WriteLine("Creating database");
        Directory.CreateDirectory(DB_FOLDER);
        using var connection = new SqliteConnection(GetConnectionString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = CREATE_SCHEMA_SQL;
        command.ExecuteNonQuery();
    }

    private const string CREATE_SCHEMA_SQL =
        @"
CREATE TABLE [Rooms] (
  [RoomCode] varchar(20) PRIMARY KEY NOT NULL,
  [Description] varchar(255)
);

CREATE TABLE [StudentInfo] (
  [StudentId] int PRIMARY KEY NOT NULL,
  [FirstName] varchar(200) NOT NULL,
  [LastName] varchar(200) NOT NULL,
  [Grade] varchar(10) NOT NULL,
  [Homeroom] varchar(20)
);

CREATE TABLE [StudentRankingInputDescription] (
  [InputId] int PRIMARY KEY NOT NULL,
  [EventId] varchar(50) NOT NULL,
  [Description] varchar(255)
);

CREATE TABLE [StudentCourseRanking] (
  [RankingInputId] int NOT NULL,
  [StudentId] int NOT NULL,
  [CourseCode] varchar(20) NOT NULL,
  [Rank] int NOT NULL,
  PRIMARY KEY ([RankingInputId], [StudentId], [CourseCode])
);

CREATE TABLE [CourseInfo] (
  [CourseCode] varchar(20) NOT NULL,
  [EventId] varchar(50) NOT NULL,
  [CourseName] varchar(250) NULL,
  [Description] varchar(255),
  [GradesFlags] int NULL,
  PRIMARY KEY ([CourseCode], [EventId])
);

CREATE TABLE [CourseScheduleInputDescription] (
  [InputId] int PRIMARY KEY NOT NULL,
  [EventId] varchar(50) NOT NULL,
  [Description] varchar(255)
);

CREATE TABLE [CourseCapacity] (
  [CourseScheduleInputId] int NOT NULL,
  [CourseCode] varchar(20) NOT NULL,
  [MinCapacity] int,
  [MaxCapacity] int,
  PRIMARY KEY ([CourseScheduleInputId], [CourseCode])
);

CREATE TABLE [CourseSchedule] (
  [CourseScheduleInputId] int NOT NULL,
  [CourseCode] varchar(20) NOT NULL,
  [SessionId] int NOT NULL,
  [RoomCode] varchar(20),
  [Instructor] varchar(200),
  [Supervisor] varchar(200),
  PRIMARY KEY ([CourseScheduleInputId], [CourseCode], [SessionId])
);

CREATE TABLE [CourseAssignmentRunInfo] (
  [RunId] int PRIMARY KEY NOT NULL,
  [RunTimeStamp] DateTime2 NOT NULL,
  [StudentRankingInputId] int NOT NULL,
  [CourseScheduleInputId] int NOT NULL,
  [Description] varchar(255)
);

CREATE TABLE [CourseAssignmentResults] (
  [RunId] int NOT NULL,
  [StudentId] int NOT NULL,
  [SessionId] int NOT NULL,
  [CourseCode] varchar(20) NOT NULL,
  PRIMARY KEY ([RunId], [StudentId], [SessionId])
);
";
}