using System.Collections.Generic;
using System.Linq;
using Data.Objects;

namespace HIDLib.Scheduling;

public static class InputValidator
{
    public static List<int> MissingCourseRankings(InputDataSet inputData)
    {
        return inputData.StudentInfo.Keys.Except(inputData.StudentCourseRanking.Keys).ToList();
    }

    public static List<int> MissingStudentInfo(InputDataSet inputData)
    {
        return inputData.StudentCourseRanking.Keys
            .Except(inputData.StudentInfo.Keys)
            .OrderBy(x => x)
            .ToList();
    }

    public static List<string> MissingCourseInfo(InputDataSet inputData)
    {
        // rankings correspond to an entry in course info
        var rankedCourses = inputData.StudentCourseRanking.Values.SelectMany(x => x.Select(y => y.CourseCode)).Distinct().ToList();
        return rankedCourses
            .Except(inputData.CourseInfo.Keys)
            .OrderBy(x => x)
            .ToList();
    }
}