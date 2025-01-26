using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Data.Objects;
using HIDLib;
using HIDUI.BusinessLogic;
using HIDUI.ViewModels;

namespace HIDUI.Views
{
    public partial class SurveyView : ContentControl
    {
        private SurveyViewModel ViewModel => DataContext as SurveyViewModel;
        
        public SurveyView()
        {
            InitializeComponent();
            SurveyResponseFileSelector.InfoText = ViewModelUtilities.GetSurveyResponsesCsvInfoText();
            StudentsFileSelector.InfoText = ViewModelUtilities.GetStudentsCsvInfoText();
            CoursesFileSelector.InfoText = ViewModelUtilities.GetCoursesCsvInfoText();
        }
        
        private async void ChooseFileOnClick(object sender, RoutedEventArgs e)
        {
            var filePaths = await ChooseFilePath("Select Import Files");
            if (filePaths.Length == 0)
                return;

            ViewModel.SetFilePaths(filePaths);
        }
        
        // TODO create a UI utility. Some of these methods were directly copied from scheduling view.
        public async Task<string[]> ChooseFilePath(string title) {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = title,
                AllowMultiple = true,
            });
            return files.Select(file => file.Path.LocalPath).ToArray();
        }

        private void SaveSchedulingInputOnClick(object sender, RoutedEventArgs e)
        {
            var folder = SettingsManager.GetDefaultOutputFolder();
            ViewModel.SaveSchedulingInput(folder);
            ViewUtilities.OpenFolderInExplorer(folder);
        }

        private void ReconcileFilesOnClick(object sender, RoutedEventArgs e) => ViewModel.ReconcileFiles();

        private void ViewMissingResponses_OnClick(object sender, RoutedEventArgs e)
        {
            NoResponseGrid.IsVisible = true;
            UnexpectedResponseGrid.IsVisible = false;
            DuplicateResponseGrid.IsVisible = false;
            InvalidCourseChoiceGrid.IsVisible = false;
            IncompleteResponseGrid.IsVisible = false;
        }

        private void ViewUnexpectedResponses_OnClick(object sender, RoutedEventArgs e)
        {
            NoResponseGrid.IsVisible = false;
            UnexpectedResponseGrid.IsVisible = true;
            DuplicateResponseGrid.IsVisible = false;
            InvalidCourseChoiceGrid.IsVisible = false;
            IncompleteResponseGrid.IsVisible = false;
        }
        private void ViewDuplicateResponses_OnClick(object sender, RoutedEventArgs e)
        {
            NoResponseGrid.IsVisible = false;
            UnexpectedResponseGrid.IsVisible = false;
            DuplicateResponseGrid.IsVisible = true;
            InvalidCourseChoiceGrid.IsVisible = false;
            IncompleteResponseGrid.IsVisible = false;
        }
        private void ViewInvalidCourseChoices_OnClick(object sender, RoutedEventArgs e)
        {
            NoResponseGrid.IsVisible = false;
            UnexpectedResponseGrid.IsVisible = false;
            DuplicateResponseGrid.IsVisible = false;
            InvalidCourseChoiceGrid.IsVisible = true;
            IncompleteResponseGrid.IsVisible = false;
        }
        private void ViewIncompleteResponses_OnClick(object sender, RoutedEventArgs e)
        {
            NoResponseGrid.IsVisible = false;
            UnexpectedResponseGrid.IsVisible = false;
            DuplicateResponseGrid.IsVisible = false;
            InvalidCourseChoiceGrid.IsVisible = false;
            IncompleteResponseGrid.IsVisible = true;
        }

        private void NoResponseGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            
            var item = e.AddedItems[0];
            ViewModel.UpdateNoResponseCandidates((NoResponseFailure)item);
        }

        private void UnexpectedResponseGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var item = e.AddedItems[0];
            ViewModel.UpdateUnexpectedResponseCandidates((UnexpectedRespondentFailure)item);
        }
    }
}
