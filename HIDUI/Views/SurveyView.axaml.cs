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
            try {
                var filePath = await this.ChooseMultipleFilePathsAsync("Choose Input Files");
                if (filePath.Length == 0)
                    return;

                ViewModel.SetFilePaths(filePath);
            }
            catch (Exception ex) {
                this.NotifyException(ex, true);
            }
        }

        private void SaveSchedulingInputOnClick(object sender, RoutedEventArgs e)
        {
            try {
                var folder = SettingsManager.GetDefaultOutputFolder();
                ViewModel.SaveSchedulingInput(folder);
                ViewUtilities.OpenFolderInExplorer(folder);
            }
            catch (Exception ex) {
                this.NotifyException(ex, false);
            }
        }

        private void ReconcileFilesOnClick(object sender, RoutedEventArgs e)
        {
            try {
                ViewModel.ReconcileFiles();
            }
            catch (Exception ex) {
                this.NotifyException(ex, true);
            }
        }

        private enum DataGrids
        {
            NoResponse,
            UnexpectedResponse,
            DuplicateResponse,
            InvalidCourseChoice,
            IncompleteResponse,
        }

        private void UpdateDataGridVisibility(DataGrids visibleGrid)
        {
            NoResponseGrid.IsVisible = visibleGrid == DataGrids.NoResponse;
            UnexpectedResponseGrid.IsVisible = visibleGrid == DataGrids.UnexpectedResponse;
            DuplicateResponseGrid.IsVisible = visibleGrid == DataGrids.DuplicateResponse;
            InvalidCourseChoiceGrid.IsVisible = visibleGrid == DataGrids.InvalidCourseChoice;
            IncompleteResponseGrid.IsVisible = visibleGrid == DataGrids.IncompleteResponse;
        }

        private void ViewMissingResponses_OnClick(object sender, RoutedEventArgs e) => UpdateDataGridVisibility(DataGrids.UnexpectedResponse);
        private void ViewUnexpectedResponses_OnClick(object sender, RoutedEventArgs e) => UpdateDataGridVisibility(DataGrids.UnexpectedResponse);
        private void ViewDuplicateResponses_OnClick(object sender, RoutedEventArgs e) => UpdateDataGridVisibility(DataGrids.DuplicateResponse);
        private void ViewInvalidCourseChoices_OnClick(object sender, RoutedEventArgs e) => UpdateDataGridVisibility(DataGrids.InvalidCourseChoice);
        private void ViewIncompleteResponses_OnClick(object sender, RoutedEventArgs e) => UpdateDataGridVisibility(DataGrids.IncompleteResponse);

        private void NoResponseGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            try {
                var item = e.AddedItems[0];
                ViewModel.UpdateNoResponseCandidates((NoResponseFailure)item);
            }
            catch (Exception ex) {
                this.NotifyException(ex, false);
            }
        }

        private void UnexpectedResponseGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            try {
                var item = e.AddedItems[0];
                ViewModel.UpdateUnexpectedResponseCandidates((UnexpectedRespondentFailure)item);
            }
            catch (Exception ex) {
                this.NotifyException(ex, false);
            }
        }
    }
}
