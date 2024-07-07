using System.ComponentModel.DataAnnotations;

namespace Data;

public class StudentInfo
{
    public int RankingInputId { get; set; }
    public int StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Grade { get; set; }
    public string Homeroom { get; set; }
}