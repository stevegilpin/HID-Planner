using Data.Objects;
using Microsoft.EntityFrameworkCore;

namespace Data.Database;

internal class HidDbContext : DbContext
{
    public DbSet<CourseAssignmentResult> CourseAssignmentResult { get; set; }
    public DbSet<CourseInfo> CourseInfo { get; set; }
    public DbSet<CourseSchedule> CourseSchedule { get; set; }
    public DbSet<Rooms> Rooms { get; set; }
    public DbSet<RunInfo> RunInfo { get; set; }
    public DbSet<StudentCourseRanking> StudentCourseRanking { get; set; }
    public DbSet<StudentInfo> StudentInfo { get; set; }
    public DbSet<Settings> Settings { get; set; }

    public HidDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentInfo>().HasKey(m => new { m.RunId, m.StudentId });
        modelBuilder.Entity<StudentCourseRanking>().HasKey(m => new { m.RunId, m.StudentId, m.CourseCode });
        modelBuilder.Entity<CourseInfo>().HasKey(m => new { m.RunId, m.CourseCode });
        modelBuilder.Entity<CourseSchedule>().HasKey(m => new { m.RunId , m.CourseCode, m.SessionId });
        modelBuilder.Entity<CourseAssignmentResult>().HasKey(m => new { m.RunId , m.StudentId, m.SessionId });
        
        base.OnModelCreating(modelBuilder);
    }
}