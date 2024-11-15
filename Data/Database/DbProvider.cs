using System;
using System.IO;
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
        Console.WriteLine("Database created");
    }

    public static bool TryDeleteDatabase(bool saveBackup, out string backupPath)
    {
        backupPath = default;
        var dbFilePath = GetDbFilePath();
        if (File.Exists(dbFilePath) == false)
            return false;

        if (saveBackup) {
            backupPath = Path.Combine(DB_FOLDER, $"{DB_FILE_NAME}-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}");
            File.Move(dbFilePath, backupPath);
        } else
            File.Delete(dbFilePath);

        return true;
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
    [GradesFlags] int,
    PRIMARY KEY ([RunId], [CourseCode])
);

CREATE TABLE [CourseSchedule] (
    [RunId] int NOT NULL,
    [CourseCode] varchar(20) NOT NULL,
    [SessionId] int NOT NULL,
    [RoomCode] varchar(20),
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
);";
}