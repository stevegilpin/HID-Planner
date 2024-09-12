using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
public class StudentScheduleGridReport(SchedulingOutputData reportData) : IHtmlGenerator
{
    private readonly SchedulingOutputData ReportData = reportData;
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Student Schedule Grid");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add(CSS.TableStyles());

        var studentIds =  ReportData.StudentInputs.Keys.OrderBy(x => x).ToList();
        foreach (var id in studentIds) {
            doc.Add(GetStudentSchedule(id));
            doc.Add(new HtmlPageBreak());
        }

        return doc;
    }

    private XElement GetStudentSchedule(int studentId)
    {
        var root = new XElement("div");
        var studentData =  ReportData.StudentInputs[studentId];

        // page header
        var name = $"{studentData.StudentInfo.FirstName} {studentData.StudentInfo.LastName}";
        root.Add(new HtmlHeader(name, 2));
        root.Add(new XElement("span", $"Grade: {studentData.StudentInfo.Grade}"));
        root.Add(new HtmlBreak());
        root.Add(new XElement("span", $"Room: {studentData.StudentInfo.Homeroom}"));
        root.Add(new HtmlBreak());
        root.Add(new XElement("span", $"ID: {studentData.StudentInfo.StudentId}"));
        root.Add(new HtmlBreak());
        
        root.Add(new HtmlHeader("Choices by Session", 3));
        var courseOptions = GetRankedCourseOfferingTable(studentId);
        root.Add(courseOptions.ToXmlNode());

        root.Add(new HtmlHeader("Assigned Classes", 3));
        var assigned = GetAssignedClassesTable(studentId);
        root.Add(assigned.ToXmlNode());

        return root;
    }

    private HtmlTable GetRankedCourseOfferingTable(int studentId)
    {
        const int numChoicesShown = 12;
        var rankings =  ReportData.StudentInputs[studentId].CourseRankings
            .Where(x => x.Rank <= numChoicesShown)
            .OrderBy(x => x.Rank).ToList();

        var availableCourses = new Dictionary<string, HashSet<int>>();
        foreach (var course in rankings) {
            var sessionIds = ReportData.CourseInputs[course.CourseCode].Schedule.Select(x => x.SessionId).ToHashSet();
            availableCourses.Add(course.CourseCode, sessionIds);
        }

        var table = new HtmlTable();
        table.CaptionBelowTable = $"Top {numChoicesShown} choices shown";
        var colHeaders = new List<object> { "Session" };
        colHeaders.AddRange(rankings.Select(x => x.Rank.ToString()));
        table.AddColumnHeaders(colHeaders);
        table.AddColGroup(colHeaders.Count - 1, ("style", "border-right: 1px solid black;"));
        
        var assignedCourses = ReportData.AssignmentResults
            .Where(x => x.StudentId == studentId)
            .Select(x => (x.CourseCode, x.SessionId))
            .ToHashSet();

        var sessions = assignedCourses.Select(x => x.SessionId).OrderBy(x => x).ToList();
        foreach (var s in sessions) {
            var rowData = new List<XElement> { new("th", s) };
            foreach (var course in rankings) {
                var cellData = new XElement("td", availableCourses[course.CourseCode].Contains(s) ? course.CourseCode : string.Empty);
                rowData.Add(cellData);
                if (assignedCourses.Contains((course.CourseCode, s)))
                    cellData.Add(new XAttribute("style", "color: red; font-weight: bold;"));
            }

            table.AddRow(rowData);
        }

        return table;
    }

    private HtmlTable GetAssignedClassesTable(int studentId)
    {
        var assigned = ReportData.AssignmentResults
            .Where(x => x.StudentId == studentId)
            .OrderBy(x => x.SessionId)
            .ToList();

        var studentData = ReportData.StudentInputs[studentId];
        var courseRankings = studentData.CourseRankings.ToDictionary(x => x.CourseCode);
        var objectiveScore = assigned.Sum(x => SchedulingSolver.GetCourseCost(courseRankings[x.CourseCode].Rank));

        var table = new HtmlTable();
        table.CaptionBelowTable = $"Objective score: {objectiveScore:N0}";
        table.AddColumnHeaders(["Session", "Course", "Course Ranking"]);
        for (int i = 0; i < assigned.Count; i++) {
            var course = assigned[i];
            var ranking = courseRankings[course.CourseCode];
            var row = table.AddRowData([i + 1, course.CourseCode, ranking.Rank]);
            if (ranking.Rank >= 7 && ranking.Rank < 10)
                row.RowContainer.AddStyle("background-color: yellow");
            else if (ranking.Rank >= 10)
                row.RowContainer.AddStyle("background-color: red");
        }

        return table;
    }
}