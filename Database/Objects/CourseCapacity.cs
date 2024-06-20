using System.ComponentModel.DataAnnotations;

namespace Database.Objects;

public class CourseCapacity
{
    public int CourseScheduleInputId { get; set; }
    public string CourseCode { get; set; }
    public int MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
}