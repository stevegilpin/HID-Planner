using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSettingsShown)));
            }
        }

        // Courses
        private bool isCoursesFileSet = false;
        public bool IsCoursesFileSet {
            get => isCoursesFileSet;
            set {
                isCoursesFileSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCoursesFileSet)));
            }
        }
        private string coursesFileName;
        public string CoursesFileName {
            get => coursesFileName;
            set {
                coursesFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoursesFileName)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoursesFilePath)));
            }
        }

        private string coursesStatusColor = "Gray";
        public string CoursesStatusColor {
            get => coursesStatusColor;
            set {
                coursesStatusColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoursesStatusColor)));
            }
        }
        
        // Schedule
        private bool isScheduleFileSet = false;
        public bool IsScheduleFileSet {
            get => isScheduleFileSet;
            set {
                isScheduleFileSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsScheduleFileSet)));
            }
        }
        private string scheduleFileName;
        public string ScheduleFileName {
            get => scheduleFileName;
            set {
                scheduleFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScheduleFileName)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScheduleFilePath)));
            }
        }

        private string scheduleStatusColor = "Gray";
        public string ScheduleStatusColor {
            get => scheduleStatusColor;
            set {
                scheduleStatusColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScheduleStatusColor)));
            }
        }

        // Students
        private bool isStudentsFileSet = false;
        public bool IsStudentsFileSet {
            get => isStudentsFileSet;
            set {
                isStudentsFileSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStudentsFileSet)));
            }
        }
        private string studentsFileName;
        public string StudentsFileName {
            get => studentsFileName;
            set {
                studentsFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentsFileName)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentsFilePath)));
            }
        }

        private string studentsStatusColor = "Gray";
        public string StudentsStatusColor {
            get => studentsStatusColor;
            set {
                studentsStatusColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentsStatusColor)));
            }
        }

        // Student Choices
        private bool isStudentChoicesFileSet = false;
        public bool IsStudentChoicesFileSet {
            get => isStudentChoicesFileSet;
            set {
                isStudentChoicesFileSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStudentChoicesFileSet)));
            }
        }
        private string studentChoicesFileName;
        public string StudentChoicesFileName {
            get => studentChoicesFileName;
            set {
                studentChoicesFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentChoicesFileName)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentChoicesFilePath)));
            }
        }

        private string studentChoicesStatusColor = "Gray";
        public string StudentChoicesStatusColor {
            get => studentChoicesStatusColor;
            set {
                studentChoicesStatusColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentChoicesStatusColor)));
            }
        }

        ObservableCollection<SchedulingRunAnalyticData> Runs { get; } = new(LoadRunData());

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

        private static List<SchedulingRunAnalyticData> LoadRunData() => SchedulingRunSelector.GetSchedulingRunData()
            .OrderByDescending(x => x.RunId).ToList();
        
        public void ToggleIsSettingsShown() {
            IsSettingsShown = !IsSettingsShown;
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