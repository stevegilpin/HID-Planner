using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
public class StudentScheduleOverviewReport(AssignmentDataSet reportData) : IHtmlGenerator
{
    private readonly AssignmentDataSet ReportData = reportData;
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Student Schedule Overview");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlTableGrid.Css());

        doc.Add(new HtmlHeader("Student Schedules"));

        var studentIds = ReportData.InputData.StudentInfo
            .OrderBy(x => x.Value.LastName)
            .ThenBy(x => x.Value.FirstName)
            .Select(x => x.Key)
            .ToList();

        var grid = doc.Add(new HtmlTableGrid());
        const int numColumns = 2;
        var rowData = new List<XElement>();
        foreach (var id in studentIds) {
            var student = ReportData.InputData.StudentInfo[id];
            var header = new HtmlHeader($"{student.LastName}, {student.FirstName} (HR: {student.Homeroom})", 3);
            var table = GetStudentTable(id);
            var cell = new XElement("div", header, table);
            rowData.Add(cell);
            if (rowData.Count == numColumns) {
                grid.AddRow(rowData.ToList());
                rowData.Clear();
            }
        }

        return doc;
    }

    private HtmlTable GetStudentTable(int studentId)
    {
        var assigned = ReportData.AssignmentResults
            .Where(x => x.StudentId == studentId)
            .OrderBy(x => x.SessionId)
            .ToList();

        var table = new HtmlTable(["Session", "Class", "Room"]);
        foreach (var course in assigned) {
            var courseName = ReportData.InputData.CourseInfo[course.CourseCode].CourseName;
            var room = ReportData.InputData.CourseSchedule[course.CourseCode].Single(x => x.SessionId == course.SessionId).RoomCode;
            table.AddRowData([course.SessionId, courseName, room ?? "TBD"]);
        }

        return table;
    } 
}