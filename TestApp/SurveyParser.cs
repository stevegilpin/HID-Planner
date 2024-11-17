using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestApp;

public record SurveyResponse(string LastName, string FirstName, int Grade, string Email, List<string> ClassRankings);

public static class SurveyParser
{
    public static List<SurveyResponse> ParseResults(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var header = lines[0].Split(',').ToList();
        var lastNameIndex = header.IndexOf("Last Name");
        var firstNameIndex = header.IndexOf("First Name");
        var gradeIndex = header.IndexOf("Grade");
        var emailIndex = header.IndexOf("Email Address");

        var rowDataStart = new[] { lastNameIndex, firstNameIndex, gradeIndex, emailIndex }.Max() + 1;
        var responses = new List<SurveyResponse>();
        foreach (var line in lines.Skip(1)) {
            var data = line.Split(',').ToList();
            var rankings = data
                .Where((_, i) => i >= rowDataStart)
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .ToList();

            responses.Add(new SurveyResponse(
                data[lastNameIndex],
                data[firstNameIndex],
                int.Parse(data[gradeIndex]),
                data[emailIndex],
                rankings));
        }

        return responses;
    }
}