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
        doc.StyleRuleSets.Add(HtmlUtil.CSS.PageBreak());

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
            var list = HtmlUtil.List();
            root.Add(list);
            foreach (var assignment in group.OrderBy(x => x.StudentId)) {
                var student = outputData.StudentInputs[assignment.StudentId].StudentInfo;
                var studentName = $"{student.FirstName} {student.LastName}, Grade {student.Grade}";
                list.Add(HtmlUtil.ListItem(studentName));
            }
        }

        root.Add(HtmlUtil.Break());
        root.Add(HtmlUtil.PageBreak());

        return root;
    }
}