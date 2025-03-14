using System;
using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class SupervisorStudentSchedulesReport(AssignmentDataSet reportData, List<SupervisorInfo> supervisors) : IHtmlGenerator
{
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Supervisors Students Schedules");
        doc.StyleRuleSets.Add(CSS.TableStyles());

        doc.Add(new HtmlHeader("Supervisors' Students Schedules"));

        var matches = GetSupervisorStudentMatches()
            .OrderBy(x => x.Supervisor.LastName)
            .ThenBy(x => x.Supervisor.FirstName)
            .ToList();

        var assignmentMap = reportData.AssignmentResults
            .GroupBy(x => x.StudentId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.SessionId));

        var sessions = reportData.AllSessions();
        var tableHeader = new List<object> { "Supervisor", "Student" };
        tableHeader.AddRange(sessions.Select(x => $"Session {x}"));
        var table = doc.Add(new HtmlTable(tableHeader));

        foreach (var match in matches) {
            var rowData = new List<object> {
                $"{match.Supervisor.FirstName} {match.Supervisor.LastName}",
                $"{match.Student.FirstName} {match.Student.LastName}",
            };

            var studentCourses = assignmentMap[match.Student.StudentId];
            foreach (var session in sessions) {
                rowData.Add(studentCourses.GetValueOrDefault(session)?.CourseCode ?? string.Empty);
            }
            table.AddRowData(rowData);
        }

        return doc;
    }

    private List<(SupervisorInfo Supervisor, StudentInfo Student)> GetSupervisorStudentMatches()
    {
        var matches = new List<(SupervisorInfo Supervisor, StudentInfo Student)>();
        var students = reportData.InputData.StudentInfo.Values.ToLookup(x => x.LastName.ToUpper());
        foreach (var supervisor in supervisors) {
            var matchedStudents = students[supervisor.LastName.ToUpper()].ToList();
            matchedStudents.AddRange(students[supervisor.AlternateLastName.ToUpper()]);
            foreach (var student in matchedStudents)
                matches.Add((supervisor, student));
        }

        return matches;
    }
}