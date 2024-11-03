using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Data.Objects;

namespace HIDLib;

public static class CsvUtil
{
    public static bool HeaderMatchesCourseInfo(string filePath)
    {
        return HeaderMatches<CourseInfo, Mappers.CourseInfoMapper>(filePath);
    }

    public static bool HeaderMatchesCourseSchedule(string filePath)
    {
        return HeaderMatches<CourseSchedule, Mappers.CourseScheduleMapper>(filePath);
    }
    
    public static bool HeaderMatchesStudentInfo(string filePath)
    {
        return HeaderMatches<StudentInfo, Mappers.StudentInfoMapper>(filePath);
    }

    public static bool HeaderMatchesStudentCourseRankings(string filePath)
    {
        return HeaderMatches<StudentCourseRanking, Mappers.StudentCourseRankingMapper>(filePath);
    }

    private static bool HeaderMatches<TTable, TMapper>(string filePath) where TMapper : ClassMap<TTable>
    {
        if (File.Exists(filePath) == false)
            return false;

        bool headerOk = true;
        using var reader = new StreamReader(filePath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HeaderValidated = args => headerOk = args.InvalidHeaders.Length == 0
        };
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
        using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<Mappers.CourseInfoMapper>();
        
        csv.Read();
        csv.ReadHeader();

        var data = new List<CourseInfo>();
        while (csv.Read()) {
            var record = csv.GetRecord<CourseInfo>()!;
            data.Add(record);
            var offeredGrades = Grades.None;
            foreach (var grade in Enum.GetValues<Grades>()) {
                if (IsCourseOfferedToGrade(csv, grade))
                    offeredGrades |= grade;
            }

            record.GradesFlags = (int)offeredGrades;
        }

        return data;

        static bool IsCourseOfferedToGrade(CsvReader reader, Grades grade)
        {
            object val;
            if (reader.TryGetField(typeof(string), grade.ToString(), out val) == false) {
                if (reader.TryGetField(typeof(string), $"{grade} Grade", out val) == false)
                    return false;
            }
            
            return bool.TryParse((string)val, out bool isOffered) && isOffered;
        }
    }

    public static List<StudentInfo> ReadStudentInfo(string filePath) => ReadFile<StudentInfo, Mappers.StudentInfoMapper>(filePath);

    public static List<StudentCourseRanking> ReadStudentCourseRankings(string filePath)
    {
        return ReadFile<StudentCourseRanking, Mappers.StudentCourseRankingMapper>(filePath);
    }

    public static List<CourseSchedule> ReadCourseSchedule(string filePath)
    {
        return ReadFile<CourseSchedule, Mappers.CourseScheduleMapper>(filePath);
    }

    public static List<CourseAssignmentResult> ReadAssignmentResults(string filePath)
    {
        return ReadFile<CourseAssignmentResult, Mappers.CourseAssignmentResultsMapper>(filePath);
    }
    
    private static List<TData> ReadFile<TData, TMapper>(string filePath) where TMapper : ClassMap<TData>
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<TMapper>();

        var records = csv.GetRecords<TData>().ToList();

        return records;
    }

    public static void WriteResultsToCsv(AssignmentDataSet data, string folder)
    {
        var courseSchedule = data.InputData.CourseSchedule.OrderBy(x => x.Key).SelectMany(x => x.Value.OrderBy(c => c.SessionId)).ToList();
        WriteCsv(courseSchedule, Path.Combine(folder, "Inputs", "CourseSchedule.csv"));

        var studentInfo = data.InputData.StudentInfo.Values.OrderBy(x => x.StudentId).ToList();
        WriteCsv(studentInfo, Path.Combine(folder, "Inputs", "StudentInfo.csv"));

        var studentRankings = data.InputData.StudentCourseRanking.OrderBy(x => x.Key).SelectMany(x => x.Value.OrderBy(c => c.Rank)).ToList();
        WriteCsv(studentRankings, Path.Combine(folder, "Inputs", "StudentCourseRankings.csv"));

        var courseInfo = data.InputData.CourseInfo.Values.OrderBy(x => x.CourseCode).ToList();
        WriteCsv(courseInfo, Path.Combine(folder, "Inputs", "CourseInfo.csv"));

        var assignments = data.AssignmentResults.OrderBy(x => x.StudentId).ThenBy(x => x.SessionId).ToList();
        WriteCsv(assignments, Path.Combine(folder, "CourseAssignments.csv"));
    }
    
    private static void WriteCsv<T>(List<T> data, string filePath)
    {
        Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);
        using var stream = new StreamWriter(filePath);
        using var writer = new CsvWriter(stream, CultureInfo.CurrentCulture);
        writer.WriteHeader<T>();
        writer.NextRecord();
        writer.WriteRecords(data);
    }

    private static class Mappers
    {
        public sealed class CourseInfoMapper : ClassMap<CourseInfo>
        {
            public CourseInfoMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.RunId).Ignore();
                Map(x => x.MinCapacity).Optional();
                Map(x => x.MaxCapacity).Optional();
                Map(x => x.GradesFlags).Ignore();
            }
        }

        public sealed class StudentInfoMapper : ClassMap<StudentInfo>
        {
            public StudentInfoMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.RunId).Ignore();
                Map(x => x.StudentId).Optional();
            }
        }

        public sealed class StudentCourseRankingMapper : ClassMap<StudentCourseRanking>
        {
            public StudentCourseRankingMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.RunId).Ignore();
            }
        }
        
        public sealed class CourseScheduleMapper : ClassMap<CourseSchedule>
        {
            public CourseScheduleMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.RunId).Ignore();
                Map(x => x.RoomCode).Optional();
                Map(x => x.Instructor).Optional();
                Map(x => x.Supervisor).Optional();
            }
        }

        public sealed class CourseAssignmentResultsMapper : ClassMap<CourseAssignmentResult>
        {
            public CourseAssignmentResultsMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.RunId).Ignore();
            }
        }
    }
}