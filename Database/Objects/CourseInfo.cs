using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace Database.Objects;

public class CourseInfo
{
    public string CourseCode { get; set; }
    [Optional]
    public string EventId { get; set; }
    [Optional]
    public string CourseName { get; set; }
    [Optional]
    public string Description { get; set; }
    [Optional]
    public int? GradesFlags { get; set; }
}