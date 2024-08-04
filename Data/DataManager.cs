using System;
using System.Collections.Generic;
using System.Linq;
using Data.Database;
using Data.Objects;

namespace Data;

public static class DataManager
{
    public static class Students
    {
        public static int SaveStudentInfo(List<StudentInfo> infoRows, List<StudentCourseRanking> rankingRows, string eventId, string inputDescription)
        {
            using var db = DbProvider.GetDbContext();
            var inputId = 1; // todo create db sequence for this
            if (db.StudentRankingInputDescription.Any())
                inputId = db.StudentRankingInputDescription.Max(x => x.InputId) + 1;

            infoRows.ForEach(x => x.RankingInputId = inputId);
            rankingRows.ForEach(x => x.RankingInputId = inputId);

            db.StudentRankingInputDescription.Add(new() {
                InputId = inputId,
                EventId = eventId,
                Description = inputDescription,
            });
            db.StudentInfo.AddRange(infoRows);
            db.StudentCourseRanking.AddRange(rankingRows);
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

        public static List<StudentInputData> GetInputData(int inputId)
        {
            using var db = DbProvider.GetDbContext();
            var studentInfo = db.StudentInfo.Where(x => x.RankingInputId == inputId).ToList();
            var courseRanking = db.StudentCourseRanking
                .Where(x => x.RankingInputId == inputId)
                .ToList()
                .GroupBy(x => x.StudentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return studentInfo.Select(x => new StudentInputData {
                StudentInfo = x,
                CourseRankings = courseRanking[x.StudentId],
            }).ToList();
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
        public static int SaveSchedulingOutput(CourseAssignmentRunInfo runInfo, List<CourseAssignmentResult> results)
        {
            using var db = DbProvider.GetDbContext();

            int runId = 1;
            if (db.CourseAssignmentRunInfo.Any())
                runId = db.CourseAssignmentRunInfo.Max(x => x.RunId) + 1;

            runInfo.RunId = runId;
            results.ForEach(x => x.RunId = runId);

            db.CourseAssignmentRunInfo.Add(runInfo);
            db.CourseAssignmentResult.AddRange(results);
            db.SaveChanges();

            return runId;
        }

        public static SchedulingOutputData LoadSavedData(int assignmentRunId)
        {
            using var db = DbProvider.GetDbContext();
            var run = db.CourseAssignmentRunInfo.Single(x => x.RunId == assignmentRunId);
            var results = db.CourseAssignmentResult.Where(x => x.RunId == assignmentRunId).ToList();
            var courseInputs = Courses.GetInputData(run.CourseScheduleInputId).ToDictionary(x => x.CourseInfo.CourseCode);
            var studentInputs = Students.GetInputData(run.StudentRankingInputId).ToDictionary(x => x.StudentInfo.StudentId);

            return new SchedulingOutputData {
                StudentInputs = studentInputs,
                CourseInputs = courseInputs,
                AssignmentResults = results,
                AssignmentRun = run
            };
        }

        public static CourseAssignmentRunInfo LatestRun()
        {
            using var db = DbProvider.GetDbContext();
            var runId = db.CourseAssignmentRunInfo.Max(x => x.RunId);
            return db.CourseAssignmentRunInfo.Single(x => x.RunId == runId);
        }
    }
}