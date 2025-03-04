namespace Data.Objects;

public class CourseInfo
{
    public int RunId { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public string Description { get; set; }
    public int MinGrade { get; set; }
    public int MaxGrade { get; set; }
    public string RoomCode { get; set; }
}