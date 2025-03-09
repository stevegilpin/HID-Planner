using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Objects;

namespace HIDLib.Scheduling;

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
        var students = data.InputData.StudentInfo;
        var gradeGroup = data.AssignmentResults.GroupBy(x => students[x.StudentId].Grade);
        var missingSessions = new List<int>();
        foreach (var group in gradeGroup) {
            var minSession = group.Min(x => x.SessionId);
            var maxSession = group.Max(x => x.SessionId);
            var expectedSessions = Enumerable.Range(minSession, maxSession - minSession + 1);
            missingSessions.AddRange(group
                .GroupBy(x => x.StudentId)
                .Where(g => expectedSessions.SequenceEqual(g.Select(x => x.SessionId).OrderBy(x => x).ToList()) == false)
                .Select(g => g.Key)
                .ToList());
        }
        return missingSessions;
        
    }
}