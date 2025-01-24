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
using HIDLib.ReportGenerators;
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
            checkInputsAreReady();
            OnPropertyChanged(nameof(CoursesFilePath));
        }
    }
    
    // Schedule
    private string scheduleFilePath;
    public string ScheduleFilePath {
        get => scheduleFilePath;
        set {
            scheduleFilePath = value;
            checkInputsAreReady();
            OnPropertyChanged(nameof(ScheduleFilePath));
        }
    }

    // Students
    private string studentsFilePath;
    public string StudentsFilePath {
        get => studentsFilePath;
        set {
            studentsFilePath = value;
            checkInputsAreReady();
            OnPropertyChanged(nameof(StudentsFilePath));
        }
    }

    // Student Choices
    private string studentChoicesFilePath;
    public string StudentChoicesFilePath {
        get => studentChoicesFilePath;
        set {
            studentChoicesFilePath = value;
            checkInputsAreReady();
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

    public ObservableCollection<ProblematicAssignmentsReport.BadAssignment> RunDetails { get; } = [];

    public ObservableCollection<SchedulingRunAnalyticData> Runs { get; } = new(LoadRunData());

    public void TrySetFilePaths(string[] paths)
    {
        CoursesFilePath = ControllerUtils.GetMatchingPath(paths, CoursesFilePath, CsvReadUtil.HeaderMatchesCourseInfo);
        ScheduleFilePath = ControllerUtils.GetMatchingPath(paths, ScheduleFilePath, CsvReadUtil.HeaderMatchesCourseSchedule);
        StudentsFilePath = ControllerUtils.GetMatchingPath(paths, StudentsFilePath, CsvReadUtil.HeaderMatchesStudentInfo);
        StudentChoicesFilePath = ControllerUtils.GetMatchingPath(paths, StudentChoicesFilePath, CsvReadUtil.HeaderMatchesStudentCourseRankings);
        SchedulingResultsFilePath = ControllerUtils.GetMatchingPath(paths, SchedulingResultsFilePath, CsvReadUtil.HeaderMatchesSchedulingResults);
    }

    private bool isRunSchedulingEnabled;
    public bool IsRunSchedulingEnabled {
        get => isRunSchedulingEnabled;
        set {
            isRunSchedulingEnabled = value;
            OnPropertyChanged(nameof(IsRunSchedulingEnabled));
        }
    }

    private bool inputsAreReady;
    public bool InputsAreReady {
        get => inputsAreReady;
        set {
            inputsAreReady = value;
            OnPropertyChanged(nameof(InputsAreReady));
        }
    }

    private void checkInputsAreReady() {
        InputsAreReady = !string.IsNullOrEmpty(CoursesFilePath)
                      && !string.IsNullOrEmpty(ScheduleFilePath)
                      && !string.IsNullOrEmpty(StudentsFilePath)
                      && !string.IsNullOrEmpty(StudentChoicesFilePath);
    }

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
        if (!InputsAreReady)
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
        if (!InputsAreReady || string.IsNullOrEmpty(SchedulingResultsFilePath))
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

    public void ViewRunDetails(int runId) {
        // string runId = string.Empty;
        // await Dispatcher.UIThread.InvokeAsync(() => { runId = ((Button)sender).Name; }, DispatcherPriority.Background);
        
        // todo store this data behind the scenes in Runs so we don't load it repeatedly
        var data = DataManager.LoadDataSet(runId);
        RunDetails.Clear();
        ProblematicAssignmentsReport.FindProblematicAssignments(data).ForEach(x => RunDetails.Add(x));
    }

    public void UpdateRunNameAndNotes(int runId, string name, string notes)
    {
        DataManager.UpdateRunName(runId, name);
        DataManager.UpdateRunNotes(runId, notes);
        Runs.Clear();
        Runs.AddRange(LoadRunData());
    }
}
