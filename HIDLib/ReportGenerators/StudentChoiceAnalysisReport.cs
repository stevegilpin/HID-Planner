using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class StudentChoiceAnalysisReport(AssignmentDataSet reportData) : IHtmlGenerator
{
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Student Choice Analysis");
        doc.StyleRuleSets.Add(CSS.TableStyles());
        doc.Add(new HtmlHeader("Student Choice Analysis"));

        var courseData = GetCourseChoiceFrequencies();
        var table = doc.Add(new HtmlTable());
        table.AddHeaderRow(["Course Name", "Course Code", "Total 1st - 5th", "Total 6th - 10th", "Capacity"]);
        foreach (var course in courseData) {
            table.AddRowData([
                course.CourseInfo.CourseName,
                course.CourseInfo.CourseCode,
                course.Top5Frequency,
                course.Second5Frequency,
                course.CourseInfo.MaxCapacity?.ToString() ?? "TBD"
            ]);
        }

        return doc;
    }

    private List<FrequencyData> GetCourseChoiceFrequencies()
    {
        var allRankings = reportData.InputData.StudentCourseRanking.SelectMany(x => x.Value).ToList();
        var courseInfo = reportData.InputData.CourseInfo;
        return allRankings
            .GroupBy(x => x.CourseCode)
            .Select(g => new FrequencyData(courseInfo[g.Key], g.Count(x => x.Rank <= 5), g.Count(x => x.Rank > 5 && x.Rank <= 10)))
            .Where(x => x.CourseInfo.CourseName != "UNASSIGNED")
            .OrderBy(x => x.CourseInfo.CourseCode)
            .ToList();
    }

    private record FrequencyData(CourseInfo CourseInfo, int Top5Frequency, int Second5Frequency);
}