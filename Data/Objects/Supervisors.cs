using System.ComponentModel.DataAnnotations;

namespace Data.Objects;

public class Supervisor
{
    [Key]
    public string SupervisorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string AlternateLastName { get; set; }
}