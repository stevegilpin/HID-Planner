using System.ComponentModel.DataAnnotations;

namespace Database.Objects;

public class StudentInfo
{
    [Key]
    public int StudentId { get; set; }
    public string Name { get; set; }
    public string Grade { get; set; }
    public string Gender { get; set; }
}