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
        try {
            var filePaths = await this.ChooseOneFilePathAsync("Select Import Files").ContinueWith(x => this.CheckTaskException(x, true));
            if (string.IsNullOrEmpty(filePaths))
                return;

            ViewModel.SetCoursesFile(filePaths);
        }
        catch (Exception ex) {
            this.NotifyException(ex, true);
        }
    }

    private void SaveCourseDescrFileOnClick(object sender, RoutedEventArgs e)
    {
        try {
            var folder = SettingsManager.GetDefaultOutputFolder();
            ViewModel.SaveCourseDescrFile(folder);
            ViewUtilities.OpenFolderInExplorer(folder);
        }
        catch (Exception exception) {
            this.NotifyException(exception, false);
        }
    }

    private void SaveCourseListFileOnClick(object sender, RoutedEventArgs e)
    {
        try {
            var folder = SettingsManager.GetDefaultOutputFolder();
            ViewModel.SaveCourseListFile(folder);
            ViewUtilities.OpenFolderInExplorer(folder);
        }
        catch (Exception ex) {
            this.NotifyException(ex, false);
        }
    }

    private void SetEventDescription(object sender, TextChangedEventArgs textChangedEventArgs) => ViewModel.SetEventDescription(EventDescriptionBox.Text);
    private void SetDocumentHeader(object sender, TextChangedEventArgs textChangedEventArgs) => ViewModel.SetDocumentHeader(DocumentHeaderBox.Text);
}