using System.Globalization;
using CsvHelper.Configuration;

namespace SuperGrouper;

public sealed class SupervisorScheduleDataMapper : ClassMap<SupervisorScheduleData>
{
    public SupervisorScheduleDataMapper()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.InstructorId).Optional();
        Map(x => x.SupervisorId).Optional();
    }
}