using System.Collections.Generic;

namespace Data.Objects;

public class AssignmentDataSet
{
    public InputDataSet InputData { get; init; }
    public List<CourseAssignmentResult> AssignmentResults { get; init; }
    public RunInfo RunInfo { get; init; }
}