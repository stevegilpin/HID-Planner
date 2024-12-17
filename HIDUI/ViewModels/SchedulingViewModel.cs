using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Data;
using DynamicData;
using HIDLib;
using HIDLib.Analytics;
using HIDLib.IO;

namespace HIDUI.ViewModels
{
    public class SchedulingViewModel : PageViewModelBase
    {

        public new event PropertyChangedEventHandler PropertyChanged;

        private bool isSettingsShown = false;
        public bool IsSettingsShown {
            get => isSettingsShown;
            set {
                isSettingsShown = value;
                OnPropertyChanged(nameof(IsSettingsShown));
            }
        }

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

        private string dbFileLocation;
        public string DbFileLocation {
            get => dbFileLocation;
            set {
                dbFileLocation = value;
                OnPropertyChanged(nameof(DbFileLocation));
            }
        }

        private string defaultInputsFolder;
        public string DefaultInputsFolder {
            get => defaultInputsFolder;
            set {
                defaultInputsFolder = value;
                OnPropertyChanged(nameof(DefaultInputsFolder));
            }
        }

        private string defaultOutputsFolder;
        public string DefaultOutputsFolder {
            get => defaultOutputsFolder;
            set {
                defaultOutputsFolder = value;
                OnPropertyChanged(nameof(DefaultOutputsFolder));
            }
        }

        private int costFunctionExponent;
        public int CostFunctionExponent {
            get => costFunctionExponent;
            set {
                costFunctionExponent = value;
                OnPropertyChanged(nameof(CostFunctionExponent));
            }
        }

        private int badCourseRankCutoff;
        public int BadCourseRankCutoff {
            get => badCourseRankCutoff;
            set {
                badCourseRankCutoff = value;
                OnPropertyChanged(nameof(BadCourseRankCutoff));
            }
        }

        public ObservableCollection<SchedulingRunAnalyticData> Runs { get; } = new(LoadRunData());

        public bool TrySetFilePaths(string[] paths, out string errorMessage) {
            var courseInfo = paths.Where(CsvReadUtil.HeaderMatchesCourseInfo).ToList();
            if (courseInfo.Count > 1) {
                errorMessage = "Multiple files found matching required headers for course info data.";
                return false;
            }

            var courseSchedule = paths.Where(CsvReadUtil.HeaderMatchesCourseSchedule).ToList();
            if (courseSchedule.Count > 1) {
                errorMessage = "Multiple files found matching required headers for course schedule data.";
                return false;
            }

            var studentInfo = paths.Where(CsvReadUtil.HeaderMatchesStudentInfo).ToList();
            if (studentInfo.Count > 1) {
                errorMessage = "Multiple files found matching required headers for student info data.";
                return false;
            }

            var studentChoices = paths.Where(CsvReadUtil.HeaderMatchesStudentCourseRankings).ToList();
            if (studentChoices.Count > 1) {
                errorMessage = "Multiple files found matching required headers for students' course ranking data.";
                return false;
            }

            if (courseInfo.Count == 1) {
                CoursesFilePath = courseInfo.Single();
            }
            if (courseSchedule.Count == 1) {
                ScheduleFilePath = courseSchedule.Single();
            }
            if (studentInfo.Count == 1) {
                StudentsFilePath = studentInfo.Single();
            }
            if (studentChoices.Count == 1) {
                StudentChoicesFilePath = studentChoices.Single();
            }
            errorMessage = null;
            return true;
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

        public void ClearDatabase() {
            DataManager.ClearApplicationData();
        }

        private static List<SchedulingRunAnalyticData> LoadRunData() => [.. SchedulingRunSelector.GetSchedulingRunData().OrderByDescending(x => x.RunId)];
        
        public void ToggleIsSettingsShown() {
            IsSettingsShown = !IsSettingsShown;
            if (IsSettingsShown)
                PopulateSettings();
        }

        private void PopulateSettings() {
            DbFileLocation = SettingsManager.GetDatabaseFileLocation();
            DefaultInputsFolder = SettingsManager.GetDefaultInputsFolder();
            DefaultOutputsFolder = SettingsManager.GetDefaultOutputFolder();
            CostFunctionExponent = SettingsManager.GetCostFunctionExponent();
            BadCourseRankCutoff = SettingsManager.GetBadCourseRankCutoff();
        }
        
        public override bool CanNavigateNext 
        { 
            get => true;
            protected set => throw new NotSupportedException(); 
        }

        public override bool CanNavigatePrevious
        {
            get => false;
            protected set => throw new NotSupportedException();
        }
    }
}