using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class ClassSupervisorsReport(AssignmentDataSet reportData, List<Supervisor> supervisors) : IHtmlGenerator
{
    private readonly Dictionary<string, Supervisor> SupervisorMap = supervisors.ToDictionary(x => x.SupervisorId);
    public HtmlDoc CreateDoc()
    {
        var title = "Class Supervisors Report"; 
        var doc = new HtmlDoc(title);
        doc.StyleRuleSets.Add(CSS.BodyPadding());
        doc.StyleRuleSets.Add(HtmlTableGrid.Css());

        var assignedCourses = reportData.AssignmentResults
            .Select(x => new { x.CourseCode, x.SessionId })
            .Distinct()
            .ToList();
        
        var courseGroups = assignedCourses
            .Select(x => new {
                reportData.InputData.CourseInfo[x.CourseCode].CourseName,
                x.CourseCode,
                x.SessionId
            })
            .GroupBy(x => x.CourseName)
            .OrderBy(x => x.Key)
            .ToList();

        var allSessions = reportData.AllSessions();
        var grid = doc.Add(new HtmlTableGrid());
        const int numColumns = 3;
        var rowData = new List<XElement>();
        for (var i = 0; i < courseGroups.Count; i++) {
            var group = courseGroups[i];
            var header = new HtmlHeader($"{group.Key}", 3);
            var courseCodes = group.Select(x => x.CourseCode).Distinct().ToList();
            var assignedSessions = group.Select(x => x.SessionId).Distinct().ToList();
            var table = GetCourseTable(allSessions, assignedSessions, courseCodes);
            table.AddColGroup(1, ("style", "width: 25%"));
            var cell = new XElement("div", header, table);
            rowData.Add(cell);
            if (rowData.Count == numColumns || i == courseGroups.Count - 1) {
                grid.AddRow(rowData.ToList());
                rowData.Clear();
            }
        }

        return doc;
    }
    
    private HtmlTable GetCourseTable(List<int> allSessions, List<int> assignedSessions, List<string> courseCodes)
    {
        var table = new HtmlTable(["Session", "Supervisors"]);
        var supervisors = assignedSessions.ToDictionary(x => x, _ => new List<Supervisor>());
        foreach (var code in courseCodes) {
            foreach (var session in reportData.InputData.CourseSchedule[code]) {
                if (supervisors.ContainsKey(session.SessionId) == false)
                    continue;
                supervisors[session.SessionId].AddRange(session.SupervisorIds.Select(x => SupervisorMap[x]));
            }
        }

        foreach (var session in allSessions) {
            var supervisorsCell = new XElement("div");
            if (supervisors.TryGetValue(session, out var sessionSupervisors)) {
                for (int i = 0; i < sessionSupervisors.Count; i++) {
                    var supervisor = sessionSupervisors[i];
                    supervisorsCell.Add($"{supervisor.FirstName} {supervisor.LastName}");
                    if (i < sessionSupervisors.Count - 1)
                        supervisorsCell.Add(new HtmlBreak());
                }
            }

            table.AddRow([new HtmlCell(session), new HtmlCell(supervisorsCell)]);
        }

        return table;
    } 
}