using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;
using HIDLib.Scheduling;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
public class StudentScheduleGridReport(AssignmentDataSet reportData) : IHtmlGenerator
{
    private readonly AssignmentDataSet ReportData = reportData;
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Student Schedule Grid");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add(CSS.TableStyles());

        var studentIds =  ReportData.InputData.StudentInfo.Keys.OrderBy(x => x).ToList();
        foreach (var id in studentIds) {
            doc.Add(GetStudentSchedule(id));
            doc.Add(new HtmlPageBreak());
        }

        return doc;
    }

    private XElement GetStudentSchedule(int studentId)
    {
        var root = new XElement("div");
        var studentData =  ReportData.InputData.StudentInfo[studentId];

        // page header
        var name = $"{studentData.FirstName} {studentData.LastName}";
        root.Add(new HtmlHeader(name, 2));
        root.Add(new XElement("span", $"Grade: {studentData.Grade}"));
        root.Add(new HtmlBreak());
        root.Add(new XElement("span", $"Room: {studentData.Homeroom}"));
        root.Add(new HtmlBreak());
        root.Add(new XElement("span", $"ID: {studentData.StudentId}"));
        root.Add(new HtmlBreak());
        
        root.Add(new HtmlHeader("Choices by Session", 3));
        root.Add(GetRankedCourseOfferingTable(studentId));

        root.Add(new HtmlHeader("Assigned Classes", 3));
        root.Add(GetAssignedClassesTable(studentId));

        return root;
    }

    private HtmlTable GetRankedCourseOfferingTable(int studentId)
    {
        const int numChoicesShown = 12;
        var rankings =  ReportData.InputData.StudentCourseRanking[studentId]
            .Where(x => x.Rank <= numChoicesShown)
            .OrderBy(x => x.Rank).ToList();

        var availableCourses = new Dictionary<string, HashSet<int>>();
        foreach (var course in rankings) {
            var sessionIds = ReportData.InputData.CourseSchedule[course.CourseCode].Select(x => x.SessionId).ToHashSet();
            availableCourses.Add(course.CourseCode, sessionIds);
        }

        var colHeaders = new List<object> { "Session" };
        colHeaders.AddRange(rankings.Select(x => x.Rank.ToString()));
        var table = new HtmlTable(colHeaders);
        table.AddCaption($"Top {numChoicesShown} choices shown", HtmlCaption.TextAlign.Right);
        table.AddColGroup(colHeaders.Count - 1, ("style", "border-right: 1px solid black;"));

        var assignedCourses = ReportData.GetStudentsAssignments(studentId)
            .Select(x => (x.CourseCode, x.SessionId))
            .ToHashSet();

        var sessions = assignedCourses.Select(x => x.SessionId).OrderBy(x => x).ToList();
        foreach (var s in sessions) {
            var rowData = new List<HtmlCell> { new(s, true) };
            foreach (var course in rankings) {
                var cellData = new HtmlCell(availableCourses[course.CourseCode].Contains(s) ? course.CourseCode : string.Empty);
                rowData.Add(cellData);
                if (assignedCourses.Contains((course.CourseCode, s)))
                    cellData.AddStyle("color: red; font-weight: bold;");
            }

            table.AddRow(rowData);
        }

        return table;
    }

    private HtmlTable GetAssignedClassesTable(int studentId)
    {
        var assigned = ReportData.GetStudentsAssignments(studentId)
            .OrderBy(x => x.SessionId)
            .ToList();

        double objectiveScore = 0;
        foreach (var course in assigned) {
            var rank = ReportData.InputData.GetCourseRank(course.StudentId, course.CourseCode);
            objectiveScore += SchedulingSolver.GetCourseCost(rank);
        }

        var table = new HtmlTable(["Session", "Course", "Course Ranking"]);
        table.AddCaption($"Objective score: {objectiveScore:N0}", HtmlCaption.TextAlign.Right);
        for (int i = 0; i < assigned.Count; i++) {
            var course = assigned[i];
            var rank = ReportData.InputData.GetCourseRank(course.StudentId, course.CourseCode);
            var row = table.AddRowData([course.SessionId, course.CourseCode, rank]);
            if (rank >= 7 && rank < 10)
                row.AddStyle("background-color: yellow");
            else if (rank >= 10)
                row.AddStyle("background-color: red");
        }

        return table;
    }
}