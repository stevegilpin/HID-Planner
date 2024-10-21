using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Objects;

namespace HIDLib.EndPoints;

public static class SchedulingRunSelector
{
    public static SchedulingRunAnalyticData GetAnalyticData(AssignmentDataSet data)
    {
        var allRankings = data.InputData.StudentCourseRanking.SelectMany(x => x.Value).ToList();
        var rankingMap = allRankings.ToDictionary(x => (x.StudentId, x.CourseCode));
        double total = 0;
        foreach (var assignment in data.AssignmentResults) {
            var rank = rankingMap[(assignment.StudentId, assignment.CourseCode)].Rank;
            total += SchedulingSolver.GetCourseCost(rank);
        }

        var rankingCounts = allRankings.GroupBy(x => x.Rank).Select(g => (g.Key, g.Count())).ToList();

        return new SchedulingRunAnalyticData {
            TotalObjectiveScore = total,
            AssignmentRankCounts = rankingCounts
        };
    }
}

public class SchedulingRunAnalyticData
{
    public required double TotalObjectiveScore { get; init; }
    public required List<(int Rank, int Count)> AssignmentRankCounts { get; init; }
}