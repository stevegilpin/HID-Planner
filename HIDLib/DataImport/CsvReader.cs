using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Database;
using Database.Objects;

namespace HIDLib.DataImport;

public static class CsvReader
{
    public static List<CourseInfo> ReadCourseInfo(string filePath, string eventId)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<CourseInfoMapper>();

        var records = csv.GetRecords<CourseInfo>().ToList();
        records.ForEach(x => x.EventId = eventId);

        return records;
    }
    
    private sealed class CourseInfoMapper : ClassMap<CourseInfo>
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
}