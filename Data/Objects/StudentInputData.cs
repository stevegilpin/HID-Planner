using System.Collections.Generic;

namespace Data.Objects;

public class StudentInputData
{
    public StudentInfo StudentInfo { get; init; }
    public List<StudentCourseRanking> CourseRankings { get; init; }
}