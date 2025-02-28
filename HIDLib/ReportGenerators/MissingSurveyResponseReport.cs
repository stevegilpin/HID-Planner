using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class MissingSurveyResponseReport(List<StudentInfo> missingStudents) : IHtmlGenerator
{
    private readonly List<StudentInfo> MissingStudents = missingStudents;
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Missing Survey Responses");

        doc.Add(new HtmlHeader("Missing Survey Responses"));
        if (MissingStudents.Count == 0) {
            doc.Add(new HtmlParagraph("No missing students!"));
            return doc;
        }

        doc.Add(new HtmlBreak());

        var homeroomGroups = MissingStudents
            .GroupBy(x => new {x.Grade, x.Homeroom})
            .OrderBy(x => x.Key.Grade)
            .ThenBy(x => x.Key.Homeroom)
            .ToList();
        foreach (var group in homeroomGroups) {
            doc.Add(new HtmlHeader($"Grade {group.Key.Grade} - Homeroom {group.Key.Homeroom}", 3));
            var list = doc.Add(new HtmlList());
            foreach (var student in group)
                list.Add(new HtmlListItem($"{student.FirstName} {student.LastName}"));
        }

        return doc;
    }
}