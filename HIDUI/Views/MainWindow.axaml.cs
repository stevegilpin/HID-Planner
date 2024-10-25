using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HIDLib;
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
            if (ViewModel.TrySetFilePaths(filePaths, out string errorMessage) == false) {
                // TODO error feedback
                return;
            }
            
            ViewModel.ImportFiles();
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoursesFilePath)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScheduleFilePath)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentsFilePath)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentChoicesFilePath)));
            }
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

        public async void ImportFiles() {
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

        public async void RunReports() {
            await Task.Run(() => DataSimulator.CreateReports());
        }
    }
}