using Data;
using Data.Objects;

namespace HIDLib.EndPoints;

public static class CourseScheduler
{
    public static AssignmentDataSet RunCourseScheduling(InputDataSet inputs)
    {
        return SchedulingSolver.RunSolve(inputs);
    }

    public static int Save(AssignmentDataSet outputData, string inputDescription)
    {
        return DataManager.Save(outputData, inputDescription);
    }
}

