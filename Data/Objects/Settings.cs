using System.ComponentModel.DataAnnotations;

namespace Data.Objects;

public class Settings
{
    [Key]
    public string Name { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public bool IsWritable { get; set; }
}