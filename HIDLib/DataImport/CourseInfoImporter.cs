using System.Globalization;
using CsvHelper;
using Database;
using Database.Objects;

namespace HIDLib.DataImport;

public static class CourseInfoImporter
{
    public static void ImportCourseInfoFromCsv(string filePath, string eventId)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        // csv.Configuration.MissingFieldFound = args => { };
        var records = csv.GetRecords<CourseInfo>().ToList();
        records.ForEach(x => x.EventId = eventId);
        
        using var db = DbConnection.GetDbContext();
        db.CourseInfo.AddRange(records);
        db.SaveChanges();
    }
}