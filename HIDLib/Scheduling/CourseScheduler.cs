using Data;
using Data.Objects;

namespace HIDLib.Scheduling;

public static class CourseScheduler
{
    public static AssignmentDataSet RunCourseScheduling(InputDataSet inputs)
    {
        return SchedulingSolver.RunSolve(inputs);
    }

    public static int Save(AssignmentDataSet outputData, string inputName)
    {
        return DataManager.Save(outputData, inputName);
    }
}

