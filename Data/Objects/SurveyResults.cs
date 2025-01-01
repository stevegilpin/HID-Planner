using System.Collections.Generic;

namespace Data.Objects;

public record SurveyResponse(string LastName, string FirstName, int Grade, string Email, List<string> ClassRankings);