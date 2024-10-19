using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using TestApp.HIDUI.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive;

namespace TestApp.HIDUI.Views;

public partial class MainWindow: ReactiveWindow<MainWindowViewModel> {
    public MainWindow() {
        InitializeComponent();
        this.WhenActivated(d => d(ViewModel.importCoursesInteraction.RegisterHandler(importCoursesHandler)));
        this.WhenActivated(d => d(ViewModel.importScheduleInteraction.RegisterHandler(importScheduleHandler)));
        this.WhenActivated(d => d(ViewModel.importStudentsInteraction.RegisterHandler(importStudentsHandler)));
        this.WhenActivated(d => d(ViewModel.importStudentChoicesInteraction.RegisterHandler(importStudentChoicesHandler)));
    }

    private async Task importCoursesHandler(IInteractionContext<Unit, string> interaction) {
        interaction.SetOutput(await chooseFilePath("Import Classes CSV"));
    }

    private async Task importScheduleHandler(IInteractionContext<Unit, string> interaction) {
        interaction.SetOutput(await chooseFilePath("Import Schedule CSV"));
    }

    private async Task importStudentsHandler(IInteractionContext<Unit, string> interaction) {
        interaction.SetOutput(await chooseFilePath("Import Students CSV"));
    }

    private async Task importStudentChoicesHandler(IInteractionContext<Unit, string> interaction) {
        interaction.SetOutput(await chooseFilePath("Import Student Choices CSV"));
    }

    // private async void importCoursesClicked(IInteractionContext<string, bool> context) {
    //     var filePath = await chooseFilePath("Import Classes CSV");
    //     context.SetOutput(filePath);
    // }

    private async void importFiles(object sender, RoutedEventArgs args) {
        if (this.ViewModel.CoursesFilePath != ""
        && this.ViewModel.ScheduleFilePath != ""
        && this.ViewModel.StudentsFilePath != ""
        && this.ViewModel.StudentChoicesFilePath != "") {
            await Task.Run(() => 
                DataSimulator.RunScheduling(
                    this.ViewModel.CoursesFilePath,
                    this.ViewModel.ScheduleFilePath,
                    this.ViewModel.StudentsFilePath,
                    this.ViewModel.StudentChoicesFilePath
                )
            );
        }
    }

    private async void runReports(object sender, RoutedEventArgs args) {
        await Task.Run(() => DataSimulator.CreateReports());
    }

    private async Task<string> chooseFilePath(string title) {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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