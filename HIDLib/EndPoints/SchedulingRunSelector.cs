using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Objects;

namespace HIDLib.EndPoints;

public static class SchedulingRunSelector
{
    public static List<SchedulingRunAnalyticData> GetSchedulingRunData()
    {
        var runs = DataManager.AllRuns();
        var data = new List<SchedulingRunAnalyticData>();
        var rankCutoff = SettingsManager.GetBadCourseRankCutoff();
        foreach (var run in runs) {
            var outputData = DataManager.LoadDataSet(run.RunId);
            data.Add(GetAnalyticData(outputData, rankCutoff));
        }

        return data;
    }
    
    public static SchedulingRunAnalyticData GetAnalyticData(AssignmentDataSet data, int rankCutoff)
    {
        var allRankings = data.InputData.StudentCourseRanking.SelectMany(x => x.Value).ToList();
        var rankingMap = allRankings.ToDictionary(x => (x.StudentId, x.CourseCode));
        double total = 0;
        int badAssignments = 0;
        foreach (var assignment in data.AssignmentResults) {
            var rank = rankingMap[(assignment.StudentId, assignment.CourseCode)].Rank;
            total += SchedulingSolver.GetCourseCost(rank);
            if (rank >= rankCutoff)
                badAssignments++;
        }

        return new SchedulingRunAnalyticData {
            RunId = data.RunInfo.RunId,
            TimeStamp = data.RunInfo.RunTimeStamp,
            TotalObjectiveScore = total,
            BadAssignmentsCount = badAssignments,
        };
    }
}

public class SchedulingRunAnalyticData
{
    public required int RunId { get; init; }
    public required DateTime TimeStamp { get; init; }
    public required double TotalObjectiveScore { get; init; }
    public required int BadAssignmentsCount { get; init; }
}