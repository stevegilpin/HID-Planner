using System.Collections.Generic;
using System.Linq;
using Data.Objects;

namespace HIDLib.EndPoints;

public static class OutputValidator
{
    public static List<int> MissingStudentOutput(AssignmentDataSet data)
    {
        return data.AssignmentResults.Select(x => x.StudentId).Except(data.InputData.StudentInfo.Keys).ToList();
    }

    public static List<string> MissingCourseOutput(AssignmentDataSet data)
    {
        return data.AssignmentResults.Select(x => x.CourseCode).Except(data.InputData.CourseInfo.Keys).ToList();
    }

    public static List<int> MissingSessions(AssignmentDataSet data)
    {
        var minSession = data.AssignmentResults.Min(x => x.SessionId);
        var maxSession = data.AssignmentResults.Max(x => x.SessionId);
        var expectedSessions = Enumerable.Range(minSession, maxSession - minSession + 1);
        return data.AssignmentResults
            .GroupBy(x => x.StudentId)
            .Where(g => expectedSessions.SequenceEqual(g.Select(x => x.SessionId).OrderBy(x => x).ToList()) == false)
            .Select(g => g.Key)
            .ToList();
    }
}