using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System;

namespace TestApp.HIDUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {

    private string coursesFilePath;
    public string CoursesFilePath {
        get => coursesFilePath;
        set => this.RaiseAndSetIfChanged(ref coursesFilePath, value);
    }

    private string scheduleFilePath;
    public string ScheduleFilePath {
        get => scheduleFilePath;
        set => this.RaiseAndSetIfChanged(ref scheduleFilePath, value);
    }

    private string studentsFilePath;
    public string StudentsFilePath {
        get => studentsFilePath;
        set => this.RaiseAndSetIfChanged(ref studentsFilePath, value);
    }

    private string studentChoicesFilePath;
    public string StudentChoicesFilePath {
        get => studentChoicesFilePath;
        set => this.RaiseAndSetIfChanged(ref studentChoicesFilePath, value);
    }

    // The importCoursesCommand is bound to a button/menu item in the UI.
    public ReactiveCommand<Unit, Unit> importCoursesCommand { get; }
    // The importCoursesInteraction requests the UI to show the file open dialog.
    public Interaction<Unit, string?> importCoursesInteraction { get; }

    public ReactiveCommand<Unit, Unit> importScheduleCommand { get; }
    public Interaction<Unit, string?> importScheduleInteraction { get; }

    public ReactiveCommand<Unit, Unit> importStudentsCommand { get; }
    public Interaction<Unit, string?> importStudentsInteraction { get; }

    public ReactiveCommand<Unit, Unit> importStudentChoicesCommand { get; }
    public Interaction<Unit, string?> importStudentChoicesInteraction { get; }

    public MainWindowViewModel() {
        importCoursesCommand = ReactiveCommand.CreateFromTask(setCoursesFilePath);
        importCoursesInteraction = new Interaction<Unit, string?>();
        
        importScheduleCommand = ReactiveCommand.CreateFromTask(setScheduleFilePath);
        importScheduleInteraction = new Interaction<Unit, string?>();
        
        importStudentsCommand = ReactiveCommand.CreateFromTask(setStudentsFilePath);
        importStudentsInteraction = new Interaction<Unit, string?>();
        
        importStudentChoicesCommand = ReactiveCommand.CreateFromTask(setStudentChoicesFilePath);
        importStudentChoicesInteraction = new Interaction<Unit, string?>();
    }

    private async Task setCoursesFilePath() {
        coursesFilePath = await importCoursesInteraction.Handle(Unit.Default);
        Console.WriteLine(coursesFilePath);
    }

    private async Task setScheduleFilePath() {
        scheduleFilePath = await importScheduleInteraction.Handle(Unit.Default);
        Console.WriteLine(scheduleFilePath);
    }

    private async Task setStudentsFilePath() {
        studentsFilePath = await importStudentsInteraction.Handle(Unit.Default);
        Console.WriteLine(studentsFilePath);
    }

    private async Task setStudentChoicesFilePath() {
        studentChoicesFilePath = await importStudentChoicesInteraction.Handle(Unit.Default);
        Console.WriteLine(studentChoicesFilePath);
    }
}
