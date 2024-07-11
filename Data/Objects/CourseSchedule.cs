namespace Data.Objects;

public class CourseSchedule
{
    public int CourseScheduleInputId { get; set; }
    public string CourseCode { get; set; }
    public int SessionId { get; set; }
    public string RoomCode { get; set; }
    public string Instructor { get; set; }
    public string Supervisor { get; set; }
}