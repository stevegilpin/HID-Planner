using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Objects;
using Google.OrTools.LinearSolver;

namespace HIDLib;

public static class SchedulingSolver
{
    private const string SOLVER_TYPE = "SCIP";
    
    private class InputData
    {
        public List<CourseInputData> CourseData { get; init; }
        public List<StudentCourseRanking> StudentRankings { get; init; }
    }

    public class SolveData
    {
        public int StudentRankingInputId { get; init; }
        public int CourseScheduleInputId { get; init; }
        public List<VariableData> Variables { get; init; }
        public Solver.ResultStatus ResultCode { get; init; }
        public double ObjectiveFunctionValue { get; init; }
        public DateTime Timestamp { get; init; }

        public List<CourseAssignmentResult> ToDbResultRows()
        {
            var resultRows = new List<CourseAssignmentResult>();
            foreach (var variable in Variables) {
                if (variable.Variable.SolutionValue() == 0)
                    continue;
                
                resultRows.Add(new CourseAssignmentResult {
                    CourseCode = variable.StudentRanking.CourseCode,
                    StudentId = variable.StudentRanking.StudentId,
                    SessionId = variable.SessionId,
                });
            }

            return resultRows;
        }

        public CourseAssignmentRunInfo ToDbRunRow(string description)
        {
            return new CourseAssignmentRunInfo {
                RunTimeStamp = Timestamp,
                CourseScheduleInputId = CourseScheduleInputId,
                StudentRankingInputId = StudentRankingInputId,
                Description = description,
            };
        }
    }

    public record VariableData(
        StudentCourseRanking StudentRanking,
        CourseInfo CourseInfo,
        int SessionId,
        Variable Variable);
    
    public static SolveData RunSolve(int studentRankingId, int courseScheduleId)
    {
        var timestamp = DateTime.Now;
        var inputs = LoadInputs(studentRankingId, courseScheduleId);

        var solver = Solver.CreateSolver(SOLVER_TYPE);
        var variables = CreateVariables(solver, inputs);
        AddConstraints_OneCoursePerSession(solver, variables);
        AddConstraints_CourseCapacity(solver, inputs, variables);
        AddConstraints_NoDupeCourses(solver, variables);
        SetObjectiveFunction(solver, variables);

        var res = solver.Solve();

        return new SolveData {
            CourseScheduleInputId = courseScheduleId,
            StudentRankingInputId = studentRankingId,
            Variables = variables,
            ObjectiveFunctionValue = solver.Objective().Value(),
            ResultCode = res,
            Timestamp = timestamp,
        };
    }
    
    private static InputData LoadInputs(int studentRankingId, int courseScheduleId)
    {
        var courseData = DataManager.Courses.GetInputData(courseScheduleId);
        var studentRankings = DataManager.Students.GetStudentCourseRankingData(studentRankingId);
        return new InputData {
            CourseData = courseData,
            StudentRankings = studentRankings,
        };
    }

    private static List<VariableData> CreateVariables(Solver solver, InputData inputs)
    {
        var studentData = inputs.StudentRankings.OrderBy(x => x.StudentId).ThenBy(x => x.Rank).ToList();

        var courseMap = inputs.CourseData.ToDictionary(x => x.CourseInfo.CourseCode);

        var variables = new List<VariableData>();
        foreach (var ranking in studentData) {
            var course = courseMap[ranking.CourseCode];
            foreach (var session in course.Schedule) {
                var variableName = $"{ranking.StudentId}_{course.CourseInfo.CourseCode}_{session.SessionId}";
                var variable = solver.MakeIntVar(0, 1, variableName);
                variables.Add(new VariableData(ranking, course.CourseInfo, session.SessionId, variable));
            }
        }

        return variables;
    }

    // a student must take exactly one course per session
    private static void AddConstraints_OneCoursePerSession(Solver solver, List<VariableData> variables)
    {
        var concurrentSessions = variables.GroupBy(x => (x.StudentRanking.StudentId, x.SessionId)).ToList();
        foreach (var sessionGroup in concurrentSessions) {
            var name = $"{sessionGroup.Key.StudentId}_{sessionGroup.Key.SessionId}_Const1";
            AddConstraint(solver, 1, 1, sessionGroup.ToList(), name);
        }
    }

    private static void AddConstraints_CourseCapacity(Solver solver, InputData inputs, List<VariableData> variables)
    {
        var uniqueSessions = variables.GroupBy(x => (x.CourseInfo.CourseCode, x.SessionId)).ToList();
        var courseInfoMap = inputs.CourseData.ToDictionary(x => x.CourseInfo.CourseCode);
        foreach (var sessionGroup in uniqueSessions) {
            var courseInfo = courseInfoMap[sessionGroup.Key.CourseCode].CourseInfo;
            var name = $"{sessionGroup.Key.CourseCode}_{sessionGroup.Key.SessionId}_Const2";
            AddConstraint(solver, courseInfo.MinCapacity ?? 0, courseInfo.MaxCapacity ?? int.MaxValue, sessionGroup.ToList(), name);
        }
    }

    // a student can take a course 0 or 1 times (no duplicate courses)
    private static void AddConstraints_NoDupeCourses(Solver solver, List<VariableData> variables)
    {
        var studentCourseGroups = variables.GroupBy(x => (x.StudentRanking.StudentId, x.StudentRanking.CourseCode)).ToList();
        foreach (var courseGroup in studentCourseGroups) {
            var name = $"{courseGroup.Key.StudentId}_{courseGroup.Key.CourseCode}_Const3";
            AddConstraint(solver, 0, 1, courseGroup.ToList(), name);
        }
    }

    private static void AddConstraint(Solver solver, int min, int max, List<VariableData> variables, string constraintName)
    {
        var constraint = solver.MakeConstraint(min, max, constraintName);
        foreach (var variable in variables)
            constraint.SetCoefficient(variable.Variable, 1);
    }

    private static void SetObjectiveFunction(Solver solver, List<VariableData> variables)
    {
        var f = solver.Objective();
        foreach (var variable in variables) {
            var coeff = GetCourseCost(variable.StudentRanking.Rank);
            f.SetCoefficient(variable.Variable, coeff);
        }

        f.SetMinimization();
    }

    public static double GetCourseCost(int courseRank)
    {
        return Math.Pow(courseRank, 3);
    }
}