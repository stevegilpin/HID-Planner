using System.Collections.Generic;
using System.Linq;
using Data.Objects;

namespace TestApp;

public static class SurveyRecon
{
    /// <summary> Students for whom a matching response was not found </summary>
    public record NoResponseFailure(StudentInfo Student, List<SurveyResponse> PossibleMatches);
    /// <summary> Responses for which a matching student was not found </summary>
    public record UnexpectedRespondentFailure(SurveyResponse Response, List<StudentInfo> PossibleMatches);
    /// <summary> Multiple responses for the same student </summary>
    public record DuplicateResponseFailure(List<SurveyResponse> Responses);
    /// <summary> Responses ranking a course which is not available to the student </summary>
    public record InvalidCourseChoiceFailure(SurveyResponse Response, string InvalidCourseChoice);
    /// <summary> Responses with fewer course rankings than other students in the same grade </summary>
    public record IncompleteResponseFailure(SurveyResponse Response, int ExpectedRankingsCount);


    public static List<object> GetReconFailures(List<SurveyResponse> surveyData, List<CourseInfo> courseInfo, List<StudentInfo> studentInfo)
    {
        var failures = new List<object>();
        failures.AddRange(GetNoResponseFailures(surveyData, studentInfo));
        failures.AddRange(GetUnexpectedRespondentFailures(surveyData, studentInfo));
        failures.AddRange(GetDuplicateResponseFailures(surveyData));
        failures.AddRange(GetInvalidCourseChoiceFailures(surveyData, courseInfo));
        failures.AddRange(GetIncompleteResponseFailures(surveyData));
        return failures;
    }

    private static bool IsPossibleNameMatch(SurveyResponse surveyResponse, StudentInfo student)
    {
        return surveyResponse.LastName.ToUpper() == student.LastName.ToUpper();
    }

    private static List<NoResponseFailure> GetNoResponseFailures(List<SurveyResponse> surveyData, List<StudentInfo> studentInfo)
    {
        var failures = new List<NoResponseFailure>();
        var respondents = surveyData.Select(x => (x.FirstName.ToUpper(), x.LastName.ToUpper(), x.Grade)).ToHashSet();
        var responsesByGrade = surveyData.GroupBy(x => x.Grade).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var student in studentInfo) {
            var grade = SurveyParser.GradeToInt(student.Grade);
            var key = (student.FirstName.ToUpper(), student.LastName.ToUpper(), grade);
            if (respondents.Contains(key))
                continue;

            var possibleMatches = responsesByGrade.GetValueOrDefault(grade, []).Where(x => IsPossibleNameMatch(x, student)).ToList();
            failures.Add(new NoResponseFailure(student, possibleMatches));
        }

        return failures;
    }

    private static List<UnexpectedRespondentFailure> GetUnexpectedRespondentFailures(List<SurveyResponse> surveyData, List<StudentInfo> studentInfo)
    {
        var failures = new List<UnexpectedRespondentFailure>();
        var students = studentInfo.Select(x => (x.FirstName.ToUpper(), x.LastName.ToUpper(), SurveyParser.GradeToInt(x.Grade))).ToHashSet();
        var studentsByGrade = studentInfo.GroupBy(x => x.Grade).ToDictionary(g => SurveyParser.GradeToInt(g.Key), g => g.ToList());
        foreach (var response in surveyData) {
            var key = (response.FirstName.ToUpper(), response.LastName.ToUpper(), response.Grade);
            if (students.Contains(key))
                continue;

            var possibleMatches = studentsByGrade.GetValueOrDefault(response.Grade, []).Where(x => IsPossibleNameMatch(response, x)).ToList();
            failures.Add(new UnexpectedRespondentFailure(response, possibleMatches));
        }

        return failures;
    }

    private static List<DuplicateResponseFailure> GetDuplicateResponseFailures(List<SurveyResponse> surveyData)
    {
        return surveyData
            .GroupBy(x => (x.FirstName.ToUpper(), x.LastName.ToUpper(), x.Grade))
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateResponseFailure(g.ToList()))
            .ToList();
    }

    private static List<InvalidCourseChoiceFailure> GetInvalidCourseChoiceFailures(List<SurveyResponse> surveyData, List<CourseInfo> courseInfo)
    {
        var failures = new List<InvalidCourseChoiceFailure>();
        foreach (var response in surveyData) {
            var eligibleCourses = courseInfo
                .Where(x => x.MinGrade <= response.Grade && x.MaxGrade >= response.Grade)
                .Select(x => x.CourseName)
                .ToHashSet();

            foreach (var course in response.ClassRankings) {
                if (eligibleCourses.Contains(course) == false)
                    failures.Add(new InvalidCourseChoiceFailure(response, course));
            }
        }

        return failures;
    }

    private static List<IncompleteResponseFailure> GetIncompleteResponseFailures(List<SurveyResponse> surveyData)
    {
        var failures = new List<IncompleteResponseFailure>();
        var gradeGroups = surveyData.GroupBy(x => x.Grade).ToList();
        foreach (var group in gradeGroups)
        {
            var maxRankingCount = group.Select(x => x.ClassRankings.Count).Max();
            var incomplete = group.Where(x => x.ClassRankings.Count < maxRankingCount).ToList();
            if (incomplete.Count > 0)
                failures.AddRange(incomplete.Select(x => new IncompleteResponseFailure(x, maxRankingCount)));
        }

        return failures;
    }
}