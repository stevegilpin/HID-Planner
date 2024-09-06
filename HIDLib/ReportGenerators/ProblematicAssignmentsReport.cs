using System;
using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class ProblematicAssignmentsReport
{
    private const int COURSE_RANK_THRESHOLD = 7;
    
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Problematic Assignments");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add(CSS.TableStyles());

        doc.Add(new HtmlHeader("Problematic Student Assignments"));

        var dataSet = FindProblematicAssignments(outputData)
            .OrderByDescending(x => x.Rank)
            .ThenBy(x => x.Student.LastName)
            .ThenBy(x => x.Student.FirstName)
            .ToList();

        if (dataSet.Count == 0) {
            doc.Add(new HtmlParagraph($"No course assignments at {COURSE_RANK_THRESHOLD} or worse"));
            return doc;
        }

        doc.Add(new HtmlParagraph($"Assigned courses ranked at or below {COURSE_RANK_THRESHOLD}"));
        var table = new HtmlTable();
        // table.CaptionBelowTable = $"Courses ranked at or below {COURSE_RANK_THRESHOLD}";
        table.AddColumnHeaders(["Last Name", "First Name", "Course Rank", "Course Code"]);
        foreach (var item in dataSet)
            table.AddRowData([item.Student.LastName, item.Student.FirstName, item.Rank, item.CourseCode]);

        doc.Add(table.ToXmlNode());

        return doc;
    }

    private static List<(StudentInfo Student, string CourseCode, int Rank)> FindProblematicAssignments(SchedulingOutputData outputData)
    {
        var rankings = outputData.StudentInputs.ToDictionary(x => x.Key, x => x.Value.CourseRankings.ToDictionary(y => y.CourseCode, y => y.Rank));
        var assignments = new List<(StudentInfo, string, int)>();
        foreach (var course in outputData.AssignmentResults) {
            var rank = rankings[course.StudentId][course.CourseCode];
            if (rank < COURSE_RANK_THRESHOLD)
                continue;

            var student = outputData.StudentInputs[course.StudentId].StudentInfo;
            assignments.Add((student, course.CourseCode, rank));
        }

        return assignments;
    }
}