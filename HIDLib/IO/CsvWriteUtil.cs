using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Data.Objects;

namespace HIDLib.IO;

public static class CsvWriteUtil
{
    public static void WriteResultsToCsv(AssignmentDataSet data, string folder)
    {
        var courseSchedule = data.InputData.CourseSchedule.OrderBy(x => x.Key).SelectMany(x => x.Value.OrderBy(c => c.SessionId)).ToList();
        WriteCsv<CourseSchedule, CsvMappers.CourseScheduleMapper>(courseSchedule, Path.Combine(folder, "Inputs", "CourseSchedule.csv"));

        var studentInfo = data.InputData.StudentInfo.Values.OrderBy(x => x.StudentId).ToList();
        WriteCsv<StudentInfo, CsvMappers.StudentInfoMapper>(studentInfo, Path.Combine(folder, "Inputs", "StudentInfo.csv"));

        var studentRankings = data.InputData.StudentCourseRanking.OrderBy(x => x.Key).SelectMany(x => x.Value.OrderBy(c => c.Rank)).ToList();
        WriteCsv<StudentCourseRanking, CsvMappers.StudentCourseRankingMapper>(studentRankings, Path.Combine(folder, "Inputs", "StudentCourseRankings.csv"));

        var courseInfo = data.InputData.CourseInfo.Values.OrderBy(x => x.CourseCode).ToList();
        WriteCsv<CourseInfo, CsvMappers.CourseInfoMapper>(courseInfo, Path.Combine(folder, "Inputs", "CourseInfo.csv"));

        var assignments = data.AssignmentResults.OrderBy(x => x.StudentId).ThenBy(x => x.SessionId).ToList();
        WriteCsv<CourseAssignmentResult, CsvMappers.CourseAssignmentResultsMapper>(assignments, Path.Combine(folder, "CourseAssignments.csv"));
    }

    public static void WriteCourseRankings(List<StudentCourseRanking> courseRankings, string folder)
    {
        courseRankings = courseRankings.OrderBy(c => c.StudentId).ThenBy(x => x.Rank).ToList();
        WriteCsv<StudentCourseRanking, CsvMappers.StudentCourseRankingMapper>(courseRankings, Path.Combine(folder, "StudentCourseRankings.csv"));
    }

    public static void WriteStudentInfo(List<StudentInfo> studentInfo, string folder)
    {
        studentInfo = studentInfo.OrderBy(x => x.StudentId).ToList();
        WriteCsv<StudentInfo, CsvMappers.StudentInfoMapper>(studentInfo, Path.Combine(folder, "StudentInfo.csv"));
    }
    private static void WriteCsv<TData, TMapper>(List<TData> data, string filePath) where TMapper : ClassMap<TData>
    {
        Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);
        using var stream = new StreamWriter(filePath);
        using var writer = new CsvWriter(stream, CultureInfo.CurrentCulture);
        writer.Context.RegisterClassMap<TMapper>();
        writer.WriteHeader<TData>();
        writer.NextRecord();
        writer.WriteRecords(data);
    }    
}