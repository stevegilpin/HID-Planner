using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
public class ProblematicAssignmentsReport(
    AssignmentDataSet reportData,
    int? rankingThreshold = null)
    : IHtmlGenerator
{
    private int WorstCourseRankThreshold = rankingThreshold ?? SettingsManager.GetBadCourseRankCutoff();
    private readonly AssignmentDataSet ReportData = reportData;

    public HtmlDoc CreateDoc()
    {
        var doc = new HtmlDoc("Problematic Assignments");
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(CSS.PageBreak());
        doc.StyleRuleSets.Add(CSS.TableStyles());

        doc.Add(new HtmlHeader("Problematic Student Assignments"));

        var dataSet = FindProblematicAssignments(ReportData, WorstCourseRankThreshold);
        if (dataSet.Count == 0) {
            doc.Add(new HtmlParagraph($"No course assignments at {WorstCourseRankThreshold} or worse"));
            return doc;
        }

        doc.Add(new HtmlParagraph($"Assigned courses ranked at or below {WorstCourseRankThreshold}"));
        var table = new HtmlTable(["Last Name", "First Name", "Course Rank", "Course Code"]);
        foreach (var item in dataSet)
            table.AddRowData([item.LastName, item.FirstName, item.Rank, item.CourseCode]);

        doc.Add(table);

        return doc;
    }

    public static List<BadAssignment> FindProblematicAssignments(AssignmentDataSet reportData, int? courseRankCutoff = null)
    {
        var cutoff = courseRankCutoff ?? SettingsManager.GetBadCourseRankCutoff();
        var rankings = reportData.InputData.StudentCourseRanking.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.CourseCode, y => y.Rank));
        var assignments = new List<BadAssignment>();
        foreach (var course in reportData.AssignmentResults) {
            var rank = rankings[course.StudentId][course.CourseCode];
            if (rank < cutoff)
                continue;

            var student = reportData.InputData.StudentInfo[course.StudentId];
            assignments.Add(new BadAssignment(student.LastName, student.FirstName, rank, course.CourseCode));
        }

        return assignments
            .OrderByDescending(x => x.Rank)
            .ThenBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToList();
    }

    public record BadAssignment(string LastName, string FirstName, int Rank, string CourseCode);
}