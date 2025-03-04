using System;
using System.IO;
using Data.Objects;
using HIDLib;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Data.Database;

internal static class DbProvider
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
        if (File.Exists(dbFilePath) == false) {
            CreateDatabase();
            SettingsManager.PopulateSettingsTable(); // todo circular reference is kinda ugly
        }

        var optionBuilder = new DbContextOptionsBuilder();
        optionBuilder.UseSqlite(GetConnectionString());
        return new HidDbContext(optionBuilder.Options);
    }

    internal static string GetDbFilePath()
    {
        return Path.Combine(DB_FOLDER, DB_FILE_NAME);
    }

    // Pooling=false fixes DB file lock on Windows
    private static string GetConnectionString() => $"Data Source={GetDbFilePath()}; Pooling=false";

    private static void CreateDatabase()
    {
        Console.WriteLine("Creating database");
        Directory.CreateDirectory(DB_FOLDER);
        using var connection = new SqliteConnection(GetConnectionString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = CREATE_SCHEMA_SQL;
        command.ExecuteNonQuery();
        Console.WriteLine("Database created");
    }

    public static string DeleteDatabase()
    {
        var dbFilePath = GetDbFilePath();
        var backupPath = Path.Combine(DB_FOLDER, $"{DB_FILE_NAME}-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}");
        File.Move(dbFilePath, backupPath);
        return backupPath;
    }

    private const string CREATE_SCHEMA_SQL =
@"
CREATE TABLE [Rooms] (
    [RoomCode] varchar(20) PRIMARY KEY NOT NULL,
    [Description] varchar(1024)
);

CREATE TABLE [StudentInfo] (
    [RunId] int NOT NULL,
    [StudentId] int NOT NULL,
    [FirstName] varchar(200) NOT NULL,
    [LastName] varchar(200) NOT NULL,
    [Grade] varchar(10) NOT NULL,
    [Gender] varchar(10) NOT NULL,
    [Homeroom] varchar(50) NOT NULL,
    PRIMARY KEY ([RunId], [StudentId])
);

CREATE TABLE [StudentCourseRanking] (
    [RunId] int NOT NULL,
    [StudentId] int NOT NULL,
    [CourseCode] varchar(20) NOT NULL,
    [Rank] int NOT NULL,
    PRIMARY KEY ([RunId], [StudentId], [CourseCode])
);

CREATE TABLE [CourseInfo] (
    [RunId] int NOT NULL,
    [CourseCode] varchar(20) NOT NULL,
    [CourseName] varchar(250) NOT NULL,
    [MinCapacity] int,
    [MaxCapacity] int,
    [Description] varchar(1024),
    [MinGrade] int NOT NULL,
    [MaxGrade] int  NOT NULL,
    [RoomCode] varchar(20),
    PRIMARY KEY ([RunId], [CourseCode])
);

CREATE TABLE [CourseSchedule] (
    [RunId] int NOT NULL,
    [CourseCode] varchar(20) NOT NULL,
    [SessionId] int NOT NULL,
    [Instructor] varchar(200),
    [Supervisor] varchar(200),
    PRIMARY KEY ([RunId], [CourseCode], [SessionId])
);

CREATE TABLE [RunInfo] (
    [RunId] int PRIMARY KEY NOT NULL,
    [RunTimeStamp] DateTime2 NOT NULL,
    [ManuallyCreatedResults] bit NOT NULL,
    [Name] varchar(100) NOT NULL,
    [Notes] varchar(4096) NULL
);

CREATE TABLE [CourseAssignmentResult] (
    [RunId] int NOT NULL,
    [StudentId] int NOT NULL,
    [SessionId] int NOT NULL,
    [CourseCode] varchar(20) NOT NULL,
    PRIMARY KEY ([RunId], [StudentId], [SessionId])
);

CREATE TABLE [Settings] (
    [Name] varchar(50) PRIMARY KEY NOT NULL,
    [Value] varchar(200) NOT NULL,
    [Description] varchar(200) NOT NULL,
    [IsWritable] bit NOT NULL
);
";
}