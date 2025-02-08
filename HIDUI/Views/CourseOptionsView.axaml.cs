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
        CoursesFileSelector.InfoText = ViewModelUtilities.GetCoursesCsvInfoText();
    }

    private async void ChooseFileOnClick(object sender, RoutedEventArgs e)
    {
        var filePaths = await this.ChooseOneFilePathAsync("Select Import Files");
        if (string.IsNullOrEmpty(filePaths))
            return;

        ViewModel.SetCoursesFile(filePaths);
    }

    private void SaveHtmlFileOnClick(object sender, RoutedEventArgs e)
    {
        try {
            var folder = SettingsManager.GetDefaultOutputFolder();
            ViewModel.SaveHtmlFile(folder);
            ViewUtilities.OpenFolderInExplorer(folder);
        }
        catch (Exception exception) {
            this.NotifyException(exception, true);
        }
    }

    private void SaveTextFileOnClick(object sender, RoutedEventArgs e)
    {
        var folder = SettingsManager.GetDefaultOutputFolder();
        ViewModel.SaveTextFile(folder);
        ViewUtilities.OpenFolderInExplorer(folder);
    }

    private void SetEventDescription(object sender, TextChangedEventArgs textChangedEventArgs) => ViewModel.SetEventDescription(EventDescriptionBox.Text);
    private void SetDocumentHeader(object sender, TextChangedEventArgs textChangedEventArgs) => ViewModel.SetDocumentHeader(DocumentHeaderBox.Text);
}