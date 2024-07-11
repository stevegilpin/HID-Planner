using System.Collections.Generic;

namespace Data.Objects;

public class CourseInputData
{
    public CourseInfo CourseInfo { get; init; }
    public List<CourseSchedule> Schedule { get; init; }
}