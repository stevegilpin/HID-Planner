using System;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;

namespace HIDLib.ReportGenerators;

public static class StudentScheduleOverviewReport
{
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Student Schedule Overview");
        doc.StyleRuleSets.Add(HtmlUtil.CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlUtil.CSS.PageBreak());
        doc.StyleRuleSets.Add(HtmlUtil.CSS.TableStyles());

        doc.Add(HtmlUtil.Header("Student Schedules"));

        var studentIds = outputData.StudentInputs
            .OrderBy(x => x.Value.StudentInfo.LastName)
            .ThenBy(x => x.Value.StudentInfo.FirstName)
            .Select(x => x.Key)
            .ToList();

        var grid = doc.Add(new HtmlGridLayout(2));
        foreach (var id in studentIds) {
            var student = outputData.StudentInputs[id].StudentInfo;
            var header = HtmlUtil.Header($"{student.LastName}, {student.FirstName} (HR: {student.Homeroom})", 3);
            var table = GetStudentTable(id, outputData).ToXmlNode();
            var cell = new XElement("div", header, table);
            grid.AddCell(cell);
        }

        return doc;
    }

    private static HtmlTable GetStudentTable(int studentId, SchedulingOutputData outputData)
    {
        var assigned = outputData.AssignmentResults
            .Where(x => x.StudentId == studentId)
            .OrderBy(x => x.SessionId)
            .ToList();

        var table = new HtmlTable();
        table.AddColumnHeaders(["Session", "Class", "Room"]);

        foreach (var course in assigned) {
            var courseName = outputData.CourseInputs[course.CourseCode].CourseInfo.CourseName;
            var room = outputData.CourseInputs[course.CourseCode].Schedule.Single(x => x.SessionId == course.SessionId).RoomCode;
            table.AddRowData([course.SessionId, courseName, room ?? "TBD"]);
        }

        return table;
    } 
}