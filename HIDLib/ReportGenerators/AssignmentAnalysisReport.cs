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
        table.AddColGroup(2);
        table.AddColGroup(1, ("style", "border-right: 1px solid black;"));
        table.AddColGroup(1);
        table.AddColGroup(1, ("style", "border-right: 1px solid black;"));
        table.AddColGroup(1, ("style", "width: 5%;"));
        table.AddColGroup(1, ("style", "width: 5%; border-right: 1px solid black;"));
        table.AddColGroup(grades.Count, ("style", "width: 5%;"));

        foreach (var course in courseData) {
            foreach (var session in course.Schedule) {
                table.AddRowData(GetRowData(session, outputData));
            }
        }

        doc.Add(table.ToXmlNode());

        return doc;
    }

    private static List<object> GetRowData(CourseSchedule session, SchedulingOutputData outputData)
    {
        var assigned = outputData.AssignmentResults
            .Where(x => x.CourseCode == session.CourseCode)
            .Where(x => x.SessionId == session.SessionId)
            .ToList();

        var numAssigned = assigned.Count;
        var courseInfo = outputData.CourseInputs[session.CourseCode].CourseInfo;
        var capacity = courseInfo.MaxCapacity;

        var rowData = new List<object> {
            courseInfo.CourseName,
            courseInfo.CourseCode,
            session.SessionId,
            capacity,
            numAssigned,
            
            // todo gender data
            string.Empty,
            string.Empty
        };

        var grades = DistinctGrades(outputData);
        var gradeData = new List<int>();
        foreach (var grade in grades)
            gradeData.Add(assigned.Count(s => outputData.StudentInputs[s.StudentId].StudentInfo.Grade == grade));

        rowData.AddRange(gradeData.Select(x => x == 0 ? string.Empty : x.ToString()));

        return rowData;
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