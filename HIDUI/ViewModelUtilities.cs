using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;
using Data.Objects;
using HIDLib.IO;

namespace HIDUI;

public static class ViewModelUtilities
{
    public static string GetCoursesCsvInfoText() => GetCsvInfoText<CourseInfo, CsvMappers.CourseInfoMapper>("the available courses");
    public static string GetCourseScheduleCsvInfoText() => GetCsvInfoText<CourseSchedule, CsvMappers.CourseScheduleMapper>("the schedule of courses");
    public static string GetStudentsCsvInfoText() => GetCsvInfoText<StudentInfo, CsvMappers.StudentInfoMapper>("the students");
    public static string GetStudentChoicesCsvInfoText() => GetCsvInfoText<StudentCourseRanking, CsvMappers.StudentCourseRankingMapper>("the students' course rankings");
    public static string GetRoomsDescriptionCsvInfoText() => GetCsvInfoText<Rooms, CsvMappers.RoomDescriptionsMapper>("the course descriptions");

    public static string GetSurveyResponsesCsvInfoText()
    {
        return $"This is a CSV file containing the survey responses.{Environment.NewLine}{Environment.NewLine}These columns are required: Student First Name, Student Last Name, Grade, Email Address";
    }

    public static string GetSchedulingResultsCsvInfoText()
    {
        var preamble = "This file is only required to save manually created scheduling results.";
        var defaultText = GetCsvInfoText<CourseAssignmentResult, CsvMappers.CourseAssignmentResultsMapper>("the student / course scheduling results");
        return $"{preamble}{Environment.NewLine}{Environment.NewLine}{defaultText}";
    }
    
    private static string GetCsvInfoText<TFile, TMap>(string fileDataDescription)
        where TMap : ClassMap<TFile>, new()
    {
        var classMap = new TMap();
        var allMembers = new List<MemberInfo>();
        allMembers.AddRange(typeof(TFile).GetProperties());
        allMembers.AddRange(typeof(TFile).GetFields());
        var optional = new List<string>();
        var required = new List<string>();
        foreach (var member in allMembers) {
            var memberMap = classMap.Map(typeof(TFile), member).Data;
            if (memberMap.Ignore)
                continue;
            if (memberMap.IsOptional)
                optional.Add(member.Name);
            else
                required.Add(member.Name);
        }
        
        var infoText = $"This is a CSV file containing {fileDataDescription}{Environment.NewLine}{Environment.NewLine}These columns are required: ";
        infoText += string.Join(", ", required.OrderBy(x => x));
        if (optional.Any()) {
            infoText += $"{Environment.NewLine}{Environment.NewLine}These columns are optional: ";
            infoText += string.Join(", ", optional.OrderBy(x => x));
        }
        
        return infoText;
    }
}