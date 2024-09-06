using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public static class AssignmentAnalysisReport
{
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Assignment Analysis");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.TableStyles("center"));
        doc.StyleRuleSets.Add(CSS.BodyBackgroundColorInPrint());
        
        doc.Add(new HtmlHeader("Analysis of Class Assignments"));

        var courses = outputData.AssignmentResults
            .Select(x => x.CourseCode)
            .Distinct()
            .ToList();

        var courseData = courses
            .Select(x => outputData.CourseInputs[x])
            .OrderBy(x => x.CourseInfo.CourseCode)
            .ToList();

        var table = new HtmlTable();
        var grades = DistinctGrades(outputData);
        AddColumnFormatting(table, grades);
        
        foreach (var course in courseData) {
            foreach (var session in course.Schedule) {
                table.AddRow(GetRowData(session, outputData));
            }
        }

        doc.Add(table.ToXmlNode());

        return doc;
    }

    private static void AddColumnFormatting(HtmlTable table, List<string> grades)
    {
        var columnHeaders = new List<object> { "Class", "Code", "Session", "Capacity", "# Assigned", "boys", "girls" };
        columnHeaders.AddRange(grades);

        var headers = table.AddColumnHeaders(columnHeaders);
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

    private static List<XElement> GetRowData(CourseSchedule session, SchedulingOutputData outputData)
    {
        var assigned = outputData.AssignmentResults
            .Where(x => x.CourseCode == session.CourseCode)
            .Where(x => x.SessionId == session.SessionId)
            .ToList();

        var courseInfo = outputData.CourseInputs[session.CourseCode].CourseInfo;
        var capacity = courseInfo.MaxCapacity;

        var rowData = new List<XElement>();
        AddCell(rowData, courseInfo.CourseName);
        AddCell(rowData, courseInfo.CourseCode);
        rowData.ForEach(x => x.AddStyle("text-align: left"));

        AddCell(rowData, session.SessionId);
        AddCell(rowData, capacity);

        var numAssignedCell = AddCell(rowData, assigned.Count);
        if (assigned.Count <= 5)
            HighlightCell(numAssignedCell);

        var boys = assigned.Count(x => outputData.StudentInputs[x.StudentId].StudentInfo.Gender == "M");
        var boysCell = AddCell(rowData, boys == 0 ? string.Empty : boys);
        if (boys == 1)
            HighlightCell(boysCell);
        
        var girls = assigned.Count(x => outputData.StudentInputs[x.StudentId].StudentInfo.Gender == "F");
        var girlsCell = AddCell(rowData, girls == 0 ? string.Empty : girls);
        if (girls == 1)
            HighlightCell(girlsCell);

        var grades = DistinctGrades(outputData);
        foreach (var grade in grades) {
            var gradeCount = assigned.Count(s => outputData.StudentInputs[s.StudentId].StudentInfo.Grade == grade);
            AddCell(rowData, gradeCount == 0 ? string.Empty : gradeCount);
        }

        return rowData;

        static XElement AddCell(List<XElement> data, object content)
        {
            var cell = new XElement("td", content);
            data.Add(cell);
            return cell;
        }

        static void HighlightCell(XElement cell) => cell.AddStyle("background-color: yellow");
    }

    private static List<string> DistinctGrades(SchedulingOutputData outputData)
    {
        return outputData.StudentInputs.Values
            .Select(x => x.StudentInfo.Grade)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }
}