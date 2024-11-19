using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class CourseOptionsReport(List<CourseInfo> courseData, string gradeFilter = null) : IHtmlGenerator
{
    public readonly List<CourseInfo> CourseData = courseData;
    public readonly string GradeFilter = gradeFilter;

    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Course Options");
        doc.StyleRuleSets.Add(CSS.PageBreak());

        var minGrade = CourseData.Min(x => x.MinGrade);
        var maxGrade = CourseData.Max(x => x.MaxGrade);
        var grades = Enumerable.Range(minGrade, maxGrade - minGrade + 1);
        foreach (var grade in grades) {
            if (string.IsNullOrEmpty(GradeFilter) == false && GradeToString(grade) != GradeFilter)
                continue;

            doc.Add(GetGradeSection(grade));
            doc.Add(new HtmlPageBreak());
        }

        return doc;
    }

    private XElement GetGradeSection(int grade)
    {
        var eligibleCourses = CourseData.Where(x => x.MinGrade <= grade && x.MaxGrade >= grade).ToList();
        if (eligibleCourses.Count == 0)
            return new XElement("div");

        var section = new XElement("div");
        var gradeCode = GradeToString(grade);
        section.Add(new HtmlHeader($"Class Options for Grade {gradeCode}", 2));

        foreach (var course in eligibleCourses.OrderBy(x => x.CourseName)) {
            section.Add(new HtmlHeader(course.CourseName, 4));
            section.Add(new HtmlParagraph(course.Description));
        }

        return section;
    }

    private static string GradeToString(int gradeNum)
    {
        return gradeNum > 0 ? gradeNum.ToString() : $"K{5 + gradeNum}";
    }
}