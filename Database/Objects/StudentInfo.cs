using System.ComponentModel.DataAnnotations;

namespace Database.Objects;

public class StudentInfo
{
    [Key]
    public int StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Grade { get; set; }
    public string Homeroom { get; set; }
}