using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class InstructorScheduleReport(AssignmentDataSet reportData, List<Rooms> rooms, List<Instructor> instructors) : IHtmlGenerator
{
    private Dictionary<string, Rooms> Rooms = rooms.ToDictionary(x => x.RoomCode);
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Instructor Schedules");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlTableGrid.Css());
        doc.StyleRuleSets.Add(CSS.GlobalFontSize(11));
        doc.StyleRuleSets.Add(CSS.EllipsisOverflow());

        var instructorMap = instructors.ToDictionary(x => x.InstructorId);
        var instructorCourseGroups = reportData.InputData.CourseSchedule
            .SelectMany(x => x.Value)
            .Where(x => string.IsNullOrEmpty(x.Instructor) == false)
            .Select(x => new {
                Instructor = instructorMap[x.Instructor],
                CourseSchedule = x,
            })
            .GroupBy(x => x.Instructor.InstructorId)
            .OrderBy(g => instructorMap[g.Key].LastName)
            .ThenBy(g => instructorMap[g.Key].FirstName)
            .ToList();
        
        var grid = doc.Add(new HtmlTableGrid());
        const int numColumns = 2;
        var rowData = new List<XElement>();
        var allSessions = reportData.AllSessions();
        for (var i = 0; i < instructorCourseGroups.Count; i++) {
            var group = instructorCourseGroups[i];
            var instructor = instructorMap[group.Key];
            var header = new HtmlHeader($"{instructor.FirstName} {instructor.LastName}", 3);
            header.AddStyle("font-size: 15px");
            var table = GetInstructorTable(group.Select(x => x.CourseSchedule).ToList(), allSessions);
            table.AddColGroup(1, ("style", "width: 5%"));
            rowData.Add(new XElement("div", header, table));
            if (rowData.Count == numColumns || i == instructorCourseGroups.Count - 1) {
                grid.AddRow(rowData.ToList());
                rowData.Clear();
            }
        }
        
        return doc;
    }
    
    private HtmlTable GetInstructorTable(List<CourseSchedule> courses, List<int> allSessions)
    {
        var table = new HtmlTable();
        var assigned = courses.ToDictionary(x => x.SessionId);
        foreach (var session in allSessions) {
            if (assigned.TryGetValue(session, out var course) == false) {
                table.AddRowData([session, string.Empty, string.Empty]);
                continue;
            }

            var courseInfo = reportData.InputData.CourseInfo[course.CourseCode];
            var courseName = courseInfo.CourseName;
            var room = courseInfo.RoomCode ?? "TBD";
            if (Rooms.TryGetValue(room, out var roomDesc) && string.IsNullOrEmpty(roomDesc.Description) == false)
                room += $" - {roomDesc.Description}";
            table.AddRowData([course.SessionId, courseName, room]);
        }

        return table;
    } 
}