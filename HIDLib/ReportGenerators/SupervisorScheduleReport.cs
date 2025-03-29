using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class SupervisorScheduleReport(AssignmentDataSet reportData, List<Supervisor> supervisors) : IHtmlGenerator
{
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Supervisor Schedules");
        doc.StyleRuleSets.Add(CSS.TableStyles());
        doc.Add(new HtmlHeader("Supervisors' Schedules"));

        var supervisorMap = supervisors.ToDictionary(x => x.SupervisorId);
        var supervisorCourseGroups = reportData.InputData.CourseSchedule
            .SelectMany(x => x.Value)
            .Where(x => x.SupervisorIds.Length != 0)
            .SelectMany(x => x.SupervisorIds.Select(i => new {
                Supervisor = supervisorMap[i],
                CourseSchedule = x,
            }))
            .GroupBy(x => x.Supervisor.SupervisorId)
            .OrderBy(g => supervisorMap[g.Key].LastName)
            .ThenBy(g => supervisorMap[g.Key].FirstName)
            .ToList();
        
        var sessions = reportData.AllSessions();
        var tableHeader = new List<object> { "Supervisor" };
        tableHeader.AddRange(sessions.Select(x => $"Session {x}"));
        var table = doc.Add(new HtmlTable(tableHeader));

        foreach (var supervisorCourses in supervisorCourseGroups) {
            var supervisor = supervisorMap[supervisorCourses.Key];
            var rowData = new List<HtmlCell> { new($"{supervisor.FirstName} {supervisor.LastName}") };
            foreach (var session in sessions) {
                var course = supervisorCourses.SingleOrDefault(x => x.CourseSchedule.SessionId == session);
                var cell = new HtmlCell(course?.CourseSchedule.CourseCode ?? string.Empty);
                rowData.Add(cell);
                if (course != null) {
                    var courseInfo = reportData.InputData.CourseInfo[course.CourseSchedule.CourseCode];
                    cell.Add(new HtmlBreak());
                    cell.Add(string.IsNullOrEmpty(courseInfo.RoomCode) ? "Room TBD" : courseInfo.RoomCode);
                }
            }
            table.AddRow(rowData);
        }

        return doc;
    }
}