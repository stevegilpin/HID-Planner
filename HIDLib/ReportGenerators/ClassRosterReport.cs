using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;

namespace HIDLib.ReportGenerators;

public class ClassRosterReport
{
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Class Roster");
        doc.StyleRuleSets.Add(HtmlUtil.CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlUtil.CSS.PageBreak());
        doc.StyleRuleSets.Add(HtmlUtil.CSS.TableStyles());

        var courseAssignments = outputData.AssignmentResults
            .GroupBy(x => x.CourseCode)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var courseGroup in courseAssignments)
            doc.Add(CreateSingleClassRoster(courseGroup.Key, courseGroup.ToList(), outputData));

        return doc;
    }

    private static XElement CreateSingleClassRoster(string courseCode, List<CourseAssignmentResult> selectedStudents, SchedulingOutputData outputData)
    {
        var courseData = outputData.CourseInputs[courseCode];
        var sessionGroups = selectedStudents
            .GroupBy(x => x.SessionId)
            .OrderBy(g => g.Key)
            .ToList();

        var root = new XElement("div");
        root.Add(HtmlUtil.Header($"Class Roster: {courseData.CourseInfo.CourseName}", 2));

        foreach (var group in sessionGroups) {
            root.Add(HtmlUtil.Header($"Session {group.Key}", 3));
            var table = new HtmlTable([1]);
            table.AddColumnHeaders(["Student Name", "Grade"]);
            var students = group.OrderBy(x => x.StudentId).ToList();
            foreach (var student in students) {
                var studentInfo = outputData.StudentInputs[student.StudentId].StudentInfo;
                table.AddRow([$"{studentInfo.FirstName} {studentInfo.LastName}", studentInfo.Grade]);
            }
            root.Add(table.ToXmlNode());
        }

        root.Add(HtmlUtil.Break());
        root.Add(HtmlUtil.PageBreak());

        return root;
    }
}