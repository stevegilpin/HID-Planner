using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public static class StudentScheduleOverviewReport
{
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Student Schedule Overview");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlTableGrid.Css());

        doc.Add(new HtmlHeader("Student Schedules"));

        var studentIds = outputData.StudentInputs
            .OrderBy(x => x.Value.StudentInfo.LastName)
            .ThenBy(x => x.Value.StudentInfo.FirstName)
            .Select(x => x.Key)
            .ToList();

        var grid = doc.Add(new HtmlTableGrid());
        const int numColumns = 2;
        var rowData = new List<XElement>();
        foreach (var id in studentIds) {
            var student = outputData.StudentInputs[id].StudentInfo;
            var header = new HtmlHeader($"{student.LastName}, {student.FirstName} (HR: {student.Homeroom})", 3);
            var table = GetStudentTable(id, outputData).ToXmlNode();
            var cell = new XElement("div", header, table);
            rowData.Add(cell);
            if (rowData.Count == numColumns) {
                grid.AddRow(rowData);
                rowData.Clear();
            }
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