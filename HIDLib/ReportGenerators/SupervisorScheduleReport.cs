using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class SupervisorScheduleReport(AssignmentDataSet reportData) : IHtmlGenerator
{
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Supervisor Schedules");
        doc.StyleRuleSets.Add(CSS.TableStyles());
        doc.Add(new HtmlHeader("Supervisors' Schedules"));

        var supervisorCourseGroups = reportData.InputData.CourseSchedule
            .SelectMany(x => x.Value)
            .Where(x => string.IsNullOrEmpty(x.Supervisor) == false)
            .GroupBy(x => x.Supervisor)
            .OrderBy(g => g.Key)
            .ToList();
        
        var sessions = reportData.AllSessions();
        var tableHeader = new List<object> { "Supervisor" };
        tableHeader.AddRange(sessions.Select(x => $"Session {x}"));
        var table = doc.Add(new HtmlTable(tableHeader));

        foreach (var supervisorCourses in supervisorCourseGroups) {
            var rowData = new List<object> { supervisorCourses.Key };
            foreach (var session in sessions) {
                var course = supervisorCourses.SingleOrDefault(x => x.SessionId == session);
                rowData.Add(course?.CourseCode ?? string.Empty);
            }
            table.AddRowData(rowData);
        }

        return doc;
    }
}