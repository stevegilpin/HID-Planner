using Database;
using Database.Objects;

namespace HIDLib;

public static class DbController
{
    public static class StudentRankings
    {
        public static int GetLatestRankingInputId()
        {
            using var db = DbProvider.GetDbContext();
            return db.StudentRankingInputDescription.Max(x => x.InputId);
        }

        public static List<StudentCourseRanking> GetStudentCourseRankingData(int inputId)
        {
            using var db = DbProvider.GetDbContext();
            return db.StudentCourseRanking.Where(x => x.RankingInputId == inputId).ToList();
        }
    }

    public static class Courses
    {
        public static int GetLatestScheduleInputId()
        {
            using var db = DbProvider.GetDbContext();
            return db.CourseScheduleInputDescription.Max(x => x.InputId);
        }

        public static List<CourseInputData> GetInputData(int inputId)
        {
            using var db = DbProvider.GetDbContext();
            var eventId = db.CourseScheduleInputDescription.Single(x => x.InputId == inputId).EventId;
            var courseInfo = db.CourseInfo.Where(x => x.EventId == eventId).ToList();
            var capacities = db.CourseCapacity.Where(x => x.CourseScheduleInputId == inputId).ToDictionary(x => x.CourseCode);
            var schedules = db.CourseSchedule
                .Where(x => x.CourseScheduleInputId == inputId)
                .ToList()
                .GroupBy(x => x.CourseCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            return courseInfo.Select(x => new CourseInputData {
                CourseInfo = x,
                Capacity = capacities[x.CourseCode],
                Schedule = schedules[x.CourseCode],
            }).ToList();
        }
    }
}

public class CourseInputData
{
    public CourseInfo CourseInfo { get; init; }
    public CourseCapacity Capacity { get; init; }
    public List<CourseSchedule> Schedule { get; init; }
}