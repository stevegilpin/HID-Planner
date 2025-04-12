using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class SupervisorStudentSchedulesReport(AssignmentDataSet reportData, List<Supervisor> supervisors) : IHtmlGenerator
{
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Supervisors Students Schedules");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlTableGrid.Css());

        doc.Add(new HtmlHeader("Supervisors' Students Schedules"));

        var matches = GetSupervisorStudentMatches()
            .OrderBy(x => x.Supervisor.LastName)
            .ThenBy(x => x.Supervisor.FirstName)
            .ToList();

        var grid = doc.Add(new HtmlTableGrid());
        const int numColumns = 2;
        var rowData = new List<XElement>();
        for (var i = 0; i < matches.Count; i++) {
            var match = matches[i];
            var student = reportData.InputData.StudentInfo[match.Student.StudentId];
            var table = GetStudentTable(match.Student.StudentId);
            table.AddColGroup(1, ("style", "width: 20%"));
            var cell = new XElement("div",
                new HtmlHeader($"{student.LastName}, {student.FirstName} (HR: {student.Homeroom})", 3),
                new HtmlParagraph($"Matched supervisor: {match.Supervisor.LastName}, {match.Supervisor.FirstName}"),
                table);

            rowData.Add(cell);
            if (rowData.Count == numColumns || i == matches.Count - 1) {
                grid.AddRow(rowData.ToList());
                rowData.Clear();
            }
        }

        return doc;
    }

    private List<(Supervisor Supervisor, StudentInfo Student)> GetSupervisorStudentMatches()
    {
        var matches = new List<(Supervisor Supervisor, StudentInfo Student)>();
        var students = reportData.InputData.StudentInfo.Values.ToLookup(x => x.LastName.ToUpper());
        foreach (var supervisor in supervisors) {
            var matchedStudents = students[supervisor.LastName.ToUpper()].ToList();
            matchedStudents.AddRange(students[supervisor.AlternateLastName.ToUpper()]);
            foreach (var student in matchedStudents)
                matches.Add((supervisor, student));
        }

        return matches;
    }
    
    
    private HtmlTable GetStudentTable(int studentId)
    {
        var assigned = reportData.GetStudentsAssignments(studentId)
            .OrderBy(x => x.SessionId)
            .ToList();

        var table = new HtmlTable(["Session", "Class", "Room"]);
        foreach (var course in assigned) {
            var courseInfo = reportData.InputData.CourseInfo[course.CourseCode];
            var courseName = courseInfo.CourseName;
            var room = courseInfo.RoomCode ?? "TBD";
            table.AddRowData([course.SessionId, courseName, room]);
        }

        return table;
    } 
}