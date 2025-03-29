using System.ComponentModel.DataAnnotations;

namespace Data.Objects;

public class Instructor
{
    [Key]
    public int InstructorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}