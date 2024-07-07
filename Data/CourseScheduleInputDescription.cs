using System.ComponentModel.DataAnnotations;

namespace Data;

public class CourseScheduleInputDescription
{
    [Key]
    public int InputId { get; set; }
    public string EventId { get; set; }
    public string Description { get; set; }
}