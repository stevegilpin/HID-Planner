using System.ComponentModel.DataAnnotations;

namespace Data.Objects;

public class Instructor
{
    [Key]
    public string InstructorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}