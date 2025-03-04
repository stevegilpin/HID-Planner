using System;
using System.Collections.Generic;
using System.Linq;
using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public class RoomAssignmentReport(AssignmentDataSet reportData, List<Rooms> rooms) : IHtmlGenerator
{
    private readonly AssignmentDataSet ReportData = reportData;
    private readonly List<Rooms> Rooms = rooms;

    public HtmlDoc CreateDoc()
    {
        var roomInfo = GetSessionRooms();
        var doc = new HtmlDoc("Room Assignments");
        doc.StyleRuleSets.Add(CSS.TableStyles());
        doc.Add(new HtmlHeader("Room Assignments"));

        var table = doc.Add(new HtmlTable());

        var distinctSessions = roomInfo.Select(x => x.SessionId).Distinct().OrderBy(x => x).ToList();
        var header = new List<object> { "Room Number", "Room Name" }
            .Concat(distinctSessions.Select(x => $"Session {x}"))
            .ToList();
        table.AddHeaderRow(header);
        
        var roomGroups = roomInfo.GroupBy(x => x.RoomCode).OrderBy(g => g.Key);
        var roomDescMap = rooms.ToDictionary(x => x.RoomCode, x => x.Description);
        roomDescMap.Add(UNASSIGNED_ROOM_CODE, "Unassigned courses");
        foreach (var roomGroup in roomGroups) {
            var row = new List<HtmlCell> {
                new(roomGroup.Key),
                new(roomDescMap.GetValueOrDefault(roomGroup.Key, "Room description not specified")),
            };
            foreach (var session in distinctSessions) {
                var matchingCourses = roomGroup.Where(x => x.SessionId == session).OrderBy(x => x.CourseCode).ToList();
                if (matchingCourses.Count == 0) {
                    row.Add(new HtmlCell(string.Empty));
                } else if (matchingCourses.Count == 1) {
                    row.Add(new HtmlCell(matchingCourses.Single().CourseCode));
                } else {
                    var cell = new HtmlCell(string.Join(", ", matchingCourses.Select(x => x.CourseCode)));
                    cell.AddStyle("background-color: yellow");
                    row.Add(cell);
                }
            }
            table.AddRow(row);
        }

        return doc;
    }

    private const string UNASSIGNED_ROOM_CODE = "TBD";
    private List<(int SessionId, string CourseCode, string RoomCode)> GetSessionRooms()
    {
        var courseInfo = ReportData.InputData.CourseInfo;
        return ReportData.AssignmentResults
            .Select(x => (
                x.SessionId,
                x.CourseCode,
                string.IsNullOrWhiteSpace(courseInfo[x.CourseCode].RoomCode) ? UNASSIGNED_ROOM_CODE : courseInfo[x.CourseCode].RoomCode))
            .Distinct()
            .ToList();
    }
}