using Database.Objects;
using Microsoft.Data.Sqlite;

namespace Database;

public static class DbConnection
{
    private static readonly string DB_FOLDER = @"C:/sql/"; // TODO change to somewhere out of the way. Determine at runtime based on OS.
    private const string DB_FILE_NAME = "HID_Planner_db";
    
    public static HidDbContext GetDbContext()
    {
        var dbFilePath = GetDbFilePath();
        if (File.Exists(dbFilePath) == false)
            CreateDatabase();

        return new HidDbContext();
    }

    private static string GetDbFilePath()
    {
        return Path.Combine(DB_FOLDER, DB_FILE_NAME);
    }

    private static void CreateDatabase()
    {
        var dbFilePath = GetDbFilePath();
        Directory.CreateDirectory(DB_FOLDER);
        using var connection = new SqliteConnection($"Data Source={dbFilePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = CREATE_SCHEMA_SQL;
        command.ExecuteNonQuery();
    }

    private const string CREATE_SCHEMA_SQL =
        @"
CREATE TABLE [Rooms] (
  [RoomCode] varchar(20) PRIMARY KEY NOT NULL,
  [Description] nvarchar(255)
);

CREATE TABLE [StudentInfo] (
  [StudentId] int PRIMARY KEY NOT NULL,
  [Name] varchar(200) NOT NULL,
  [Grade] varchar(10) NOT NULL,
  [Gender] varchar(20)
);

CREATE TABLE [StudentRankingInputDescription] (
  [InputId] int PRIMARY KEY NOT NULL,
  [EventId] varchar(50) NOT NULL,
  [Description] nvarchar(255)
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
  [CourseName] varchar(250) NOT NULL,
  [Description] nvarchar(255),
  [GradesFlags] int NOT NULL,
  PRIMARY KEY ([CourseCode], [EventId])
);

CREATE TABLE [CourseScheduleInputDescription] (
  [InputId] int PRIMARY KEY NOT NULL,
  [EventId] varchar(50) NOT NULL,
  [Description] nvarchar(255)
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
  [Description] nvarchar(255)
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