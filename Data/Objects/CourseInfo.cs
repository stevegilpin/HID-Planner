namespace Data.Objects;

public class CourseInfo
{
    public int CourseScheduleInputId { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public string Description { get; set; }
    public int? GradesFlags { get; set; }
}