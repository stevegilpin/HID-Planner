using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
internal class AssignmentAnalysisReport(AssignmentDataSet reportData) : IHtmlGenerator
{
    private readonly AssignmentDataSet ReportData = reportData;

    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Assignment Analysis");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.TableStyles("center"));
        doc.StyleRuleSets.Add(CSS.BodyBackgroundColorInPrint());
        
        doc.Add(new HtmlHeader("Analysis of Class Assignments"));

        var courses = ReportData.AssignmentResults
            .Select(x => x.CourseCode)
            .Distinct()
            .ToList();

        var courseData = courses
            .Select(x => ReportData.InputData.CourseInfo[x])
            .OrderBy(x => x.CourseCode)
            .ToList();

        var table = doc.Add(new HtmlTable());
        var grades = DistinctGrades();
        AddColumnFormatting(table, grades);
        
        foreach (var course in courseData) {
            var schedule = ReportData.InputData.CourseSchedule[course.CourseCode];
            foreach (var session in schedule)
                table.AddRow(GetRowData(session));
        }


        return doc;
    }

    private static void AddColumnFormatting(HtmlTable table, List<string> grades)
    {
        var columnHeaders = new List<object> { "Class", "Code", "Session", "Capacity", "# Assigned", "boys", "girls" };
        columnHeaders.AddRange(grades);

        var headers = table.AddHeaderRow(columnHeaders);
        headers.Data[0].AddStyle("text-align: left");
        headers.Data[1].AddStyle("text-align: left");

        table.AddColGroup(2, ("style", "width: 10%;"));
        table.AddColGroup(1, ("style", "width: 10%; border-right: 1px solid black;"));
        table.AddColGroup(1, ("style", "width: 10%;"));
        table.AddColGroup(1, ("style", "width: 10%; border-right: 1px solid black;"));
        table.AddColGroup(1, ("style", "width: 5%;"));
        table.AddColGroup(1, ("style", "width: 5%; border-right: 1px solid black;"));

        // special formatting to separate middle school
        if (grades.Last() == "8") {
            // 1 and 2
            table.AddColGroup(1, ("style", "width: 5%;"));
            table.AddColGroup(1, ("style", "width: 5%; border-right: 1px solid black;"));
            
            // 3, 4, 5
            table.AddColGroup(2, ("style", "width: 5%;"));
            table.AddColGroup(1, ("style", "width: 5%; border-right: 1px solid black;"));

            // 6, 7, 8
            table.AddColGroup(3, ("style", "width: 5%;"));
        } else {
            for (int i = 0; i < grades.Count; i += 2) {
                table.AddColGroup(1, ("style", "width: 5%;"));
                if (i >= grades.Count - 2)
                    table.AddColGroup(1, ("style", "width: 5%;"));
                else
                    table.AddColGroup(1, ("style", "width: 5%; border-right: 1px solid black;"));
            }
        }

    }

    private List<HtmlCell> GetRowData(CourseSchedule session)
    {
        var assigned = ReportData.AssignmentResults
            .Where(x => x.CourseCode == session.CourseCode)
            .Where(x => x.SessionId == session.SessionId)
            .ToList();

        var courseInfo = ReportData.InputData.CourseInfo[session.CourseCode];
        var capacity = courseInfo.MaxCapacity;

        var rowData = new List<HtmlCell>();
        AddCell(rowData, courseInfo.CourseName);
        AddCell(rowData, courseInfo.CourseCode);
        rowData.ForEach(x => x.AddStyle("text-align: left"));

        AddCell(rowData, session.SessionId);
        AddCell(rowData, capacity);

        var numAssignedCell = AddCell(rowData, assigned.Count);
        if (assigned.Count <= 5)
            HighlightCell(numAssignedCell);

        var boys = assigned.Count(x => ReportData.InputData.StudentInfo[x.StudentId].Gender == "M");
        var boysCell = AddCell(rowData, boys == 0 ? string.Empty : boys);
        if (boys == 1)
            HighlightCell(boysCell);
        
        var girls = assigned.Count(x => ReportData.InputData.StudentInfo[x.StudentId].Gender == "F");
        var girlsCell = AddCell(rowData, girls == 0 ? string.Empty : girls);
        if (girls == 1)
            HighlightCell(girlsCell);

        var grades = DistinctGrades();
        foreach (var grade in grades) {
            var gradeCount = assigned.Count(s => ReportData.InputData.StudentInfo[s.StudentId].Grade == grade);
            AddCell(rowData, gradeCount == 0 ? string.Empty : gradeCount);
        }

        return rowData;

        static HtmlCell AddCell(List<HtmlCell> data, object content)
        {
            var cell = new HtmlCell(content);
            data.Add(cell);
            return cell;
        }

        static void HighlightCell(XElement cell) => cell.AddStyle("background-color: yellow");
    }

    private List<string> DistinctGrades()
    {
        return ReportData.InputData.StudentInfo.Values
            .Select(x => x.Grade)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }
}