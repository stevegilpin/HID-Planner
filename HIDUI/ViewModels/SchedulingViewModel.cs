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
    private bool isCoursesFileSet = false;
    public bool IsCoursesFileSet {
        get => isCoursesFileSet;
        set {
            isCoursesFileSet = value;
            OnPropertyChanged(nameof(IsCoursesFileSet));
        }
    }
    private string coursesFileName;
    public string CoursesFileName {
        get => coursesFileName;
        set {
            coursesFileName = value;
            OnPropertyChanged(nameof(CoursesFileName));
        }
    }

    private string coursesFilePath;
    public string CoursesFilePath {
        get => coursesFilePath;
        set {
            coursesFilePath = value;
            CoursesFileName = System.IO.Path.GetFileName(CoursesFilePath);
            IsCoursesFileSet = true;
            CoursesStatusColor = "Green";
            OnPropertyChanged(nameof(CoursesFilePath));
        }
    }

    private string coursesStatusColor = "Gray";
    public string CoursesStatusColor {
        get => coursesStatusColor;
        set {
            coursesStatusColor = value;
            OnPropertyChanged(nameof(CoursesStatusColor));
        }
    }
    
    // Schedule
    private bool isScheduleFileSet = false;
    public bool IsScheduleFileSet {
        get => isScheduleFileSet;
        set {
            isScheduleFileSet = value;
            OnPropertyChanged(nameof(IsScheduleFileSet));
        }
    }
    private string scheduleFileName;
    public string ScheduleFileName {
        get => scheduleFileName;
        set {
            scheduleFileName = value;
            OnPropertyChanged(nameof(ScheduleFileName));
        }
    }

    private string scheduleFilePath;
    public string ScheduleFilePath {
        get => scheduleFilePath;
        set {
            scheduleFilePath = value;
            ScheduleFileName = System.IO.Path.GetFileName(ScheduleFilePath);
            IsScheduleFileSet = true;
            ScheduleStatusColor = "Green";
           OnPropertyChanged(nameof(ScheduleFilePath));
        }
    }

    private string scheduleStatusColor = "Gray";
    public string ScheduleStatusColor {
        get => scheduleStatusColor;
        set {
            scheduleStatusColor = value;
            OnPropertyChanged(nameof(ScheduleStatusColor));
        }
    }

    // Students
    private bool isStudentsFileSet = false;
    public bool IsStudentsFileSet {
        get => isStudentsFileSet;
        set {
            isStudentsFileSet = value;
            OnPropertyChanged(nameof(IsStudentsFileSet));
        }
    }
    private string studentsFileName;
    public string StudentsFileName {
        get => studentsFileName;
        set {
            studentsFileName = value;
            OnPropertyChanged(nameof(StudentsFileName));
        }
    }
    private string studentsFilePath;
    public string StudentsFilePath {
        get => studentsFilePath;
        set {
            studentsFilePath = value;
            StudentsFileName = System.IO.Path.GetFileName(StudentsFilePath);
            IsStudentsFileSet = true;
            StudentsStatusColor = "Green";
            OnPropertyChanged(nameof(StudentsFilePath));
        }
    }

    private string studentsStatusColor = "Gray";
    public string StudentsStatusColor {
        get => studentsStatusColor;
        set {
            studentsStatusColor = value;
            OnPropertyChanged(nameof(StudentsStatusColor));
        }
    }

    // Student Choices
    private bool isStudentChoicesFileSet = false;
    public bool IsStudentChoicesFileSet {
        get => isStudentChoicesFileSet;
        set {
            isStudentChoicesFileSet = value;
            OnPropertyChanged(nameof(IsStudentChoicesFileSet));
        }
    }
    private string studentChoicesFileName;
    public string StudentChoicesFileName {
        get => studentChoicesFileName;
        set {
            studentChoicesFileName = value;
            OnPropertyChanged(nameof(StudentChoicesFileName));
        }
    }
    private string studentChoicesFilePath;
    public string StudentChoicesFilePath {
        get => studentChoicesFilePath;
        set {
            studentChoicesFilePath = value;
            StudentChoicesFileName = System.IO.Path.GetFileName(StudentChoicesFilePath);
            IsStudentChoicesFileSet = true;
            StudentChoicesStatusColor = "Green";
            OnPropertyChanged(nameof(StudentChoicesFilePath));
        }
    }

    private string studentChoicesStatusColor = "Gray";
    public string StudentChoicesStatusColor {
        get => studentChoicesStatusColor;
        set {
            studentChoicesStatusColor = value;
            OnPropertyChanged(nameof(StudentChoicesStatusColor));
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
    }

    private bool InputsAreReady() => !string.IsNullOrEmpty(CoursesFilePath)
                                  && !string.IsNullOrEmpty(ScheduleFilePath)
                                  && !string.IsNullOrEmpty(StudentsFilePath)
                                  && !string.IsNullOrEmpty(StudentChoicesFilePath);

    public void RemoveCoursesFile() {
        CoursesFilePath = null;
        IsCoursesFileSet = false;
        CoursesStatusColor = "Gray";
    }

    public void RemoveScheduleFile() {
        ScheduleFilePath = null;
        IsScheduleFileSet = false;
        ScheduleStatusColor = "Gray";
    }

    public void RemoveStudentsFile() {
        StudentsFilePath = null;
        IsStudentsFileSet = false;
        StudentsStatusColor = "Gray";
    }

    public void RemoveStudentChoicesFile() {
        StudentChoicesFilePath = null;
        IsStudentChoicesFileSet = false;
        StudentChoicesStatusColor = "Gray";
    }

    public async void RunScheduling() {
        if (InputsAreReady()) {
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
