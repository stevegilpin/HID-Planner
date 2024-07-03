using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Objects;

namespace HIDLib;

// TODO move to a new project (DataAccess). Call this DataManager.
public static class DbController
{
    public static class Students
    {
        public static void SaveStudentInfo(List<StudentInfo> dbRows)
        {
            using var db = DbProvider.GetDbContext();
            db.StudentInfo.AddRange(dbRows);
            db.SaveChanges();
        }

        public static int SaveRankingInput(List<StudentCourseRanking> rows, string eventId, string inputDescription)
        {
            using var db = DbProvider.GetDbContext();
            var inputId = 1; // todo create db sequence for this
            if (db.StudentRankingInputDescription.Any())
                inputId = db.StudentRankingInputDescription.Max(x => x.InputId) + 1;

            rows.ForEach(x => x.RankingInputId = inputId);

            db.StudentRankingInputDescription.Add(new() {
                InputId = inputId,
                EventId = eventId,
                Description = inputDescription,
            });
            db.StudentCourseRanking.AddRange(rows);
            db.SaveChanges();

            return inputId;
        }

        public static int GetLatestRankingInputId()
        {
            using var db = DbProvider.GetDbContext();
            return db.StudentRankingInputDescription.Max(x => x.InputId);
        }

        public static List<StudentCourseRanking> GetStudentCourseRankingData(int inputId)
        {
            using var db = DbProvider.GetDbContext();
            return db.StudentCourseRanking.Where(x => x.RankingInputId == inputId).ToList();
        }
    }

    public static class Courses
    {
        public static int SaveCourseInfo(List<CourseInfo> infoRows, List<CourseSchedule> scheduleRows, string eventId, string inputDescription)
        {
            using var db = DbProvider.GetDbContext();
            int inputId = 1;
            if (db.CourseScheduleInputDescription.Any())
                inputId = db.CourseScheduleInputDescription.Max(x => x.InputId) + 1;


            infoRows.ForEach(x => x.CourseScheduleInputId = inputId);
            scheduleRows.ForEach(x => x.CourseScheduleInputId = inputId);
        
            db.CourseScheduleInputDescription.Add(new() {
                InputId = inputId,
                Description = inputDescription,
                EventId = eventId,
            });
            db.CourseInfo.AddRange(infoRows);
            db.CourseSchedule.AddRange(scheduleRows);
            db.SaveChanges();

            return inputId;
        }
        
        public static int GetLatestScheduleInputId()
        {
            using var db = DbProvider.GetDbContext();
            return db.CourseScheduleInputDescription.Max(x => x.InputId);
        }

        public static List<CourseInputData> GetInputData(int inputId)
        {
            using var db = DbProvider.GetDbContext();
            var courseInfo = db.CourseInfo.Where(x => x.CourseScheduleInputId == inputId).ToList();
            var schedules = db.CourseSchedule
                .Where(x => x.CourseScheduleInputId == inputId)
                .ToList()
                .GroupBy(x => x.CourseCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            return courseInfo.Select(x => new CourseInputData {
                CourseInfo = x,
                Schedule = schedules.TryGetValue(x.CourseCode, out var sched) ? sched : [],
            }).ToList();
        }
    }

    public static class Scheduling
    {
        // TODO the business logic should unpack SolveData into data types that live in the data layer
        public static int SaveSchedulingOutput(SchedulingSolver.SolveData solve, string description)
        {
            using var db = DbProvider.GetDbContext();

            int runId = 1;
            if (db.CourseAssignmentRunInfo.Any())
                runId = db.CourseAssignmentRunInfo.Max(x => x.RunId) + 1;

            var runInfo = new CourseAssignmentRunInfo {
                RunId = runId,
                RunTimeStamp = solve.Timestamp,
                CourseScheduleInputId = solve.CourseScheduleInputId,
                StudentRankingInputId = solve.StudentRankingInputId,
                Description = description,
            };

            var resultRows = new List<CourseAssignmentResult>();
            foreach (var variable in solve.Variables) {
                if (variable.Variable.SolutionValue() == 0)
                    continue;
                
                resultRows.Add(new CourseAssignmentResult {
                    RunId = runId,
                    CourseCode = variable.StudentRanking.CourseCode,
                    StudentId = variable.StudentRanking.StudentId,
                    SessionId = variable.SessionId,
                });
            }

            db.CourseAssignmentRunInfo.Add(runInfo);
            db.CourseAssignmentResult.AddRange(resultRows);
            db.SaveChanges();

            return runId;
        }
    }
}

public class CourseInputData
{
    public CourseInfo CourseInfo { get; init; }
    public List<CourseSchedule> Schedule { get; init; }
}