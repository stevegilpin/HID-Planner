using Data.Objects;

namespace Data.Database;

internal static class DefaultSettings
{
    public static void PopulateSettingsTable()
    {
        using var db = DbProvider.GetDbContext();
        db.AddRange(new Settings {
            Name = "Hello",
            Value = "World",
            Description = "This is a test.",
            IsWritable = true,
        });
        db.SaveChanges();
    }
}