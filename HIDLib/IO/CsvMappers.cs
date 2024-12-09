using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Data.Objects;

namespace HIDLib.IO;

public static class CsvMappers
{
    public sealed class CourseInfoMapper : ClassMap<CourseInfo>
    {
        public CourseInfoMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(x => x.RunId).Ignore();
            Map(x => x.MinCapacity).Optional();
            Map(x => x.MaxCapacity).Optional();
            Map(x => x.MinGrade).Convert(ParseMinGrade);
            Map(x => x.MaxGrade).Convert(ParseMaxGrade);
            Map(x => x.MinGrade).Convert(WriteMinGradeNumber);
            Map(x => x.MaxGrade).Convert(WriteMaxGradeNumber);
        }

        private string WriteMinGradeNumber(ConvertToStringArgs<CourseInfo> args) => WriteGradeNumber(args.Value.MinGrade);
        private string WriteMaxGradeNumber(ConvertToStringArgs<CourseInfo> args) => WriteGradeNumber(args.Value.MaxGrade);
        private string WriteGradeNumber(int gradeNumber) => gradeNumber > 0 ? gradeNumber.ToString() : $"K{gradeNumber + 5}";

        private int ParseMinGrade(ConvertFromStringArgs args)
        {
            var gradeCode = args.Row.GetField<string>(nameof(CourseInfo.MinGrade));
            return ParseGradeNumber(gradeCode);
        }
        private int ParseMaxGrade(ConvertFromStringArgs args)
        {
            var gradeCode = args.Row.GetField<string>(nameof(CourseInfo.MaxGrade));
            return ParseGradeNumber(gradeCode);
        }
        private int ParseGradeNumber(string gradeCode)
        {
            if (int.TryParse(gradeCode, out var grade))
                return grade;

            if (int.TryParse(gradeCode!.ToUpper().Replace("K", string.Empty), out var kindergartenGrade))
                return kindergartenGrade - 5; // K5 = 0, K4 = -1, K3 = -2, etc.

            throw new ArgumentException();
        }
    }

    public sealed class StudentInfoMapper : ClassMap<StudentInfo>
    {
        public StudentInfoMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(x => x.RunId).Ignore();
            Map(x => x.StudentId).Optional(); // todo should be ignored or required
        }
    }

    public sealed class StudentCourseRankingMapper : ClassMap<StudentCourseRanking>
    {
        public StudentCourseRankingMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(x => x.RunId).Ignore();
        }
    }
        
    public sealed class CourseScheduleMapper : ClassMap<CourseSchedule>
    {
        public CourseScheduleMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(x => x.RunId).Ignore();
            Map(x => x.RoomCode).Optional();
            Map(x => x.Instructor).Optional();
            Map(x => x.Supervisor).Optional();
        }
    }

    public sealed class CourseAssignmentResultsMapper : ClassMap<CourseAssignmentResult>
    {
        public CourseAssignmentResultsMapper()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(x => x.RunId).Ignore();
        }
    }
}