using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
public class ProblematicAssignmentsReport(AssignmentDataSet reportData, int rankingThreshold = 7) : IHtmlGenerator
{
    private int WorstCourseRankThreshold = rankingThreshold;
    private readonly AssignmentDataSet ReportData = reportData;
    
    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Problematic Assignments");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add(CSS.TableStyles());

        doc.Add(new HtmlHeader("Problematic Student Assignments"));

        var dataSet = FindProblematicAssignments()
            .OrderByDescending(x => x.Rank)
            .ThenBy(x => x.Student.LastName)
            .ThenBy(x => x.Student.FirstName)
            .ToList();

        if (dataSet.Count == 0) {
            doc.Add(new HtmlParagraph($"No course assignments at {WorstCourseRankThreshold} or worse"));
            return doc;
        }

        doc.Add(new HtmlParagraph($"Assigned courses ranked at or below {WorstCourseRankThreshold}"));
        var table = new HtmlTable(["Last Name", "First Name", "Course Rank", "Course Code"]);
        foreach (var item in dataSet)
            table.AddRowData([item.Student.LastName, item.Student.FirstName, item.Rank, item.CourseCode]);

        doc.Add(table);

        return doc;
    }

    private List<(StudentInfo Student, string CourseCode, int Rank)> FindProblematicAssignments()
    {
        var rankings = ReportData.InputData.StudentCourseRanking.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.CourseCode, y => y.Rank));
        var assignments = new List<(StudentInfo, string, int)>();
        foreach (var course in ReportData.AssignmentResults) {
            var rank = rankings[course.StudentId][course.CourseCode];
            if (rank < WorstCourseRankThreshold)
                continue;

            var student = ReportData.InputData.StudentInfo[course.StudentId];
            assignments.Add((student, course.CourseCode, rank));
        }

        return assignments;
    }
}