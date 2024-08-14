using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class ClassRosterReport
{
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Class Roster");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add(CSS.TableStyles());

        var courseAssignments = outputData.AssignmentResults
            .GroupBy(x => x.CourseCode)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var courseGroup in courseAssignments)
            doc.Add(CreateSingleClassRoster(courseGroup.Key, courseGroup.ToList(), outputData));

        return doc;
    }

    private static XElement CreateSingleClassRoster(string courseCode, List<CourseAssignmentResult> selectedStudents, SchedulingOutputData outputData)
    {
        var courseData = outputData.CourseInputs[courseCode];
        var sessionGroups = selectedStudents
            .GroupBy(x => x.SessionId)
            .OrderBy(g => g.Key)
            .ToList();

        var root = new XElement("div");
        root.Add(new HtmlHeader($"Class Roster: {courseData.CourseInfo.CourseName}", 2));

        var grid = new HtmlGridLayout(2);
        root.Add(grid);

        foreach (var group in sessionGroups) {
            var header = new HtmlHeader($"Session {group.Key}", 3);
            var table = new HtmlTable();
            table.AddColumnHeaders(["Student Name", "Grade"]);
            table.CaptionBelowTable = $"{group.Count()} students";
            var students = group.OrderBy(x => x.StudentId).ToList();
            foreach (var student in students) {
                var studentInfo = outputData.StudentInputs[student.StudentId].StudentInfo;
                table.AddRowData([$"{studentInfo.FirstName} {studentInfo.LastName}", studentInfo.Grade]);
            }
            
            foreach (var row in table.Data)
                row.Data[1].Add(new XAttribute("style", "text-align: center;"));

            grid.AddCell(header, table.ToXmlNode());
        }

        root.Add(new HtmlBreak());
        root.Add(new HtmlPageBreak());

        return root;
    }
}