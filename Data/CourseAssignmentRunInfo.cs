using System;
using System.ComponentModel.DataAnnotations;

namespace Data;

public class CourseAssignmentRunInfo
{
    [Key]
    public int RunId { get; set; }
    public DateTime RunTimeStamp { get; set; }
    public int StudentRankingInputId { get; set; }
    public int CourseScheduleInputId { get; set; }
    public string Description { get; set; }
}