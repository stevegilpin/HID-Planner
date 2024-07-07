using Microsoft.EntityFrameworkCore;

namespace Data;

public class HidDbContext : DbContext
{
    public DbSet<CourseAssignmentResult> CourseAssignmentResult { get; set; }
    public DbSet<CourseAssignmentRunInfo> CourseAssignmentRunInfo { get; set; }
    public DbSet<CourseInfo> CourseInfo { get; set; }
    public DbSet<CourseSchedule> CourseSchedule { get; set; }
    public DbSet<CourseScheduleInputDescription> CourseScheduleInputDescription { get; set; }
    public DbSet<Rooms> Rooms { get; set; }
    public DbSet<StudentCourseRanking> StudentCourseRanking { get; set; }
    public DbSet<StudentInfo> StudentInfo { get; set; }
    public DbSet<StudentRankingInputDescription> StudentRankingInputDescription { get; set; }

    public HidDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentCourseRanking>().HasKey(m => new { m.RankingInputId , m.StudentId, m.CourseCode });
        modelBuilder.Entity<CourseInfo>().HasKey(m => new { m.CourseScheduleInputId, m.CourseCode });
        modelBuilder.Entity<CourseSchedule>().HasKey(m => new { m.CourseScheduleInputId , m.CourseCode, m.SessionId });
        modelBuilder.Entity<CourseAssignmentResult>().HasKey(m => new { m.RunId , m.StudentId, m.SessionId });
        
        base.OnModelCreating(modelBuilder);
    }
}