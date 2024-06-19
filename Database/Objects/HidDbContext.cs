using Microsoft.EntityFrameworkCore;

namespace Database.Objects;

public class HidDbContext : DbContext
{
    public DbSet<CourseAssignmentResult> CourseAssignmentResult { get; set; }
    public DbSet<CourseAssignmentRunInfo> CourseAssignmentRunInfo { get; set; }
    public DbSet<CourseCapacity> CourseCapacity { get; set; }
    public DbSet<CourseInfo> CourseInfo { get; set; }
    public DbSet<CourseSchedule> CourseSchedule { get; set; }
    public DbSet<CourseScheduleInputDescription> CourseScheduleInputDescription { get; set; }
    public DbSet<Rooms> Rooms { get; set; }
    public DbSet<StudentCourseRanking> StudentCourseRanking { get; set; }
    public DbSet<StudentInfo> StudentInfo { get; set; }
    public DbSet<StudentRankingInputDescription> StudentRankingInputDescription { get; set; }
}