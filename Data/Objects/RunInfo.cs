using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Objects;

public class RunInfo
{
    [Key]
    public int RunId { get; set; }
    public DateTime RunTimeStamp { get; set; }
    public bool ManuallyCreatedResults { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
}