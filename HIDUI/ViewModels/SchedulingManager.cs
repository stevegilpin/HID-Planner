using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using Data.Objects;
using HIDLib.IO;
using HIDLib.Scheduling;
using HIDUI.BusinessLogic;
using Serilog;

namespace HIDUI.ViewModels;

public class SchedulingManager
{
    public ILogger Logger { get; set; } = new LoggerConfiguration().WriteTo.Console().CreateLogger();

    public required string RunName { get; init; }
    public required string CourseInfoFilePath { get; init; }
    public required string CourseScheduleFilePath { get; init; }
    public required string StudentInfoFilePath { get; init; }
    public required string StudentCourseRankingsFilePath { get; init; }
    public string ManualSchedulingResultsFilePath { get; init; }

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

    public string SaveManualSchedulingResults()
    {
        if (TryLoadInputs(out var inputData, out var loadError) == false)
            return loadError;

        var results = CsvReadUtil.ReadAssignmentResults(ManualSchedulingResultsFilePath);
        
        if (ValidateInputs(inputData, out var inputError) == false)
            return inputError;

        var dataSet = new AssignmentDataSet {
            InputData = inputData,
            AssignmentResults = results,
            RunInfo = new RunInfo {
                RunTimeStamp = DateTime.Now,
                ManuallyCreatedResults = true,
                Name = RunName,
            }
        };

        if (ValidateOutputs(dataSet, out var errorMessage) == false)
            return errorMessage;

        if (ValidateManualResults(dataSet, out var manualResErrorMessage) == false)
            return manualResErrorMessage;

        var runId = CourseScheduler.Save(dataSet, RunName);
        return $"Results saved with ID {runId}";
    }

    private bool TryLoadInputs(out InputDataSet inputData, out string errorMessage)
    {
        string error;
        inputData = null;

        if (TryLoad(StudentInfoFilePath, CsvReadUtil.ReadStudentInfo, out var infoRows, out error) == false) {
            errorMessage = $"Error parsing student info file:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(StudentCourseRankingsFilePath, CsvReadUtil.ReadStudentCourseRankings, out var rankingRows, out error) == false) {
            errorMessage = $"Error parsing student ranking file:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(CourseInfoFilePath, CsvReadUtil.ReadCourseInfo, out var courseInfoRows, out error) == false) {
            errorMessage = $"Error parsing course info file:{Environment.NewLine}{error}";
            return false;
        }

        if (TryLoad(CourseScheduleFilePath, CsvReadUtil.ReadCourseSchedule, out var scheduleRows, out error) == false) {
            errorMessage = $"Error parsing course schedule file:{Environment.NewLine}{error}";
            return false;
        }

        Logger.Information($"Read {infoRows.Count} StudentInfo rows");
        Logger.Information($"Read {rankingRows.Count} StudentCourseRanking rows");
        Logger.Information($"Read {courseInfoRows.Count} {nameof(courseInfoRows)} rows");
        Logger.Information($"Read {scheduleRows.Count} {nameof(CourseSchedule)} rows");
        
        const string unassigned = "UNASSIGNED";
        bool hasUnassigned = courseInfoRows.Any(x => x.CourseCode == unassigned) ||
                             scheduleRows.Any(x => x.CourseCode == unassigned) ||
                             rankingRows.Any(x => x.CourseCode == unassigned);
        if (hasUnassigned == false) {
            foreach (var student in infoRows)
                rankingRows.Add(new StudentCourseRanking {
                    StudentId = student.StudentId,
                    CourseCode = unassigned,
                    Rank = (int)10e3,
                });

            courseInfoRows.Add(new CourseInfo {
                CourseCode = unassigned,
                CourseName = unassigned,
                Description = "Placeholder value in case the scheduling algorithm cannot find a solution",
                MinGrade = int.MinValue,
                MaxGrade = int.MaxValue,
            });

            var sessions = scheduleRows.Select(x => x.SessionId).Distinct().ToList();
            scheduleRows.AddRange(sessions.Select(x => new CourseSchedule {
                CourseCode = unassigned,
                SessionId = x,
            }));
        }

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
        // check for violations of implicit foreign keys (e.g. Do all courses in CourseSchedule have a row in CourseInfo?)
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

    private static bool ValidateManualResults(AssignmentDataSet outputData, out string errorMessage)
    {
        // each course / session assignment exists in the inputs
        // each student exists in the inputs
        // students are assigned to valid classes based on the class's min/max grade

        var inputSessions = new HashSet<(string, int)>();
        foreach (var session in outputData.InputData.CourseSchedule.Values.SelectMany(g => g)) {
            inputSessions.Add((session.CourseCode, session.SessionId));
        }

        foreach (var course in outputData.AssignmentResults) {
            if (inputSessions.Contains((course.CourseCode, course.SessionId)))
                continue;
            
            errorMessage = $"Course code {course.CourseCode} does not exist in the course schedule for session {course.SessionId}.";
            return false;
        }

        foreach (var course in outputData.AssignmentResults) {
            if (outputData.InputData.StudentInfo.ContainsKey(course.StudentId))
                continue;

            errorMessage = $"Student ID {course.StudentId} not found in input data";
            return false;
        }
        

        foreach (var course in outputData.AssignmentResults) {
            var courseInfo = outputData.InputData.CourseInfo[course.CourseCode];
            var studentInfo = outputData.InputData.StudentInfo[course.StudentId];
            var grade = CsvReadUtil.GradeToInt(studentInfo.Grade);
            if (grade < courseInfo.MinGrade || grade > courseInfo.MaxGrade) {
                errorMessage = $"{course.CourseCode} is not permitted for student {course.StudentId} in grade {studentInfo.Grade}";
                return false;
            }
        }
        
        errorMessage = null;
        return true;
    }
}