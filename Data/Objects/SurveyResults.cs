using System.Collections.Generic;

namespace Data.Objects;

public class SurveyResponse(string lastName, string firstName, int grade, string email, List<string> rankings)
{
    public string LastName { get; set; } = lastName;
    public string FirstName { get; set; } = firstName;
    public int Grade { get; set; } = grade;
    public string Email { get; set; } = email;
    public List<string> ClassRankings { get; set; } = rankings;
}