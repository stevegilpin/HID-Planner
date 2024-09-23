using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Objects;

namespace HIDLib.EndPoints;

public static class RunSelectionController
{
    public static List<RunInfo> GetAllRuns() => DataManager.AllRuns();
    public static RunInfo LatestRun() => DataManager.LatestRun();

    public static bool TryGetAnalytics(int assignmentRunId, out RunSelectionAnalytics analytics, out string errorMessage)
    {
        AssignmentDataSet data;
        try {
            data = DataManager.LoadDataSet(assignmentRunId);
        }
        catch (Exception e) {
            errorMessage = $"Error loading data for run ID {assignmentRunId}:{Environment.NewLine}{e.Message}";
            analytics = null;
            return false;
        }

        try {
            var allRankings = data.InputData.StudentCourseRanking.SelectMany(x => x.Value).ToList();
            var rankingMap = allRankings.ToDictionary(x => (x.StudentId, x.CourseCode));
            double total = 0;
            foreach (var assignment in data.AssignmentResults) {
                var rank = rankingMap[(assignment.StudentId, assignment.CourseCode)].Rank;
                total += SchedulingSolver.GetCourseCost(rank);
            }

            var rankingCounts = allRankings.GroupBy(x => x.Rank).Select(g => (g.Key, g.Count())).ToList();

            analytics = new RunSelectionAnalytics {
                TotalObjectiveScore = total,
                AssignmentRankCounts = rankingCounts
            };

            errorMessage = null;
            return true;
        }
        catch (Exception e) {
            analytics = null;
            errorMessage = $"Error creating analytics:{Environment.NewLine}{e.Message}";
            return false;
        }
    }
}

public class RunSelectionAnalytics
{
    public required double TotalObjectiveScore { get; init; }
    public required List<(int Rank, int Count)> AssignmentRankCounts { get; init; }
}