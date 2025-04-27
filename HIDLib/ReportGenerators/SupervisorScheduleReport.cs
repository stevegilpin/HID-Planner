using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class SupervisorScheduleReport(AssignmentDataSet reportData, List<Rooms> rooms, List<Supervisor> supervisors) : IHtmlGenerator
{
    private Dictionary<string, Rooms> Rooms = rooms.ToDictionary(x => x.RoomCode);
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Supervisor Schedules");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlTableGrid.Css());
        doc.StyleRuleSets.Add(CSS.GlobalFontSize(11));
        doc.StyleRuleSets.Add(CSS.EllipsisOverflow());

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
        
        var grid = doc.Add(new HtmlTableGrid());
        const int numColumns = 2;
        var rowData = new List<XElement>();
        var allSessions = reportData.AllSessions();
        for (var i = 0; i < supervisorCourseGroups.Count; i++) {
            var group = supervisorCourseGroups[i];
            var supervisor = supervisorMap[group.Key];
            var header = new HtmlHeader($"{supervisor.FirstName} {supervisor.LastName}", 3);
            var table = GetSupervisorTable(group.Select(x => x.CourseSchedule).ToList(), allSessions);
            table.AddColGroup(1, ("style", "width: 5%"));
            rowData.Add(new XElement("div", header, table));
            if (rowData.Count == numColumns || i == supervisorCourseGroups.Count - 1) {
                grid.AddRow(rowData.ToList());
                rowData.Clear();
            }
        }
        
        return doc;
    }
    
    private HtmlTable GetSupervisorTable(List<CourseSchedule> courses, List<int> allSessions)
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