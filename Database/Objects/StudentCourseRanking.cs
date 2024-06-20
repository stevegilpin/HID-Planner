using System.ComponentModel.DataAnnotations;

namespace Database.Objects;

public class StudentCourseRanking
{
    public int RankingInputId { get; set; }
    public int StudentId { get; set; }
    public string CourseCode { get; set; }
    public int Rank { get; set; }
}