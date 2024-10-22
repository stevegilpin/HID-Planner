using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TestApp;

namespace HIDUI.Views {

    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        public async void ImportCoursesOnClick(object sender, RoutedEventArgs args) {
            string coursesFilePath = await ChooseFilePath("Import Courses CSV");
            // var dialog = new OpenFileDialog();

            // if (dialog.ShowDialog() == true)
            // {
            // // Consider to create the FileStream here to handle errors 
            // // related to the user's picked file in the view. 
            // // If opening the FileStream succeeds, we can pass it over to the viewmodel.
            // // Also consider wrapping the actual call to the view model into a try-catch block 
            // // in order to provide feedback to the user if needed.
            // string destinationFilePath = dialog.FileName;
            (this.DataContext as MainWindowViewModel)?.ImportCourses(coursesFilePath);
            // }
        }

        public async void ImportScheduleOnClick(object sender, RoutedEventArgs args) {
            string scheduleFilePath = await ChooseFilePath("Import Schedule CSV");
            (this.DataContext as MainWindowViewModel)?.ImportSchedule(scheduleFilePath);
        }

        public async void ImportStudentsOnClick(object sender, RoutedEventArgs args) {
            string studentsFilePath = await ChooseFilePath("Import Students CSV");
            (this.DataContext as MainWindowViewModel)?.ImportStudents(studentsFilePath);
        }

        public async void ImportStudentChoicesOnClick(object sender, RoutedEventArgs args) {
            string studentChoicesFilePath = await ChooseFilePath("Import Student Choices CSV");
            (this.DataContext as MainWindowViewModel)?.ImportStudentChoices(studentChoicesFilePath);
        }

        public async Task<string> ChooseFilePath(string title) {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
{
                Title = title,
                AllowMultiple = false
            });

            if (files.Count == 1) {
                return files[0].Path.LocalPath;
            }
            // TODO: error handling and not selecting file?
            return "";
        }
    }

    public partial class MainWindowViewModel: INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string coursesFileName = "";
        public string CoursesFileName {
            get => coursesFileName;
            set {
                coursesFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoursesFileName)));
            }
        }

        private string coursesFilePath = "";
        public string CoursesFilePath {
            get => coursesFilePath;
            set {
                coursesFilePath = value;
                CoursesFileName = System.IO.Path.GetFileName(CoursesFilePath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoursesFilePath)));
            }
        }
        
        private string scheduleFileName = "";
        public string ScheduleFileName {
            get => scheduleFileName;
            set {
                scheduleFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScheduleFileName)));
            }
        }

        private string scheduleFilePath = "";
        public string ScheduleFilePath {
            get => scheduleFilePath;
            set {
                scheduleFilePath = value;
                ScheduleFileName = System.IO.Path.GetFileName(ScheduleFilePath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScheduleFilePath)));
            }
        }

        private string studentsFileName = "";
        public string StudentsFileName {
            get => studentsFileName;
            set {
                studentsFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentsFileName)));
            }
        }
        private string studentsFilePath = "";
        public string StudentsFilePath {
            get => studentsFilePath;
            set {
                studentsFilePath = value;
                StudentsFileName = System.IO.Path.GetFileName(StudentsFilePath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentsFilePath)));
            }
        }

        private string studentChoicesFileName = "";
        public string StudentChoicesFileName {
            get => studentChoicesFileName;
            set {
                studentChoicesFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentChoicesFileName)));
            }
        }
        private string studentChoicesFilePath = "";
        public string StudentChoicesFilePath {
            get => studentChoicesFilePath;
            set {
                studentChoicesFilePath = value;
                StudentChoicesFileName = System.IO.Path.GetFileName(StudentChoicesFilePath);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StudentChoicesFilePath)));
            }
        }

        public void ImportCourses(string filePathSelection) {
            CoursesFilePath = filePathSelection;
        }

        public void ImportSchedule(string filePathSelection) {
            ScheduleFilePath = filePathSelection;
        }

        public void ImportStudents(string filePathSelection) {
            StudentsFilePath = filePathSelection;
        }

        public void ImportStudentChoices(string filePathSelection) {
            StudentChoicesFilePath = filePathSelection;
        }

        public async void ImportFiles() {
            if (CoursesFilePath != ""
            && ScheduleFilePath != ""
            && StudentsFilePath != ""
            && StudentChoicesFilePath != "") {
                await Task.Run(() => 
                    DataSimulator.RunScheduling(
                        CoursesFilePath,
                        ScheduleFilePath,
                        StudentsFilePath,
                        StudentChoicesFilePath
                    )
                );
            }
        }

        public async void RunReports() {
            await Task.Run(() => DataSimulator.CreateReports());
        }
    }
}