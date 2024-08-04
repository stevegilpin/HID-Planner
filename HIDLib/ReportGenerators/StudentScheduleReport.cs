using System;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;

namespace HIDLib.ReportGenerators;

public class StudentScheduleReport
{
    public static HtmlDoc Create(SchedulingOutputData outputData)
    {
        var doc = new HtmlDoc("Student Schedules");
        doc.StyleRuleSets.Add(HtmlUtil.CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlUtil.CSS.PageBreak());
        doc.StyleRuleSets.Add(HtmlUtil.CSS.TableStyles());

        var studentIds = outputData.StudentInputs.Keys.OrderBy(x => x).ToList();
        foreach (var id in studentIds) {
            doc.Add(GetStudentSchedule(id, outputData));
            doc.Add(HtmlUtil.PageBreak());
        }

        return doc;
    }

    private static XElement GetStudentSchedule(int studentId, SchedulingOutputData outputData)
    {
        var root = new XElement("div");
        var studentData = outputData.StudentInputs[studentId];

        // page header
        var name = $"{studentData.StudentInfo.FirstName} {studentData.StudentInfo.LastName}";
        var grade = $"Grade {studentData.StudentInfo.Grade}";
        var homeroom = $"Room {studentData.StudentInfo.Homeroom}";
        root.Add(HtmlUtil.Header($"{name} - {grade} - {homeroom}", 2));
        
        root.Add(HtmlUtil.Header("Choices by Session", 3));
        var courseOptions = GetRankedCourseOfferingTable(studentId, outputData);
        root.Add(courseOptions.ToXmlNode());

        root.Add(HtmlUtil.Header("Assigned Classes", 3));
        var assigned = GetAssignedClassesTable(studentId, outputData);
        root.Add(assigned.ToXmlNode());

        return root;
    }

    private static HtmlTable GetRankedCourseOfferingTable(int studentId, SchedulingOutputData outputData)
    {
        throw new NotImplementedException();
    }

    private static HtmlTable GetAssignedClassesTable(int studentId, SchedulingOutputData outputData)
    {
        var assigned = outputData.AssignmentResults
            .Where(x => x.StudentId == studentId)
            .OrderBy(x => x.SessionId)
            .ToList();

        var studentData = outputData.StudentInputs[studentId];
        var courseRankings = studentData.CourseRankings.ToDictionary(x => x.CourseCode);

        var table = new HtmlTable();
        table.AddColumnHeaders(["Session", "Course", "Course Ranking"]);
        for (int i = 0; i < assigned.Count; i++) {
            var course = assigned[i];
            var ranking = courseRankings[course.CourseCode];
            table.AddRow([i + 1, course.CourseCode, ranking.Rank]);
        }

        return table;
    }
}