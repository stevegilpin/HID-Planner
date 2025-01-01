using System;
using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.IO;

namespace HIDUI.BusinessLogic;

public class SurveyReconController
{
    public string SurveyResponsesFilePath { get; private set; }
    public string StudentInfoFilePath { get; private set; }
    public string CourseInfoFilePath { get; private set; }

    private List<SurveyResponse> SurveyData;
    private List<StudentInfo> StudentData;
    private List<CourseInfo> CourseData;

    public List<NoResponseFailure> NoResponseFailures { get; private set; } = [];
    public List<UnexpectedRespondentFailure> UnexpectedRespondentFailures { get; private set; } = [];
    public List<DuplicateResponseFailure> DuplicateResponseFailures { get; private set; } = [];
    public List<InvalidCourseChoiceFailure> InvalidCourseChoiceFailures { get; private set; } = [];
    public List<IncompleteResponseFailure> IncompleteResponseFailures { get; private set; } = [];

    public void SetFilePaths(string[] paths)
    {
        SurveyResponsesFilePath = ControllerUtils.GetMatchingPath(paths, SurveyResponsesFilePath, CsvReadUtil.HeaderMatchesSurveyResponses);
        CourseInfoFilePath = ControllerUtils.GetMatchingPath(paths, CourseInfoFilePath, CsvReadUtil.HeaderMatchesCourseInfo);
        StudentInfoFilePath = ControllerUtils.GetMatchingPath(paths, StudentInfoFilePath, CsvReadUtil.HeaderMatchesStudentInfo);
    }

    public void LoadAllFailures()
    {
        SurveyData = SurveyResponsesInputReady() ? CsvReadUtil.ReadSurveyResponses(SurveyResponsesFilePath) : null;
        StudentData = StudentInfoInputReady() ? CsvReadUtil.ReadStudentInfo(StudentInfoFilePath) : null;
        CourseData = CourseInfoInputReady() ? CsvReadUtil.ReadCourseInfo(CourseInfoFilePath) : null;
        
        NoResponseFailures = CanLoadNoResponseFailures() ? LoadNoResponseFailures(SurveyData, StudentData) : [];
        UnexpectedRespondentFailures = CanLoadUnexpectedRespondentFailures() ? LoadUnexpectedRespondentFailures(SurveyData, StudentData) : [];
        DuplicateResponseFailures = CanLoadDuplicateResponseFailures() ? LoadDuplicateResponseFailures(SurveyData) : [];
        InvalidCourseChoiceFailures = CanLoadInvalidCourseChoiceFailures() ? LoadInvalidCourseChoiceFailures(SurveyData, CourseData) : [];
        IncompleteResponseFailures = CanLoadIncompleteResponseFailures() ? LoadIncompleteResponseFailures(SurveyData) : [];
    }

    public bool CanLoadNoResponseFailures() => SurveyResponsesInputReady() && StudentInfoInputReady();
    private static List<NoResponseFailure> LoadNoResponseFailures(List<SurveyResponse> surveyData, List<StudentInfo> studentData)
    {
        var failures = new List<NoResponseFailure>();
        var respondents = surveyData.Select(x => (x.FirstName.ToUpper(), x.LastName.ToUpper(), x.Grade)).ToHashSet();
        var responsesByGrade = surveyData.GroupBy(x => x.Grade).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var student in studentData) {
            var grade = GradeToInt(student.Grade);
            var key = (student.FirstName.ToUpper(), student.LastName.ToUpper(), grade);
            if (respondents.Contains(key))
                continue;

            var possibleMatches = responsesByGrade.GetValueOrDefault(grade, []).Where(x => IsPossibleNameMatch(x, student)).ToList();
            failures.Add(new NoResponseFailure(student, possibleMatches));
        }

        return failures;
    }

    public bool CanLoadUnexpectedRespondentFailures() => SurveyResponsesInputReady() && StudentInfoInputReady();
    private static List<UnexpectedRespondentFailure> LoadUnexpectedRespondentFailures(List<SurveyResponse> surveyData, List<StudentInfo> studentData)
    {
        var failures = new List<UnexpectedRespondentFailure>();
        var students = studentData.Select(x => (x.FirstName.ToUpper(), x.LastName.ToUpper(), GradeToInt(x.Grade))).ToHashSet();
        var studentsByGrade = studentData.GroupBy(x => x.Grade).ToDictionary(g => GradeToInt(g.Key), g => g.ToList());
        foreach (var response in surveyData) {
            var key = (response.FirstName.ToUpper(), response.LastName.ToUpper(), response.Grade);
            if (students.Contains(key))
                continue;

            var possibleMatches = studentsByGrade.GetValueOrDefault(response.Grade, []).Where(x => IsPossibleNameMatch(response, x)).ToList();
            failures.Add(new UnexpectedRespondentFailure(response, possibleMatches));
        }

        return failures;
    }

    public bool CanLoadDuplicateResponseFailures() => SurveyResponsesInputReady();
    private static List<DuplicateResponseFailure> LoadDuplicateResponseFailures(List<SurveyResponse> surveyData)
    {
        return surveyData
            .GroupBy(x => (x.FirstName.ToUpper(), x.LastName.ToUpper(), x.Grade))
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateResponseFailure(g.ToList()))
            .ToList();
    }

    public bool CanLoadInvalidCourseChoiceFailures() => SurveyResponsesInputReady() && CourseInfoInputReady();
    private static List<InvalidCourseChoiceFailure> LoadInvalidCourseChoiceFailures(List<SurveyResponse> surveyData, List<CourseInfo> courseData)
    {
        var failures = new List<InvalidCourseChoiceFailure>();
        foreach (var response in surveyData) {
            var eligibleCourses = courseData
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

    public bool CanLoadIncompleteResponseFailures() => SurveyResponsesInputReady();
    private static List<IncompleteResponseFailure> LoadIncompleteResponseFailures(List<SurveyResponse> surveyData)
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
    
    public bool CanSaveAsSchedulingInput() => SurveyResponsesInputReady() && StudentInfoInputReady() && CourseInfoInputReady();
    public void SaveAsSchedulingInput(string folder)
    {
        var surveyData = CsvReadUtil.ReadSurveyResponses(SurveyResponsesFilePath);
        var studentData = CsvReadUtil.ReadStudentInfo(StudentInfoFilePath);
        var courseData = CsvReadUtil.ReadCourseInfo(CourseInfoFilePath);

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

        CsvWriteUtil.WriteCourseRankings(rankings, folder);
    }
    
    private bool SurveyResponsesInputReady() => string.IsNullOrEmpty(SurveyResponsesFilePath) == false;
    private bool StudentInfoInputReady() => string.IsNullOrEmpty(StudentInfoFilePath) == false;
    private bool CourseInfoInputReady() => string.IsNullOrEmpty(CourseInfoFilePath) == false;

    private static bool IsPossibleNameMatch(SurveyResponse surveyResponse, StudentInfo student)
    {
        return surveyResponse.LastName.ToUpper() == student.LastName.ToUpper();
    }
    
    private static int GradeToInt(string gradeCode)
    {
        if (int.TryParse(gradeCode, out var grade))
            return grade;

        return int.Parse(gradeCode!.Replace("K", string.Empty)) - 5;
    }
}

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