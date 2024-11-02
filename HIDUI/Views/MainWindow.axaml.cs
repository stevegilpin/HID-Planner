using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Data;
using Data.Objects;
using HIDLib;
using HIDLib.EndPoints;
using HIDLib.HTML;
using HIDLib.ReportGenerators;
using TestApp;

namespace HIDUI.Views {

    public partial class MainWindow: Window
    {
        private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;
        
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        public async void ImportFilesOnClick(object sender, RoutedEventArgs args) {
            var filePaths = await ChooseFilePath("Select Import Files");
            ViewModel.TrySetFilePaths(filePaths, out string errorMessage);
        }

        public async Task<string[]> ChooseFilePath(string title) {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = title,
                AllowMultiple = true
            });
            return files.Select(file => file.Path.LocalPath).ToArray();
        }
    }

    public partial class MainWindowViewModel: INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

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

        ObservableCollection<RunInfo> Runs { get; }

        public MainWindowViewModel() {
            List<RunInfo> currentRuns = DataManager.AllRuns();
            Runs = new ObservableCollection<RunInfo>(currentRuns);
        }

        public bool TrySetFilePaths(string[] paths, out string errorMessage) {
            var courseInfo = paths.Where(CsvUtil.HeaderMatchesCourseInfo).ToList();
            if (courseInfo.Count > 1) {
                errorMessage = "Multiple files found matching required headers for course info data.";
                return false;
            }

            var courseSchedule = paths.Where(CsvUtil.HeaderMatchesCourseSchedule).ToList();
            if (courseSchedule.Count > 1) {
                errorMessage = "Multiple files found matching required headers for course schedule data.";
                return false;
            }

            var studentInfo = paths.Where(CsvUtil.HeaderMatchesStudentInfo).ToList();
            if (studentInfo.Count > 1) {
                errorMessage = "Multiple files found matching required headers for student info data.";
                return false;
            }

            var studentChoices = paths.Where(CsvUtil.HeaderMatchesStudentCourseRankings).ToList();
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
            if (InputsAreReady())
                await Task.Run(() =>
                    DataSimulator.RunScheduling(
                        CoursesFilePath,
                        ScheduleFilePath,
                        StudentsFilePath,
                        StudentChoicesFilePath
                    )
                );
        }

        public static async void CreateReports() {
            await Task.Run(() => DataSimulator.CreateReports());
        }
        
        public static List<HtmlDoc> GetHtmlDocs(AssignmentDataSet data)
        {
            var generators = new IHtmlGenerator[] {
                new ProblematicAssignmentsReport(data),
                new ClassRosterReport(data),
                new StudentScheduleGridReport(data),
                new AssignmentAnalysisReport(data),
                new StudentScheduleOverviewReport(data),
            };

            return generators.Select(x => x.CreateDoc()).ToList();
        }

        public static void ExportData(List<HtmlDoc> htmlDocs, AssignmentDataSet data, string outputFolder)
        {
            ReportGenerator.WriteRawData(data, outputFolder);
            foreach (var doc in htmlDocs)
                doc.WriteToFile(outputFolder);
        }

        public static bool TryGetAnalytics(int assignmentRunId, out SchedulingRunAnalyticData analytics, out string errorMessage)
        {
            AssignmentDataSet data;
            try {
                data = DataManager.LoadDataSet(assignmentRunId);
            }
            catch (Exception e) {
                errorMessage = $"Error loading data for run ID {assignmentRunId}:{Environment.NewLine}{e.Message}";
                analytics = null;
                return false;
            }

            try {
                analytics = SchedulingRunSelector.GetAnalyticData(data);
                errorMessage = null;
                return true;
            }
            catch (Exception e) {
                analytics = null;
                errorMessage = $"Error creating analytics:{Environment.NewLine}{e.Message}";
                return false;
            }
        }
    }

    public class RunModel {
        public string RunId { get; set; }
        public DateTime RunTimeStamp { get; set; }
        
        public RunModel(string runId , DateTime runTimeStamp)
        {
            RunId = runId;
            RunTimeStamp = runTimeStamp;
        }
    }
}