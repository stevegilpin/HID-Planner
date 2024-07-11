using System.ComponentModel.DataAnnotations;

namespace Data.Objects;

public class Rooms
{
    [Key]
    public string RoomCode { get; set; }
    public string Description { get; set; }
}