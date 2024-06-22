using System.Globalization;
using CsvHelper.Configuration;
using Database.Objects;

namespace HIDLib.DataImport;

public static class CsvReader
{
    public static List<CourseInfo> ReadCourseInfo(string filePath, string eventId)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<Mappers.CourseInfoMapper>();

        var records = csv.GetRecords<CourseInfo>().ToList();
        records.ForEach(x => x.EventId = eventId);

        return records;
    }
    
    public static List<StudentInfo> ReadStudentInfo(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<Mappers.StudentInfoMapper>();

        var records = csv.GetRecords<StudentInfo>().ToList();

        return records;
    }

    public static List<StudentCourseRanking> ReadStudentCourseRankings(string filePath, int rankingId)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<Mappers.StudentCourseRankingMapper>();

        var records = csv.GetRecords<StudentCourseRanking>().ToList();
        records.ForEach(x => x.RankingInputId = rankingId);

        return records;
    }

    private static bool MissingColumn(CsvHelper.CsvReader reader, params string[] validNames)
    {
        if (reader.HeaderRecord == null)
            throw new ArgumentException();

        foreach (var name in validNames)
            if (reader.HeaderRecord.Any(x => x.ToUpper().Equals(name.ToUpper())))
                return false;

        return true;
    }

    private static class Mappers
    {
        public sealed class CourseInfoMapper : ClassMap<CourseInfo>
        {
            public CourseInfoMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.EventId).Ignore();
                Map(x => x.CourseName).Optional();
                Map(x => x.Description).Optional();
                Map(x => x.GradesFlags).Optional();
            }
        }

        public sealed class StudentInfoMapper : ClassMap<StudentInfo>
        {
            public StudentInfoMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.StudentId).Optional();
            }
        }
        public sealed class StudentCourseRankingMapper : ClassMap<StudentCourseRanking>
        {
            public StudentCourseRankingMapper()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(x => x.RankingInputId).Optional();
            }
        }
    }
}