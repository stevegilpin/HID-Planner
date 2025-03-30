using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Data.Objects;

public class CourseSchedule
{
    public int RunId { get; set; }
    public string CourseCode { get; set; }
    public int SessionId { get; set; }
    public string Instructor { get; set; }
    [Description("List of supervisor IDs delimited by ';' character")]
    public string Supervisor { get; set; }
    [NotMapped]
    public string[] SupervisorIds => Supervisor?.Split(';').Where(x => string.IsNullOrWhiteSpace(x) == false).ToArray() ?? [];
}