using System;
using System.Linq;
using Data.Objects;
using HIDLib;
using HIDLib.EndPoints;
using Serilog;

namespace TestApp;

public class SchedulingController
{
    public ILogger Logger { get; set; } = new LoggerConfiguration().WriteTo.Console().CreateLogger();

    public string RunName { get; set; }
    public string CourseInfoFilePath { get; set; }
    public string CourseScheduleFilePath { get; set; }
    public string StudentInfoFilePath { get; set; }
    public string StudentCourseRankingsFilePath { get; set; }

    public bool TrySetFilePaths(string[] paths, out string errorMessage)
    {
        var courseInfo = paths.Where(CsvUtil.HeaderMatchesCourseInfo).ToList();
        if (courseInfo.Count == 0) {
            errorMessage = "No files found matching required headers for course info data.";
            return false;
        }
        if (courseInfo.Count > 1) {
            errorMessage = "Multiple files found matching required headers for course info data.";
            return false;
        }

        var courseSchedule = paths.Where(CsvUtil.HeaderMatchesCourseSchedule).ToList();
        if (courseSchedule.Count == 0) {
            errorMessage = "No files found matching required headers for course schedule data.";
            return false;
        }
        if (courseSchedule.Count > 1) {
            errorMessage = "Multiple files found matching required headers for course schedule data.";
            return false;
        }

        var studentInfo = paths.Where(CsvUtil.HeaderMatchesStudentInfo).ToList();
        if (studentInfo.Count == 0) {
            errorMessage = "No files found matching required headers for student info data.";
            return false;
        }
        if (studentInfo.Count > 1) {
            errorMessage = "Multiple files found matching required headers for student info data.";
            return false;
        }

        var studentRankings = paths.Where(CsvUtil.HeaderMatchesStudentCourseRankings).ToList();
        if (studentRankings.Count == 0) {
            errorMessage = "No files found matching required headers for students' course ranking data.";
            return false;
        }
        if (studentRankings.Count > 1) {
            errorMessage = "Multiple files found matching required headers for students' course ranking data.";
            return false;
        }

        CourseInfoFilePath = courseInfo.Single();
        CourseScheduleFilePath = courseSchedule.Single();
        StudentInfoFilePath = studentInfo.Single();
        StudentCourseRankingsFilePath = studentRankings.Single();
        errorMessage = null;
        return true;
    }
    
    public string RunScheduling()
    {
        if (TryLoadInputs(out var inputData, out var loadError) == false)
            return loadError;

        if (ValidateInputs(inputData, out var inputError) == false)
            return inputError;

        try {
            var output = CourseScheduler.RunCourseScheduling(inputData);
            if (ValidateOutputs(output, out var errorMessage) == false)
                return errorMessage;

            var runId = CourseScheduler.Save(output, RunName);
            return $"Results saved with ID {runId}";
        }
        catch (Exception e) {
            return $"The course assignment algorithm encountered an error:{Environment.NewLine}{e}";
        }
    }

    private bool TryLoadInputs(out InputDataSet inputData, out string errorMessage)
    {
        string error;
        inputData = null;

        if (TryLoad(StudentInfoFilePath, CsvUtil.ReadStudentInfo, out var infoRows, out error) == false) {
            errorMessage = $"Error parsing student info file:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(StudentCourseRankingsFilePath, CsvUtil.ReadStudentCourseRankings, out var rankingRows, out error) == false) {
            errorMessage = $"Error parsing student ranking file:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(CourseInfoFilePath, CsvUtil.ReadCourseInfo, out var courseInfoRows, out error) == false) {
            errorMessage = $"Error parsing course info file:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(CourseScheduleFilePath, CsvUtil.ReadCourseSchedule, out var scheduleRows, out error) == false) {
            errorMessage = $"Error parsing course schedule file:{Environment.NewLine}{error}";
            return false;
        }

        Logger.Information($"Read {infoRows.Count} StudentInfo rows");
        Logger.Information($"Read {rankingRows.Count} StudentCourseRanking rows");
        Logger.Information($"Read {courseInfoRows.Count} {nameof(courseInfoRows)} rows");
        Logger.Information($"Read {scheduleRows.Count} {nameof(CourseSchedule)} rows");

        const string unassigned = "UNASSIGNED";
        foreach (var student in infoRows)
            rankingRows.Add(new StudentCourseRanking {
                StudentId = student.StudentId,
                CourseCode = unassigned,
                Rank = -1,
            });

        courseInfoRows.Add(new CourseInfo {
            CourseCode = unassigned,
            CourseName = unassigned,
            Description = unassigned,
            MinGrade = int.MinValue,
            MaxGrade = int.MaxValue,
        });

        var sessions = scheduleRows.Select(x => x.SessionId).Distinct().ToList();
        scheduleRows.AddRange(sessions.Select(x => new CourseSchedule {
            CourseCode = unassigned,
            SessionId = x,
        }));

        errorMessage = null;
        inputData = new InputDataSet(courseInfoRows, scheduleRows, infoRows, rankingRows);

        return true;
    }

    private static bool TryLoad<T>(string filePath, Func<string, T> loaderFunc, out T data, out string error) where T : class
    {
        error = null;
        try {
            data = loaderFunc.Invoke(filePath);
            return true;
        }
        catch (Exception e) {
            data = null;
            error = e.Message;
            return false;
        }
    }
    
    private static bool ValidateInputs(InputDataSet inputData, out string errorMessage)
    {
        // all students have rankings
        var missingRankings = InputValidator.MissingCourseRankings(inputData);
        if (missingRankings.Any()) {
            errorMessage = $"Course rankings not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingRankings.Take(10))}";
            if (missingRankings.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingRankings.Count} shown.";
            return false;
        }

        // rankings correspond to an entry in student info
        var missingStudentInfo = InputValidator.MissingStudentInfo(inputData);
        if (missingStudentInfo.Any()) {
            errorMessage = $"Rankings found for the following student IDs without corresponding student info:{Environment.NewLine}{string.Join(", ", missingStudentInfo.Take(10))}";
            if (missingStudentInfo.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingStudentInfo.Count} shown.";
            return false;
        }

        // rankings correspond to an entry in course info
        var missingCourseInfo = InputValidator.MissingCourseInfo(inputData);
        if (missingCourseInfo.Any()) {
            errorMessage = $"Rankings found for the following course codes without corresponding course info:{Environment.NewLine}{string.Join(", ", missingCourseInfo.Take(10))}";
            if (missingCourseInfo.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingCourseInfo.Count} shown.";
            return false;
        }

        
        // TODO more validation later
        // all students have at least as many rankings as there are sessions
        // all students have ranked classes such that they can at least take one class per session

        errorMessage = null;
        return true;
    }

    private static bool ValidateOutputs(AssignmentDataSet outputData, out string errorMessage)
    {
        var missingStudents = OutputValidator.MissingStudentOutput(outputData);
        if (missingStudents.Any()) {
            errorMessage = $"Student info not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingStudents.Take(10))}";
            if (missingStudents.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingStudents.Count} shown.";
            return false;
        }
        
        var missingCourses = OutputValidator.MissingCourseOutput(outputData);
        if (missingCourses.Any()) {
            errorMessage = $"Course info not found for the following course codes:{Environment.NewLine}{string.Join(", ", missingCourses.Take(10))}";
            if (missingCourses.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingCourses.Count} shown.";
            return false;
        }

        var missingSessions = OutputValidator.MissingSessions(outputData);
        if (missingSessions.Any()) {
            var minSession = outputData.AssignmentResults.Min(x => x.SessionId);
            var maxSession = outputData.AssignmentResults.Max(x => x.SessionId);
            errorMessage = $"Expected sessions ({minSession} - {maxSession}) not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingSessions.Take(10))}";
            if (missingSessions.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingSessions.Count} shown.";
            return false;
        }

        errorMessage = null;
        return true;
    }
}