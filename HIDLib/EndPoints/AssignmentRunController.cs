using System;
using System.Linq;
using Data;
using Data.Objects;

namespace HIDLib.EndPoints;

public static class AssignmentRunController
{
    public static AssignmentRunResult RunAssignments(AssignmentRunInputs inputs)
    {
        if (TryLoadInputs(inputs, out var inputData, out var loadError) == false)
            return AssignmentRunResult.InputError(loadError);

        if (ValidateInputs(inputData, out var inputError) == false)
            return AssignmentRunResult.InputError(inputError);

        return RunAssignment(inputData, inputs.RunDescription, out var runId, out var algoError) == false
            ? AssignmentRunResult.AssignmentError(algoError)
            : AssignmentRunResult.SuccessfulResult(runId);
    }

    public static AssignmentRunResult UploadAssignmentData(AssignmentRunInputs inputs, string resultFilePath, string description)
    {
        if (TryLoadInputs(inputs, out var inputData, out var loadError) == false)
            return AssignmentRunResult.InputError(loadError);

        if (ValidateInputs(inputData, out var inputError) == false)
            return AssignmentRunResult.InputError(inputError);
        
        if (TryLoad(resultFilePath, CsvUtil.ReadAssignmentResults, out var resultRows, out var resultLoadError) == false)
            return AssignmentRunResult.InputError(resultLoadError);

        var dataSet = new AssignmentDataSet {
            AssignmentResults = resultRows,
            InputData = inputData,
            RunInfo = new RunInfo {
                ManuallyCreatedResults = true,
                RunTimeStamp = DateTime.Now,
            }
        };

        if (ValidateResults(dataSet, out var resultValidationError) == false)
            return AssignmentRunResult.InputError(resultValidationError);

        var runId = DataManager.Save(dataSet, description);
        return AssignmentRunResult.SuccessfulResult(runId);
    }
    
    private static bool TryLoadInputs(AssignmentRunInputs fileInfo, out InputDataSet inputData, out string errorMessage)
    {
        string error;
        inputData = null;

        if (TryLoad(fileInfo.StudentInfoFilePath, CsvUtil.ReadStudentInfo, out var infoRows, out error) == false) {
            errorMessage = $"Error parsing student info fie:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(fileInfo.StudentRankingsFilePath, CsvUtil.ReadStudentCourseRankings, out var rankingRows, out error) == false) {
            errorMessage = $"Error parsing student ranking fie:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(fileInfo.CourseInfoFilePath, CsvUtil.ReadCourseInfo, out var courseInfoRows, out error) == false) {
            errorMessage = $"Error parsing course info fie:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(fileInfo.CourseScheduleFilePath, CsvUtil.ReadCourseSchedule, out var scheduleRows, out error) == false) {
            errorMessage = $"Error parsing course schedule fie:{Environment.NewLine}{error}";
            return false;
        }

        Console.WriteLine($"Read {infoRows.Count} StudentInfo rows");
        Console.WriteLine($"Read {rankingRows.Count} StudentCourseRanking rows");
        Console.WriteLine($"Read {courseInfoRows.Count} {nameof(courseInfoRows)} rows");
        Console.WriteLine($"Read {scheduleRows.Count} {nameof(CourseSchedule)} rows");

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

    private static bool ValidateInputs(InputDataSet inputs, out string errorMessage)
    {
        var students = inputs.StudentInfo;
        var courses = inputs.CourseInfo;
        
        // all students have rankings
        var missingRankings = students.Keys.Except(inputs.StudentCourseRanking.Keys).ToList();
        if (missingRankings.Any()) {
            errorMessage = $"Course rankings not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingRankings.Take(10))}";
            if (missingRankings.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingRankings.Count} shown.";
            return false;
        }

        // rankings correspond to an entry in student info
        var missingStudentInfo = inputs.StudentCourseRanking.Keys
            .Except(students.Keys)
            .OrderBy(x => x)
            .ToList();

        if (missingStudentInfo.Any()) {
            errorMessage = $"Rankings found for the following student IDs without corresponding student info:{Environment.NewLine}{string.Join(", ", missingStudentInfo.Take(10))}";
            if (missingStudentInfo.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingStudentInfo.Count} shown.";
            return false;
        }

        // rankings correspond to an entry in course info
        var rankedCourses = inputs.StudentCourseRanking.Values.SelectMany(x => x.Select(y => y.CourseCode)).Distinct().ToList();
        var missingCourseInfo = rankedCourses
            .Except(courses.Keys)
            .OrderBy(x => x)
            .ToList();

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

    private static bool ValidateResults(AssignmentDataSet data, out string errorMessage)
    {
        var missingStudents = data.AssignmentResults.Select(x => x.StudentId).Except(data.InputData.StudentInfo.Keys).ToList();
        if (missingStudents.Any()) {
            errorMessage = $"Student info not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingStudents.Take(10))}";
            if (missingStudents.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingStudents.Count} shown.";
            return false;
        }
        
        var missingCourses = data.AssignmentResults.Select(x => x.CourseCode).Except(data.InputData.CourseInfo.Keys).ToList();
        if (missingCourses.Any()) {
            errorMessage = $"Course info not found for the following course codes:{Environment.NewLine}{string.Join(", ", missingCourses.Take(10))}";
            if (missingCourses.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingCourses.Count} shown.";
            return false;
        }

        var minSession = data.AssignmentResults.Min(x => x.SessionId);
        var maxSession = data.AssignmentResults.Max(x => x.SessionId);
        var expectedSessions = Enumerable.Range(minSession, maxSession - minSession + 1);
        var missingSessions = data.AssignmentResults
            .GroupBy(x => x.StudentId)
            .Where(g => expectedSessions.SequenceEqual(g.Select(x => x.SessionId).OrderBy(x => x).ToList()) == false)
            .Select(g => g.Key)
            .ToList();
        if (missingSessions.Any()) {
            errorMessage = $"Expected sessions ({minSession} - {maxSession}) not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingSessions.Take(10))}";
            if (missingSessions.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingSessions.Count} shown.";
            return false;
        }

        errorMessage = null;
        return true;
    }
    
    private static bool RunAssignment(InputDataSet inputs, string description, out int runId, out string errorMessage)
    {
        try {
            var output = SchedulingSolver.RunSolve(inputs);
            runId = DataManager.Save(output, description);
            errorMessage = null;
            return true;
        }
        catch (Exception e) {
            runId = default;
            errorMessage = $"Error running the course assignment algorithm:{Environment.NewLine}{e.Message}";
            return false;
        }
    }
}

public sealed class AssignmentRunInputs
{
    public required string RunDescription { get; init; }
    public required string CourseInfoFilePath { get; init; }
    public required string CourseScheduleFilePath { get; init; }
    public required string StudentInfoFilePath { get; init; }
    public required string StudentRankingsFilePath { get; init; }
}

public sealed class AssignmentRunResult
{
    public string InputValidationError;
    public string CourseAssignmentError;
    public int RunId;

    public bool ValidInputs => string.IsNullOrWhiteSpace(InputValidationError);
    public bool SuccessfulAssignment => string.IsNullOrWhiteSpace(CourseAssignmentError);

    private AssignmentRunResult(int runId, string inputError, string courseAssignmentError)
    {
        RunId = runId;
        InputValidationError = inputError;
        CourseAssignmentError = courseAssignmentError;
    }

    internal static AssignmentRunResult SuccessfulResult(int runId) => new (runId, null, null);
    internal static AssignmentRunResult InputError(string errorMessage) => new (0, errorMessage, null);
    internal static AssignmentRunResult AssignmentError(string errorMessage) => new (0, null, errorMessage);
}