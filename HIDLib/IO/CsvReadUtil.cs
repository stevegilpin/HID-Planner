using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Data.Objects;

namespace HIDLib.IO;

public static class CsvReadUtil
{
    public static bool HeaderMatchesCourseInfo(string filePath)
    {
        return HeaderMatches<CourseInfo, CsvMappers.CourseInfoMapper>(filePath);
    }

    public static bool HeaderMatchesCourseSchedule(string filePath)
    {
        return HeaderMatches<CourseSchedule, CsvMappers.CourseScheduleMapper>(filePath) &&
               HeaderMatches<CourseAssignmentResult, CsvMappers.CourseAssignmentResultsMapper>(filePath) == false;
    }
    
    public static bool HeaderMatchesStudentInfo(string filePath)
    {
        return HeaderMatches<StudentInfo, CsvMappers.StudentInfoMapper>(filePath);
    }

    public static bool HeaderMatchesStudentCourseRankings(string filePath)
    {
        return HeaderMatches<StudentCourseRanking, CsvMappers.StudentCourseRankingMapper>(filePath);
    }

    public static bool HeaderMatchesSchedulingResults(string filePath)
    {
        return HeaderMatches<CourseAssignmentResult, CsvMappers.CourseAssignmentResultsMapper>(filePath);
    }

    public static List<string> SurveyResponseHeaders() => [
        "Student Last Name",
        "Student First Name",
        "Grade",
        "Username",
    ];
    public static bool HeaderMatchesSurveyResponses(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var header = lines[0].Split(',').ToList();
        return SurveyResponseHeaders().All(x => header.Contains(x));
    }

    public static bool HeaderMatchesRooms(string filePath)
    {
        return HeaderMatches<Rooms, CsvMappers.RoomDescriptionsMapper>(filePath);
    }

    private static bool HeaderMatches<TTable, TMapper>(string filePath) where TMapper : ClassMap<TTable>
    {
        if (File.Exists(filePath) == false)
            return false;

        bool headerOk = true;
        using var reader = new StreamReader(filePath);
        var config = GetCsvReaderConfig();
        config.HeaderValidated = args => headerOk = args.InvalidHeaders.Length == 0;

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<TMapper>();
        csv.Read();
        csv.ReadHeader();
        csv.ValidateHeader(typeof(TTable));
        return headerOk;
    }
    
    public static List<CourseInfo> ReadCourseInfo(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, GetCsvReaderConfig());
        csv.Context.RegisterClassMap<CsvMappers.CourseInfoMapper>();

        csv.Read();
        csv.ReadHeader();

        var data = new List<CourseInfo>();
        while (csv.Read()) {
            var record = csv.GetRecord<CourseInfo>();
            data.Add(record);
            if (record.MinGrade > record.MaxGrade)
                throw new Exception($"Error parsing course info on line {csv.Parser.Row}. Min grade is greater than max grade.{Environment.NewLine}{Environment.NewLine}{csv.Parser.RawRecord}");
        }

        return data;
    }

    public static List<StudentInfo> ReadStudentInfo(string filePath) => ReadFile<StudentInfo, CsvMappers.StudentInfoMapper>(filePath);

    public static List<StudentCourseRanking> ReadStudentCourseRankings(string filePath)
    {
        return ReadFile<StudentCourseRanking, CsvMappers.StudentCourseRankingMapper>(filePath);
    }

    public static List<CourseSchedule> ReadCourseSchedule(string filePath)
    {
        return ReadFile<CourseSchedule, CsvMappers.CourseScheduleMapper>(filePath);
    }

    public static List<CourseAssignmentResult> ReadAssignmentResults(string filePath)
    {
        return ReadFile<CourseAssignmentResult, CsvMappers.CourseAssignmentResultsMapper>(filePath);
    }

    public static List<SurveyResponse> ReadSurveyResponses(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var header = lines[0].Split(',').ToList();
        var lastNameIndex = header.IndexOf("Student Last Name");
        var firstNameIndex = header.IndexOf("Student First Name");
        var gradeIndex = header.IndexOf("Grade");
        var emailIndex = header.IndexOf("Username");

        var rowDataStart = new[] { lastNameIndex, firstNameIndex, gradeIndex, emailIndex }.Max() + 1;
        var responses = new List<SurveyResponse>();
        foreach (var line in lines.Skip(1)) {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var data = line.Split(',').Select(x => x.Trim()).ToList();
            var rankings = data
                .Where((_, i) => i >= rowDataStart)
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .ToList();

            responses.Add(new SurveyResponse(
                data[lastNameIndex],
                data[firstNameIndex],
                int.Parse(data[gradeIndex]),
                data[emailIndex],
                rankings));
        }

        return responses;
    }
    
    public static List<Rooms> ReadRoomDescriptions(string filePath) => ReadFile<Rooms, CsvMappers.RoomDescriptionsMapper>(filePath);
    public static List<Supervisor> ReadSupervisorInfo(string filePath) => ReadFile<Supervisor, CsvMappers.SupervisorInfoMapper>(filePath);
    public static List<Instructor> ReadInstructorInfo(string filePath) => ReadFile<Instructor, CsvMappers.InstructorInfoMapper>(filePath);
    
    public static List<TData> ReadFile<TData, TMapper>(string filePath) where TMapper : ClassMap<TData>
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, GetCsvReaderConfig());
        csv.Context.RegisterClassMap<TMapper>();

        var records = csv.GetRecords<TData>().ToList();

        return records;
    }

    private static CsvConfiguration GetCsvReaderConfig()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture) {
            IgnoreBlankLines = true,
            ExceptionMessagesContainRawData = true,
            ShouldSkipRecord = args => string.IsNullOrWhiteSpace(args.Row.Parser.RawRecord.Replace(args.Row.Parser.Delimiter, string.Empty)),
            PrepareHeaderForMatch = args => args.Header?.Replace(" ", string.Empty).ToLower(),
        };
    }

    public static int GradeToInt(string gradeCode)
    {
        if (int.TryParse(gradeCode, out var grade))
            return grade;

        return int.Parse(gradeCode!.Replace("K", string.Empty)) - 5;
    }
}