using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class InstructorScheduleReport(AssignmentDataSet reportData) : IHtmlGenerator
{
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Instructor Schedules");
        doc.StyleRuleSets.Add(CSS.TableStyles());
        doc.Add(new HtmlHeader("Instructor' Schedules"));

        var instructorCourseGroups = reportData.InputData.CourseSchedule
            .SelectMany(x => x.Value)
            .Where(x => string.IsNullOrEmpty(x.Instructor) == false)
            .GroupBy(x => x.Instructor)
            .OrderBy(g => g.Key)
            .ToList();

        var sessions = reportData.AllSessions();
        var tableHeader = new List<object> { "Instructor" };
        tableHeader.AddRange(sessions.Select(x => $"Session {x}"));
        var table = doc.Add(new HtmlTable(tableHeader));

        foreach (var instructorCourses in instructorCourseGroups) {
            var rowData = new List<object> { instructorCourses.Key };
            foreach (var session in sessions) {
                var course = instructorCourses.SingleOrDefault(x => x.SessionId == session);
                rowData.Add(course?.CourseCode ?? string.Empty);
            }
            table.AddRowData(rowData);
        }

        return doc;
    }
}