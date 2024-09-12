using System.Collections.Generic;
using System.Linq;

namespace Data.Objects;

public class InputDataSet
{
    public readonly Dictionary<string, CourseInfo> CourseInfo;
    public readonly Dictionary<string, List<CourseSchedule>> CourseSchedule;
    public readonly Dictionary<int, StudentInfo> StudentInfo;
    public readonly Dictionary<int, List<StudentCourseRanking>> StudentCourseRanking;

    public InputDataSet(
        List<CourseInfo> courseInfo,
        List<CourseSchedule> courseSchedule,
        List<StudentInfo> studentInfo,
        List<StudentCourseRanking> studentCourseRanking)
    {
        CourseInfo = courseInfo.ToDictionary(x => x.CourseCode);
        var courseScheduleMap = courseSchedule.GroupBy(x => x.CourseCode).ToDictionary(g => g.Key, g => g.ToList());
        CourseSchedule = courseInfo.ToDictionary(x => x.CourseCode, x => courseScheduleMap.GetValueOrDefault(x.CourseCode, [])); 
        StudentInfo = studentInfo.ToDictionary(x => x.StudentId);
        StudentCourseRanking = studentCourseRanking.GroupBy(x => x.StudentId).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Rank).ToList());
    }
}