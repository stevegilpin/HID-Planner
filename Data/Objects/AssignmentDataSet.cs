using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Objects;

public class AssignmentDataSet
{
    public InputDataSet InputData { get; init; }
    public List<CourseAssignmentResult> AssignmentResults { get; init; }
    public RunInfo RunInfo { get; init; }

    public List<int> AllSessions()
    {
        return AssignmentResults.Select(x => x.SessionId).Distinct().OrderBy(x => x).ToList();
    }

    public List<CourseAssignmentResult> GetStudentsAssignments(int student)
    {
        return AssignmentResults.Where(x => x.StudentId == student).ToList();
    }
}