using System;
using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.IO;
using HIDLib.Scheduling;
using Serilog;

namespace HIDUI.BusinessLogic;

public class SchedulingManager
{
    public ILogger Logger { get; set; } = new LoggerConfiguration().WriteTo.Console().CreateLogger();

    public required string RunName { get; init; }
    public required string CourseInfoFilePath { get; init; }
    public required string CourseScheduleFilePath { get; init; }
    public required string StudentInfoFilePath { get; init; }
    public required string StudentCourseRankingsFilePath { get; init; }
    public string ManualSchedulingResultsFilePath { get; init; }

    public void RunScheduling()
    {
        var inputData = LoadInputs();
        var output = CourseScheduler.RunCourseScheduling(inputData);
        var outputError = ValidateOutputs(output);
        if (outputError != null)
            throw outputError;

        var runId = CourseScheduler.Save(output, RunName);
        Logger.Information($"Results saved with ID {runId}");
    }

    public void SaveManualSchedulingResults()
    {
        var inputData = LoadInputs();
        var results = CsvReadUtil.ReadAssignmentResults(ManualSchedulingResultsFilePath);
        var output = new AssignmentDataSet {
            InputData = inputData,
            AssignmentResults = results,
            RunInfo = new RunInfo {
                RunTimeStamp = DateTime.Now,
                ManuallyCreatedResults = true,
                Name = RunName,
            }
        };

        var outputError = ValidateOutputs(output) ?? ValidateManualResults(output);
        if (outputError != null)
            throw outputError;

        var runId = CourseScheduler.Save(output, RunName);
        Logger.Information($"Results saved with ID {runId}");
    }

    private InputDataSet LoadInputs()
    {
        var infoRows = CsvReadUtil.ReadStudentInfo(StudentInfoFilePath);
        Logger.Information($"Read {infoRows.Count} {nameof(StudentInfo)} rows");

        var rankingRows = CsvReadUtil.ReadStudentCourseRankings(StudentCourseRankingsFilePath);
        Logger.Information($"Read {rankingRows.Count} {nameof(StudentCourseRanking)} rows");

        var courseInfoRows = CsvReadUtil.ReadCourseInfo(CourseInfoFilePath);
        Logger.Information($"Read {courseInfoRows.Count} {nameof(CourseInfo)} rows");

        var scheduleRows = CsvReadUtil.ReadCourseSchedule(CourseScheduleFilePath);
        Logger.Information($"Read {scheduleRows.Count} {nameof(CourseSchedule)} rows");
        
        var inputData = new InputDataSet(courseInfoRows, scheduleRows, infoRows, rankingRows);
        var inputError = ValidateInputs(inputData);
        if (inputError != null)
            throw inputError;

        AddUnassignedCoursePlaceholder(inputData, Logger);

        return inputData;
    }

    private static void AddUnassignedCoursePlaceholder(InputDataSet inputData, ILogger logger)
    {
        const string unassigned = "UNASSIGNED";
        var grades = inputData.StudentInfo.Values.GroupBy(x => x.Grade).ToList();
        foreach (var gradeGroup in grades) {
            var gradeNum = CsvReadUtil.GradeToInt(gradeGroup.Key);
            var sessions = inputData.CourseInfo.Values
                .Where(x => x.MinGrade <= gradeNum &&
                            x.MaxGrade >= gradeNum)
                .SelectMany(x => inputData.CourseSchedule[x.CourseCode].Select(s => s.SessionId))
                .Distinct()
                .ToList();

            foreach (var session in sessions) {
                var courseCode = $"{unassigned}_{gradeGroup.Key}_{session}";
                if (inputData.CourseInfo.ContainsKey(courseCode))
                    continue;

                inputData.CourseInfo.Add(courseCode, new CourseInfo {
                    CourseCode = courseCode,
                    CourseName = unassigned,
                    Description = "Placeholder value in case the scheduling algorithm cannot find a solution",
                    MinGrade = gradeNum,
                    MaxGrade = gradeNum,
                });

                inputData.CourseSchedule.Add(courseCode, [
                    new CourseSchedule {
                        CourseCode = courseCode,
                        SessionId = session,
                    }
                ]);

                foreach (var student in gradeGroup) {
                    inputData.StudentCourseRanking[student.StudentId].Add(new StudentCourseRanking {
                        StudentId = student.StudentId,
                        CourseCode = courseCode,
                        Rank = (int)10e3,
                    });
                }
            }
        }

        logger.Information($"Added {unassigned} rows");
    }
    
    private static Exception ValidateInputs(InputDataSet inputData)
    {
        // all students have rankings
        var missingRankings = InputValidator.MissingCourseRankings(inputData);
        if (missingRankings.Any()) {
            var errorMessage = $"Course rankings not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingRankings.Take(10))}";
            if (missingRankings.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingRankings.Count} shown.";
            return new Exception(errorMessage);
        }

        // rankings correspond to an entry in student info
        var missingStudentInfo = InputValidator.MissingStudentInfo(inputData);
        if (missingStudentInfo.Any()) {
            var errorMessage = $"Rankings found for the following student IDs without corresponding student info:{Environment.NewLine}{string.Join(", ", missingStudentInfo.Take(10))}";
            if (missingStudentInfo.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingStudentInfo.Count} shown.";
            return new Exception(errorMessage);
        }

        // rankings correspond to an entry in course info
        var missingCourseInfo = InputValidator.MissingCourseInfo(inputData);
        if (missingCourseInfo.Any()) {
            var errorMessage = $"Rankings found for the following course codes without corresponding course info:{Environment.NewLine}{string.Join(", ", missingCourseInfo.Take(10))}";
            if (missingCourseInfo.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingCourseInfo.Count} shown.";
            return new Exception(errorMessage);
        }

        
        // TODO more validation later
        // check for violations of implicit foreign keys (e.g. Do all courses in CourseSchedule have a row in CourseInfo?)
        // all students have at least as many rankings as there are sessions
        // all students have ranked classes such that they can at least take one class per session

        return null;
    }

    private static Exception ValidateOutputs(AssignmentDataSet outputData)
    {
        var missingStudents = OutputValidator.MissingStudentOutput(outputData);
        if (missingStudents.Any()) {
            var errorMessage = $"Student info not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingStudents.Take(10))}";
            if (missingStudents.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingStudents.Count} shown.";
            return new Exception(errorMessage);
        }
        
        var missingCourses = OutputValidator.MissingCourseOutput(outputData);
        if (missingCourses.Any()) {
            var errorMessage = $"Course info not found for the following course codes:{Environment.NewLine}{string.Join(", ", missingCourses.Take(10))}";
            if (missingCourses.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingCourses.Count} shown.";
            return new Exception(errorMessage);
        }

        var missingSessions = OutputValidator.MissingSessions(outputData);
        if (missingSessions.Any()) {
            var errorMessage = $"Expected sessions not found for the following student IDs:{Environment.NewLine}{string.Join(", ", missingSessions.Take(10))}";
            if (missingSessions.Count > 10)
                errorMessage += $"{Environment.NewLine}{Environment.NewLine}First 10 of {missingSessions.Count} shown.";
            return new Exception(errorMessage);
        }

        return null;
    }

    private static Exception ValidateManualResults(AssignmentDataSet outputData)
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
            
            var errorMessage = $"Course code {course.CourseCode} does not exist in the course schedule for session {course.SessionId}.";
            return new Exception(errorMessage);
        }

        foreach (var course in outputData.AssignmentResults) {
            if (outputData.InputData.StudentInfo.ContainsKey(course.StudentId))
                continue;

            var errorMessage = $"Student ID {course.StudentId} not found in input data";
            return new Exception(errorMessage);
        }
        
        foreach (var course in outputData.AssignmentResults) {
            var courseInfo = outputData.InputData.CourseInfo[course.CourseCode];
            var studentInfo = outputData.InputData.StudentInfo[course.StudentId];
            var grade = CsvReadUtil.GradeToInt(studentInfo.Grade);
            if (grade < courseInfo.MinGrade || grade > courseInfo.MaxGrade) {
                var errorMessage = $"{course.CourseCode} is not permitted for student {course.StudentId} in grade {studentInfo.Grade}";
                return new Exception(errorMessage);
            }
        }
        
        return null;
    }
}