using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
            .OrderBy(g => reportData.InputData.CourseInfo[g.Key].CourseName)
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

            HtmlCell firstNameCell;
            HtmlCell lastNameCell;
            if (instructors.Count == 0) {
                firstNameCell = new HtmlCell(string.Empty);
                lastNameCell = new HtmlCell(string.Empty);
            }
            else {
                var divFirst = new XElement("div");
                var divLast = new XElement("div");
                firstNameCell = new HtmlCell(divFirst);
                lastNameCell = new HtmlCell(divLast);
                for (int i = 0; i < instructorIds.Count; i++) {
                    divFirst.Add(instructorIds[i].FirstName);
                    divLast.Add(instructorIds[i].LastName);
                    if (i < assignedSessions.Count - 1) {
                        divFirst.Add(new HtmlBreak());
                        divLast.Add(new HtmlBreak());
                    }
                }
            }

            var room = courseInfo.RoomCode ?? "TBD";
            var roomDescription = RoomMap.GetValueOrDefault(courseInfo.RoomCode)?.Description ?? string.Empty;
            var rowData = new List<HtmlCell> { new(courseInfo.CourseName), firstNameCell, lastNameCell, new(room), new(roomDescription) };
            rowData.AddRange(allSessions.Select(s => assignedSessions.Contains(s) ? new HtmlCell(s) : new HtmlCell(string.Empty)));
            table.AddRow(rowData);
        }

        return doc;
    }
}