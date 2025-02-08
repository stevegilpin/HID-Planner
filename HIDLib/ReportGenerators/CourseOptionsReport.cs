using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class CourseOptionsReport(List<CourseInfo> courseData, string docHeader, string eventDescription, string gradeFilter = null) : IHtmlGenerator
{
    public readonly List<CourseInfo> CourseData = courseData;
    public readonly string DocumentHeader = docHeader;
    public readonly string EventDescription = eventDescription;
    public readonly string GradeFilter = gradeFilter;

    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Course Options");
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add("""
                              li {
                                  padding: 5px;
                              }
                              """);
        var headerLines = DocumentHeader.Split(Environment.NewLine).ToList();
        var header = doc.Add(new HtmlHeader(string.Empty));
        foreach (var line in headerLines) {
            header.Add(line);
            header.Add(new HtmlBreak());
        }
        // var header = doc.Add(new HtmlHeader(string.Join("<br/>", headerLines)));
        header.AddStyle("text-align: center");

        var descriptionLines = EventDescription.Split(Environment.NewLine).ToList();
        var desc = doc.Add(new HtmlParagraph(string.Empty));
        foreach (var line in descriptionLines) {
            desc.Add(line);
            desc.Add(new HtmlBreak());
        }
        // doc.Add(new HtmlParagraph(string.Join("<br/>", descriptionLines)));

        var grades = GetGrades();
        foreach (var grade in grades) {
            if (string.IsNullOrEmpty(GradeFilter) == false && GradeToString(grade) != GradeFilter)
                continue;

            doc.Add(GetGradeSection(grade));
            doc.Add(new HtmlPageBreak());
        }

        return doc;
    }

    public string CreateTextFileContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine(DocumentHeader);
        sb.AppendLine();
        sb.AppendLine(EventDescription);
        sb.AppendLine();

        var grades = GetGrades();
        foreach (var grade in grades) {
            var eligibleCourses = CourseData.Where(x => x.MinGrade <= grade && x.MaxGrade >= grade).ToList();
            if (eligibleCourses.Count == 0)
                continue;

            var gradeCode = GradeToString(grade);
            sb.AppendLine($"Class Options for Grade {gradeCode}");

            foreach (var course in eligibleCourses.OrderBy(x => x.CourseName))
                sb.AppendLine($"{course.CourseName}: {course.Description}");
        }

        return sb.ToString();
    }

    private IEnumerable<int> GetGrades()
    {
        var minGrade = CourseData.Min(x => x.MinGrade);
        var maxGrade = CourseData.Max(x => x.MaxGrade);
        return Enumerable.Range(minGrade, maxGrade - minGrade + 1);
    }

    private XElement GetGradeSection(int grade)
    {
        var eligibleCourses = CourseData.Where(x => x.MinGrade <= grade && x.MaxGrade >= grade).ToList();
        if (eligibleCourses.Count == 0)
            return new XElement("div");

        var section = new XElement("div");
        var gradeCode = GradeToString(grade);
        section.Add(new HtmlHeader($"Class Options for Grade {gradeCode}", 2));

        var list = new HtmlList();
        section.Add(list);

        foreach (var course in eligibleCourses.OrderBy(x => x.CourseName)) {
            var itemContent = new XElement("div");
            itemContent.Add(new XElement("span", new XAttribute("style", "font-weight: bold;"), $"{course.CourseName}: "));
            itemContent.Add(new XElement("span", course.Description));
            list.Add(new HtmlListItem(itemContent));
        }

        return section;
    }

    private static string GradeToString(int gradeNum)
    {
        return gradeNum > 0 ? gradeNum.ToString() : $"K{5 + gradeNum}";
    }
}