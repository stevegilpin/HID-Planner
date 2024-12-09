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
        return HeaderMatches<CourseSchedule, CsvMappers.CourseScheduleMapper>(filePath);
    }
    
    public static bool HeaderMatchesStudentInfo(string filePath)
    {
        return HeaderMatches<StudentInfo, CsvMappers.StudentInfoMapper>(filePath);
    }

    public static bool HeaderMatchesStudentCourseRankings(string filePath)
    {
        return HeaderMatches<StudentCourseRanking, CsvMappers.StudentCourseRankingMapper>(filePath);
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
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<CsvMappers.CourseInfoMapper>();

        csv.Read();
        csv.ReadHeader();

        var data = new List<CourseInfo>();
        while (csv.Read())
            data.Add(csv.GetRecord<CourseInfo>());

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
    
    private static List<TData> ReadFile<TData, TMapper>(string filePath) where TMapper : ClassMap<TData>
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<TMapper>();

        var records = csv.GetRecords<TData>().ToList();

        return records;
    }
}