using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HIDLib;
using HIDUI.ViewModels;

namespace HIDUI.Views;

public partial class CourseOptionsView : UserControl
{
    private CourseOptionsViewModel ViewModel => DataContext as CourseOptionsViewModel;
    public CourseOptionsView()
    {
        InitializeComponent();
    }
    
    // TODO create a UI utility. Some of these methods were directly copied from scheduling view.
    public async Task<string[]> ChooseFilePath(string title) {
        var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
        });
        return files.Select(file => file.Path.LocalPath).ToArray();
    }

    private async void ChooseFileOnClick(object sender, RoutedEventArgs e)
    {
        var filePaths = await ChooseFilePath("Select Import Files");
        if (filePaths.Length == 0)
            return;

        ViewModel.SetCoursesFile(filePaths.Single());
    }

    private void SaveHtmlFileOnClick(object sender, RoutedEventArgs e)
    {
        var folder = SettingsManager.GetDefaultOutputFolder();
        ViewModel.SaveHtmlFile(folder);

        // open file explorer at output folder
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", folder);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", $"-R \"{folder}\"");
    }

    private void SaveTextFileOnClick(object sender, RoutedEventArgs e)
    {
        var folder = SettingsManager.GetDefaultOutputFolder();
        ViewModel.SaveTextFile(folder);

        // open file explorer at output folder
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", folder);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", $"-R \"{folder}\"");
    }

    private void SetEventDescription(object sender, TextChangedEventArgs textChangedEventArgs) => ViewModel.SetEventDescription(EventDescriptionBox.Text);
    private void SetDocumentHeader(object sender, TextChangedEventArgs textChangedEventArgs) => ViewModel.SetDocumentHeader(DocumentHeaderBox.Text);
}