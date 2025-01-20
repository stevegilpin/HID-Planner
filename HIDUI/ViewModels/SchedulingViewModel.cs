using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Data;
using DynamicData;
using HIDLib;
using HIDLib.Analytics;
using HIDLib.IO;
using HIDUI.BusinessLogic;

namespace HIDUI.ViewModels;

public class SchedulingViewModel : ViewModelBase
{
    // Courses
    private string coursesFilePath;
    public string CoursesFilePath {
        get => coursesFilePath;
        set {
            coursesFilePath = value;
            OnPropertyChanged(nameof(CoursesFilePath));
        }
    }
    
    // Schedule
    private string scheduleFilePath;
    public string ScheduleFilePath {
        get => scheduleFilePath;
        set {
            scheduleFilePath = value;
           OnPropertyChanged(nameof(ScheduleFilePath));
        }
    }

    // Students
    private string studentsFilePath;
    public string StudentsFilePath {
        get => studentsFilePath;
        set {
            studentsFilePath = value;
            OnPropertyChanged(nameof(StudentsFilePath));
        }
    }

    // Student Choices
    private string studentChoicesFilePath;
    public string StudentChoicesFilePath {
        get => studentChoicesFilePath;
        set {
            studentChoicesFilePath = value;
            OnPropertyChanged(nameof(StudentChoicesFilePath));
        }
    }

    // Scheduling Results
    private string schedulingResultsFilePath;
    public string SchedulingResultsFilePath {
        get => schedulingResultsFilePath;
        set {
            schedulingResultsFilePath = value;
            OnPropertyChanged(nameof(SchedulingResultsFilePath));
        }
    }

    private SchedulingRunAnalyticData runDetails;
    public SchedulingRunAnalyticData RunDetails {
        get => runDetails;
        set {
            runDetails = value;
            OnPropertyChanged(nameof(RunDetails));
        }
    }

    public ObservableCollection<SchedulingRunAnalyticData> Runs { get; } = new(LoadRunData());

    public void TrySetFilePaths(string[] paths)
    {
        CoursesFilePath = ControllerUtils.GetMatchingPath(paths, CoursesFilePath, CsvReadUtil.HeaderMatchesCourseInfo);
        ScheduleFilePath = ControllerUtils.GetMatchingPath(paths, ScheduleFilePath, CsvReadUtil.HeaderMatchesCourseSchedule);
        StudentsFilePath = ControllerUtils.GetMatchingPath(paths, StudentsFilePath, CsvReadUtil.HeaderMatchesStudentInfo);
        StudentChoicesFilePath = ControllerUtils.GetMatchingPath(paths, StudentChoicesFilePath, CsvReadUtil.HeaderMatchesStudentCourseRankings);
        SchedulingResultsFilePath = ControllerUtils.GetMatchingPath(paths, SchedulingResultsFilePath, CsvReadUtil.HeaderMatchesSchedulingResults);
    }

    private bool InputsAreReady() => !string.IsNullOrEmpty(CoursesFilePath)
                                  && !string.IsNullOrEmpty(ScheduleFilePath)
                                  && !string.IsNullOrEmpty(StudentsFilePath)
                                  && !string.IsNullOrEmpty(StudentChoicesFilePath);

    public void RemoveCoursesFile() {
        CoursesFilePath = null;
    }

    public void RemoveScheduleFile() {
        ScheduleFilePath = null;
    }

    public void RemoveStudentsFile() {
        StudentsFilePath = null;
    }

    public void RemoveStudentChoicesFile() {
        StudentChoicesFilePath = null;
    }

    public void RemoveSchedulingResultsFile()
    {
        SchedulingResultsFilePath = null;
    }

    public async void RunScheduling()
    {
        if (InputsAreReady() == false)
            return;

        await Task.Run(() => {
                var mgr = new SchedulingManager {
                    RunName = "Manual test run", // todo set from UI
                    CourseInfoFilePath = CoursesFilePath,
                    CourseScheduleFilePath = ScheduleFilePath,
                    StudentInfoFilePath = StudentsFilePath,
                    StudentCourseRankingsFilePath = StudentChoicesFilePath,
                };
                var msg = mgr.RunScheduling();
                mgr.Logger.Information(msg);

            }
        );

        Runs.Clear();
        Runs.AddRange(LoadRunData());
    }

    public async void SaveManualScheduling()
    {
        if (InputsAreReady() == false)
            return;
        
        await Task.Run(() => {
                var mgr = new SchedulingManager {
                    RunName = "Manual scheduling test run", // todo set from UI
                    CourseInfoFilePath = CoursesFilePath,
                    CourseScheduleFilePath = ScheduleFilePath,
                    StudentInfoFilePath = StudentsFilePath,
                    StudentCourseRankingsFilePath = StudentChoicesFilePath,
                    ManualSchedulingResultsFilePath = SchedulingResultsFilePath,
                };
                var msg = mgr.SaveManualSchedulingResults();
                mgr.Logger.Information(msg);

            }
        );

        Runs.Clear();
        Runs.AddRange(LoadRunData());
    }

    private static List<SchedulingRunAnalyticData> LoadRunData() => [.. SchedulingRunSelector.GetSchedulingRunData().OrderByDescending(x => x.RunId)];

    public void ViewRunDetails(string runId) {
        // string runId = string.Empty;
        // await Dispatcher.UIThread.InvokeAsync(() => { runId = ((Button)sender).Name; }, DispatcherPriority.Background);
        
        // todo store this data behind the scenes in Runs so we don't load it repeatedly
        // var data = DataManager.LoadDataSet(Int32.Parse(runId));
        // Console.WriteLine(data.)
        // var docs = GetHtmlDocs(data);
        

        // Console.WriteLine("REPORT: ");
        // // Console.WriteLine(docs[0]);
        // Runs.ForEach(Console.WriteLine);
        // Runs.ForEach(x => Console.WriteLine(x.RunId));
    }
}
