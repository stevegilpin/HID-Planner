using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class InstructorScheduleReport(AssignmentDataSet reportData, List<Instructor> instructors) : IHtmlGenerator
{
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Instructor Schedules");
        doc.StyleRuleSets.Add(CSS.TableStyles());
        doc.Add(new HtmlHeader("Instructor' Schedules"));

        var instructorMap = instructors.ToDictionary(x => x.InstructorId);
        var instructorCourseGroups = reportData.InputData.CourseSchedule
            .SelectMany(x => x.Value)
            .Where(x => x.Instructor.HasValue)
            .GroupBy(x => x.Instructor.Value)
            .OrderBy(g => g.Key)
            .ToList();

        var sessions = reportData.AllSessions();
        var tableHeader = new List<object> { "Instructor" };
        tableHeader.AddRange(sessions.Select(x => $"Session {x}"));
        var table = doc.Add(new HtmlTable(tableHeader));

        foreach (var instructorCourses in instructorCourseGroups) {
            var rowData = new List<HtmlCell> { new(instructorMap[instructorCourses.Key]) };
            foreach (var session in sessions) {
                var course = instructorCourses.SingleOrDefault(x => x.SessionId == session);
                var cell = new HtmlCell(course?.CourseCode ?? string.Empty);
                rowData.Add(cell);
                if (course != null) {
                    var courseInfo = reportData.InputData.CourseInfo[course.CourseCode];
                    cell.Add(new HtmlBreak());
                    cell.Add(string.IsNullOrEmpty(courseInfo.RoomCode) ? "Room TBD" : courseInfo.RoomCode);
                }
            }
            table.AddRow(rowData);
        }

        return doc;
    }
}