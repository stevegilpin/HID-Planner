using System;
using System.Collections.Generic;
using System.Linq;
using Database.Objects;
using Google.OrTools.LinearSolver;

namespace HIDLib;

public class SchedulingSolver
{
    private const string SOLVER_TYPE = "SCIP";
    
    public int StudentRankingInputId { get; init; }
    public int CourseScheduleInputId { get; init; }
    
    private class InputData
    {
        public List<CourseInputData> CourseData { get; init; }
        public List<StudentCourseRanking> StudentRankings { get; init; }
    }

    private record VariableData(
        StudentCourseRanking StudentRanking,
        CourseInfo CourseInfo,
        int SessionId,
        Variable Variable);
    
    public void RunSolve()
    {
        var inputs = LoadInputs();
        var solver = Solver.CreateSolver(SOLVER_TYPE);
        var variables = CreateVariables(solver, inputs);
        AddConstraints_OneCoursePerSession(solver, variables);
        AddConstraints_CourseCapacity(solver, inputs, variables);
        AddConstraints_NoDupeCourses(solver, variables);
        SetObjectiveFunction(solver, variables);

        var res = solver.Solve();
        // output sanity check
        throw new NotImplementedException();
    }
    
    private InputData LoadInputs()
    {
        var courseData = DbController.Courses.GetInputData(CourseScheduleInputId);
        var studentRankings = DbController.StudentRankings.GetStudentCourseRankingData(StudentRankingInputId);
        return new InputData {
            CourseData = courseData,
            StudentRankings = studentRankings,
        };
    }

    private List<VariableData> CreateVariables(Solver solver, InputData inputs)
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
    private void AddConstraints_OneCoursePerSession(Solver solver, List<VariableData> variables)
    {
        var concurrentSessions = variables.GroupBy(x => (x.StudentRanking.StudentId, x.SessionId)).ToList();
        foreach (var sessionGroup in concurrentSessions) {
            var name = $"{sessionGroup.Key.StudentId}_{sessionGroup.Key.SessionId}_Const1";
            AddConstraint(solver, 1, 1, sessionGroup.ToList(), name);
        }
    }

    private void AddConstraints_CourseCapacity(Solver solver, InputData inputs, List<VariableData> variables)
    {
        var uniqueSessions = variables.GroupBy(x => (x.CourseInfo.CourseCode, x.SessionId)).ToList();
        var courseInfo = inputs.CourseData.ToDictionary(x => x.CourseInfo.CourseCode);
        foreach (var sessionGroup in uniqueSessions) {
            var capacity = courseInfo[sessionGroup.Key.CourseCode].Capacity;
            var name = $"{sessionGroup.Key.CourseCode}_{sessionGroup.Key.SessionId}_Const2";
            AddConstraint(solver, capacity.MinCapacity, capacity.MaxCapacity ?? int.MaxValue, sessionGroup.ToList(), name);
        }
    }

    // a student can take a course 0 or 1 times (no duplicate courses)
    private void AddConstraints_NoDupeCourses(Solver solver, List<VariableData> variables)
    {
        var studentCourseGroups = variables.GroupBy(x => (x.StudentRanking.StudentId, x.StudentRanking.CourseCode)).ToList();
        foreach (var courseGroup in studentCourseGroups) {
            var name = $"{courseGroup.Key.StudentId}_{courseGroup.Key.CourseCode}_Const3";
            AddConstraint(solver, 0, 1, courseGroup.ToList(), name);
        }
    }

    private void AddConstraint(Solver solver, int min, int max, List<VariableData> variables, string constraintName)
    {
        var constraint = solver.MakeConstraint(min, max, constraintName);
        foreach (var variable in variables)
            constraint.SetCoefficient(variable.Variable, 1);
    }

    private void SetObjectiveFunction(Solver solver, List<VariableData> variables)
    {
        var f = solver.Objective();
        foreach (var variable in variables) {
            var coeff = GetCourseCost(variable.StudentRanking.Rank);
            f.SetCoefficient(variable.Variable, coeff);
        }

        f.SetMinimization();
    }

    private static double GetCourseCost(int courseRank)
    {
        if (courseRank <= 5)
            return courseRank;

        if (courseRank <= 10)
            return Math.Pow(courseRank, 2);

        return Math.Pow(courseRank, 3);
    }
}