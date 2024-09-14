using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Diagnostics;
using System.IO;
using TestApp;
using System.Threading.Tasks;

namespace TestApp.HIDUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void importFiles(object sender, RoutedEventArgs args)
    {
        await Task.Run(() => DataSimulator.RunScheduling());
    }

    private async void runReports(object sender, RoutedEventArgs args)
    {
        await Task.Run(() => DataSimulator.CreateReports());
    }
    
    private async void importCourses(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Classes CSV",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            // Open reading stream from the first file.
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            // Reads all the content of file as a text.
            var fileContent = await streamReader.ReadToEndAsync();
        }
    }

        private async void importSchedule(object sender, RoutedEventArgs args)
        {

        }

        private async void importStudents(object sender, RoutedEventArgs args)
        {

        }

        private async void importStudentChoices(object sender, RoutedEventArgs args)
        {

        }

}