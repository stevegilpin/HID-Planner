using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class ClassListForEveryone(AssignmentDataSet reportData, List<Rooms> rooms, List<Instructor> instructors) : IHtmlGenerator
{
    private Dictionary<string, Instructor> InstructorMap = instructors.ToDictionary(x => x.InstructorId);
    private Dictionary<string, Rooms> RoomMap = rooms.ToDictionary(x => x.RoomCode);
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Class List for Everyone");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.TableStyles());
        doc.StyleRuleSets.Add(CSS.EllipsisOverflow());
        
        doc.Add(new HtmlHeader("Class List"));
        var allSessions = reportData.AllSessions();
        var table = doc.Add(new HtmlTable());
        var row = table.AddHeaderRow(["Class", "Instructor", "Classroom", "Sessions Available"]); // todo merge cells
        row.Data[1].AddAttribute("colspan", "2");
        row.Data[2].AddAttribute("colspan", "2");
        row.Data[3].AddAttribute("colspan", $"{allSessions.Count}");
        row.Data[3].AddAttribute("style", "text-align: center");
        table.AddColGroup(1, ("style", "width: 20%"));
        table.AddColGroup(2, ("style", "width: 10%"));
        table.AddColGroup(1, ("style", "width: 10%"));
        table.AddColGroup(1, ("style", "width: 20%"));
        table.AddColGroup(allSessions.Count, ("style", "width: 2%"));

        var assignedCourses = reportData.AssignmentResults
            .GroupBy(x => x.CourseCode)
            .OrderBy(g => reportData.InputData.CourseInfo[g.Key].Description)
            .ToList();

        foreach (var group in assignedCourses) {
            var courseInfo = reportData.InputData.CourseInfo[group.Key];
            var assignedSessions = group.Select(x => x.SessionId).Distinct().ToList();
            var courseScheduleInfo = reportData.InputData.CourseSchedule[group.Key].ToDictionary(x => x.SessionId);
            var instructorIds = assignedSessions
                .Select(x => courseScheduleInfo[x].Instructor)
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => InstructorMap[x])
                .ToList();

            var instructorFirstName = instructors.Any() ?
                string.Join("\n", instructorIds.Select(x => x.FirstName)) :
                string.Empty;

            var instructorLastName = instructors.Any() ?
                string.Join("\n", instructorIds.Select(x => x.LastName)) :
                string.Empty;

            var room = courseInfo.RoomCode ?? "TBD";
            var roomDescription = RoomMap.GetValueOrDefault(courseInfo.RoomCode)?.Description ?? string.Empty;

            var rowData = new List<object> { courseInfo.CourseName, instructorFirstName, instructorLastName, room, roomDescription };
            foreach (var s in allSessions) {
                if (assignedSessions.Contains(s))
                    rowData.Add(s);
                else
                    rowData.Add(string.Empty);
            }

            table.AddRowData(rowData);
        }

        return doc;
    }
}