using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Data.Objects;
using HIDLib;
using HIDUI.BusinessLogic;
using HIDUI.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;

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
                var filePath = await this.ChooseMultipleFilePathsAsync("Choose Input Files")
                    .ContinueWith(x => this.CheckTaskException(x, true));
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
                var folder = System.IO.Path.Combine(SettingsManager.GetDefaultOutputFolder(), "Survey Validation");
                ViewModel.SaveSchedulingInput(folder);
                ViewUtilities.OpenFolderInExplorer(folder);
            }
            catch (Exception ex) {
                this.NotifyException(ex, false);
            }
        }
        
        private void SaveMissingResponsesOnClick(object sender, RoutedEventArgs e)
        {
            try {
                var folder = System.IO.Path.Combine(SettingsManager.GetDefaultOutputFolder(), "Survey Validation");
                ViewModel.SaveMissingResponsesList(folder);
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

        private void ViewMissingResponses_OnClick(object sender, RoutedEventArgs e) => UpdateDataGridVisibility(DataGrids.NoResponse);
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

        private void KeepMatchOnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var surveyResponse = (UnexpectedRespondentFailure)UnexpectedResponseGridData.SelectedItem;
            var surveyName = $"{surveyResponse.Response.FirstName} {surveyResponse.Response.LastName}";
            var studentInfo = (StudentInfo)btn.DataContext!;
            var studentInfoName = $"{studentInfo.FirstName} {studentInfo.LastName}";
            var box = MessageBoxManager.GetMessageBoxCustom(
                new MessageBoxCustomParams {
                    ButtonDefinitions = new List<ButtonDefinition> {
                        new() { Name = surveyName, IsDefault = true},
                        new() { Name = studentInfoName },
                        new() { Name = "Cancel", IsCancel = true, }
                    },
                    ContentTitle = "Select Student Name",
                    ContentMessage = "Which name do you want to use for this student?",
                    Icon = MsBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false,
                    MaxWidth = 500,
                    MaxHeight = 800,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ShowInCenter = true,
                    Topmost = false,
                });

            box.ShowWindowDialogAsync((Window)this.GetVisualRoot())
                .ContinueWith(x => {
                    if (x.Result == surveyName)
                        Dispatcher.UIThread.Invoke(() => ViewModel.ResolveUnexpectedResponse(surveyResponse, studentInfo, true));
                    else if (x.Result == studentInfoName)
                        Dispatcher.UIThread.Invoke(() => ViewModel.ResolveUnexpectedResponse(surveyResponse, studentInfo, false));
                });
        }
    }
}
