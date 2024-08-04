using System.Collections.Generic;

namespace Data.Objects;

public class SchedulingOutputData
{
    public Dictionary<int, StudentInputData> StudentInputs { get; init; }
    public Dictionary<string, CourseInputData> CourseInputs { get; init; }

    public List<CourseAssignmentResult> AssignmentResults { get; init; }
    public CourseAssignmentRunInfo AssignmentRun { get; init; }
}