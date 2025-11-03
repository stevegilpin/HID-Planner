using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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
    public static string GetSupervisorsCsvInfoText() => GetCsvInfoText<Supervisor, CsvMappers.SupervisorInfoMapper>("the supervisors");
    public static string GetInstructorsCsvInfoText() => GetCsvInfoText<Instructor, CsvMappers.InstructorInfoMapper>("the instructors");

    public static string GetSurveyResponsesCsvInfoText()
    {
        return $"This is a CSV file containing the survey responses.{Environment.NewLine}{Environment.NewLine}These columns are required: {string.Join(", ", CsvReadUtil.SurveyResponseHeaders())}";
    }

    public static string GetSchedulingResultsCsvInfoText()
    {
        var preamble = "This file is only required to save manually created scheduling results.";
        var defaultText = GetCsvInfoText<CourseAssignmentResult, CsvMappers.CourseAssignmentResultsMapper>("the student / course scheduling results");
        return $"{preamble}{Environment.NewLine}{Environment.NewLine}{defaultText}";
    }
    
    public static string GetCsvInfoText<TFile, TMap>(string fileDataDescription)
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
            if (member.GetCustomAttribute<NotMappedAttribute>() != null)
                continue;
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

        var columnDescriptions = allMembers.Select(x => new {
                x.Name,
                x.GetCustomAttribute<DescriptionAttribute>()?.Description,
            })
            .Where(x => string.IsNullOrWhiteSpace(x.Description) == false)
            .ToList();
        if (columnDescriptions.Any()) {
            infoText += $"{Environment.NewLine}{Environment.NewLine}Column Descriptions:{Environment.NewLine}";
            infoText += string.Join(Environment.NewLine, columnDescriptions.Select(x => $"{x.Name}: {x.Description}"));
        }
        
        return infoText;
    }
}