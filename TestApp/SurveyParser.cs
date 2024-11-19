using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Objects;

namespace TestApp;

public record SurveyResponse(string LastName, string FirstName, int Grade, string Email, List<string> ClassRankings);

public static class SurveyParser
{
    public static List<SurveyResponse> ParseResults(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var header = lines[0].Split(',').ToList();
        var lastNameIndex = header.IndexOf("Last Name");
        var firstNameIndex = header.IndexOf("First Name");
        var gradeIndex = header.IndexOf("Grade");
        var emailIndex = header.IndexOf("Email Address");

        var rowDataStart = new[] { lastNameIndex, firstNameIndex, gradeIndex, emailIndex }.Max() + 1;
        var responses = new List<SurveyResponse>();
        foreach (var line in lines.Skip(1)) {
            var data = line.Split(',').ToList();
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

    public static List<StudentCourseRanking> ToSchedulingInput(List<SurveyResponse> surveyData, List<CourseInfo> courseData, List<StudentInfo> studentData)
    {
        var rankings = new List<StudentCourseRanking>();
        var studentInfoMap = studentData.ToDictionary(x => (x.LastName, x.FirstName));
        foreach (var response in surveyData) {
            var studentInfo = studentInfoMap[(response.LastName, response.FirstName)];
            var grade = GradeToInt(studentInfo.Grade);
            var eligibleCourses = courseData.Where(x => x.MinGrade <= grade && x.MaxGrade >= grade).ToDictionary(x => x.CourseName);
            rankings.AddRange(response.ClassRankings.Select((x, i) => new StudentCourseRanking {
                CourseCode = eligibleCourses[x].CourseCode,
                Rank = i + 1,
                StudentId = studentInfo.StudentId,
            }));
        }

        return rankings;
    }

    private static int GradeToInt(string gradeCode)
    {
        if (int.TryParse(gradeCode, out var grade))
            return grade;

        return int.Parse(gradeCode!.Replace("K", string.Empty)) - 5;
    }
}