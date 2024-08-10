using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;

namespace HIDLib.ReportGenerators;

public static class AssignmentAnalysisReport
{
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Assignment Analysis");
        doc.StyleRuleSets.Add(HtmlUtil.CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlUtil.CSS.TableStyles());

        var courses = outputData.AssignmentResults
            .Select(x => x.CourseCode)
            .Distinct()
            .ToList();

        var courseData = courses
            .Select(x => outputData.CourseInputs[x])
            .OrderBy(x => x.CourseInfo.CourseName)
            .ToList();

        // todo change width of some columns. I think i can use something like a colspan child element in the table?
        var columnHeaders = new List<object> { "Class", "Code", "Session", "Capacity", "# Assigned", "boys", "girls" };
        var grades = DistinctGrades(outputData);
        columnHeaders.AddRange(grades);

        var table = new HtmlTable();
        table.AddColumnHeaders(columnHeaders);
        table.AddColGroup(2, ("style", "width: 10%;"));
        table.AddColGroup(1, ("style", "width: 10%; border-right: 1px solid black;"));
        table.AddColGroup(1, ("style", "width: 10%;"));
        table.AddColGroup(1, ("style", "width: 10%; border-right: 1px solid black;"));
        table.AddColGroup(1, ("style", "width: 5%;"));
        table.AddColGroup(1, ("style", "width: 5%; border-right: 1px solid black;"));
        table.AddColGroup(grades.Count, ("style", "width: 5%;"));

        foreach (var course in courseData) {
            foreach (var session in course.Schedule) {
                table.AddRow(GetRowData(session, outputData));
            }
        }

        doc.Add(table.ToXmlNode());

        return doc;
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

        static void HighlightCell(XElement cell) => cell.Add(new XAttribute("style", "background-color: yellow;"));
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