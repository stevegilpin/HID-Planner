using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
internal class ClassRosterReport(AssignmentDataSet reportData) : IHtmlGenerator
{
    private readonly AssignmentDataSet ReportData = reportData;

    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Class Roster");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add(CSS.TableStyles());

        var courseAssignments = ReportData.AssignmentResults
            .GroupBy(x => new {
                ReportData.InputData.CourseInfo[x.CourseCode].CourseName,
                x.CourseCode,
            })
            .OrderBy(g => g.Key.CourseName)
            .ToList();

        foreach (var courseGroup in courseAssignments)
            doc.Add(CreateSingleClassRoster(courseGroup.Key.CourseName, courseGroup.Key.CourseCode, courseGroup.ToList()));

        return doc;
    }

    private XElement CreateSingleClassRoster(string courseName, string courseCode, List<CourseAssignmentResult> selectedStudents)
    {
        var sessionGroups = selectedStudents
            .GroupBy(x => x.SessionId)
            .OrderBy(g => g.Key)
            .ToList();

        var root = new XElement("div");
        root.Add(new HtmlHeader($"Class Roster: {courseName}", 2));
        root.Add(new HtmlParagraph($"Class code: {courseCode}"));

        var grid = new HtmlGridLayout(2);
        root.Add(grid);

        foreach (var group in sessionGroups) {
            var header = new HtmlHeader($"Session {group.Key}", 3);
            var table = new HtmlTable();
            table.AddColumnHeaders(["Student Name", "Grade", "Gender"]);
            table.CaptionBelowTable = $"{group.Count()} students";
            var students = group.OrderBy(x => x.StudentId).ToList();
            foreach (var student in students) {
                var studentInfo = ReportData.InputData.StudentInfo[student.StudentId];
                table.AddRowData([$"{studentInfo.FirstName} {studentInfo.LastName}", studentInfo.Grade, studentInfo.Gender]);
            }
            
            foreach (var row in table.Data) {
                row.Data[1].AddStyle("text-align: center;");
                row.Data[2].AddStyle("text-align: center;");
            }

            grid.AddCell(header, table.ToXmlNode());
        }

        root.Add(new HtmlBreak());
        root.Add(new HtmlPageBreak());

        return root;
    }
}