using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
public class StudentScheduleOverviewReport(
    AssignmentDataSet reportData,
    List<Rooms> rooms,
    bool forStudentHandouts = false
    ) : IHtmlGenerator
{
    private readonly AssignmentDataSet ReportData = reportData;
    private readonly Dictionary<string, Rooms> Rooms = rooms.ToDictionary(x => x.RoomCode);
    private readonly bool SortByHomeroom = forStudentHandouts;
    public HtmlDoc CreateDoc()
    {
        var title = SortByHomeroom ? "Student Schedule Overview - By Homeroom" : "Student Schedule Overview"; 
        var doc = new HtmlDoc(title);
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlTableGrid.Css());
        if (forStudentHandouts) {
            doc.StyleRuleSets.Add(@"* {
              font-size: 10px;
            }");
        }

        if (!forStudentHandouts)
            doc.Add(new HtmlHeader("Student Schedules"));

        List<int> studentIds;
        if (SortByHomeroom) {
            studentIds = ReportData.InputData.StudentInfo
                .OrderBy(x => x.Value.Homeroom)
                .ThenBy(x => x.Value.LastName)
                .ThenBy(x => x.Value.FirstName)
                .Select(x => x.Key)
                .ToList();
        } else {
            studentIds = ReportData.InputData.StudentInfo
                .OrderBy(x => x.Value.LastName)
                .ThenBy(x => x.Value.FirstName)
                .Select(x => x.Key)
                .ToList();
        }

        var grid = doc.Add(new HtmlTableGrid());
        const int numColumns = 2;
        var rowData = new List<XElement>();
        for (var i = 0; i < studentIds.Count; i++) {
            var id = studentIds[i];
            var student = ReportData.InputData.StudentInfo[id];
            var header = new HtmlHeader($"{student.LastName}, {student.FirstName} (HR: {student.Homeroom})", 3);
            if (forStudentHandouts)
                header.AddStyle("font-size: 15px");
            var table = GetStudentTable(id);
            table.AddColGroup(1, ("style", forStudentHandouts ? "width: 5%" : "width: 20%"));
            var cell = new XElement("div", header, table);
            rowData.Add(cell);
            if (rowData.Count == numColumns || i == studentIds.Count - 1) {
                grid.AddRow(rowData.ToList());
                rowData.Clear();
            }
        }

        return doc;
    }

    private HtmlTable GetStudentTable(int studentId)
    {
        var assigned = ReportData.GetStudentsAssignments(studentId)
            .OrderBy(x => x.SessionId)
            .ToList();

        var table = new HtmlTable(forStudentHandouts ? null : ["Session", "Class", "Room"]);
        foreach (var course in assigned) {
            var courseInfo = ReportData.InputData.CourseInfo[course.CourseCode];
            var courseName = courseInfo.CourseName;
            var room = courseInfo.RoomCode ?? "TBD";
            if (Rooms.TryGetValue(room, out var roomDesc) && string.IsNullOrEmpty(roomDesc.Description) == false)
                room += $" - {roomDesc.Description}";
            table.AddRowData([course.SessionId, courseName, room]);
        }

        return table;
    } 
}