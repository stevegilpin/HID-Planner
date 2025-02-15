using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CsvHelper.Configuration;
using Data;
using Data.Objects;
using HIDLib;
using HIDLib.Analytics;
using HIDLib.HTML;
using HIDLib.IO;
using HIDLib.ReportGenerators;
using HIDUI.ViewModels;

namespace HIDUI.Views;

public partial class SchedulingView : ContentControl
{
    private SchedulingViewModel ViewModel => DataContext as SchedulingViewModel;

    public SchedulingView()
    {
        InitializeComponent();
        CoursesFileSelector.InfoText = ViewModelUtilities.GetCoursesCsvInfoText();
        ScheduleFileSelector.InfoText = ViewModelUtilities.GetCourseScheduleCsvInfoText();
        StudentsFileSelector.InfoText = ViewModelUtilities.GetStudentsCsvInfoText();
        StudentChoicesFileSelector.InfoText = ViewModelUtilities.GetStudentChoicesCsvInfoText();
        SchedulingResultsFileSelector.InfoText = ViewModelUtilities.GetSchedulingResultsCsvInfoText();
    }

    public async void ChooseInputFilesOnClick(object sender, RoutedEventArgs args) {
        try {
            var filePaths = await this.ChooseMultipleFilePathsAsync("Choose Input Files");
            ViewModel.TrySetFilePaths(filePaths);
        }
        catch (Exception ex) {
            this.NotifyException(ex, true);
        }
    }

    public void SaveFilesButton_OnClick(object sender, RoutedEventArgs args) {
        if (SelectedRunInfo == null)
            return;

        try {
            var outputFolder = ViewModel.WriteOutputFiles(SelectedRunInfo.RunId);
            Console.WriteLine($"Reports written to: {outputFolder}");
            ViewUtilities.OpenFolderInExplorer(outputFolder);
        }
        catch (Exception ex) {
            this.NotifyException(ex, false);
        }
    }

    private SchedulingRunAnalyticData SelectedRunInfo => RunGrid.SelectedItem as SchedulingRunAnalyticData;
    private void DataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedRunInfo == null) {
            ViewModel.RunDetails.Clear();
            RunNameTextBox.Text = string.Empty;
            RunNotesTextBox.Text = string.Empty;
            SaveFilesButton.IsEnabled = false;
            UpdateRunInfoButton.IsEnabled = false;
            return;
        }

        try {
            ViewModel.ViewRunDetails(SelectedRunInfo.RunId);
            RunNameTextBox.Text = SelectedRunInfo.Name;
            RunNotesTextBox.Text = SelectedRunInfo.Notes;
            SaveFilesButton.IsEnabled = true;
            UpdateRunInfoButton.IsEnabled = true;
        }
        catch (Exception ex) {
            this.NotifyException(ex, false);
        }
    }

    private void UpdateRunInfoButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (SelectedRunInfo == null)
            return;

        try {
            ViewModel.UpdateRunNameAndNotes(SelectedRunInfo.RunId, RunNameTextBox.Text, RunNotesTextBox.Text);
        }
        catch (Exception ex) {
            this.NotifyException(ex, true);
        }
    }

    private async void RunSchedulingButton_OnClick(object sender, RoutedEventArgs e)
    {
        try {
            IsEnabled = false;
            await ViewModel.RunSchedulingAsync()
                .ContinueWith(_ => Dispatcher.UIThread.Invoke(() => IsEnabled = true));
        }
        catch (Exception ex) {
            this.NotifyException(ex, true);
        }
    }

    private async void SaveManualSchedulingButton_OnClick(object sender, RoutedEventArgs e)
    {
        try {
            IsEnabled = false;
            await ViewModel.SaveManualSchedulingAsync()
                .ContinueWith(_ => Dispatcher.UIThread.Invoke(() => IsEnabled = true));
        }
        catch (Exception ex) {
            this.NotifyException(ex, true);
        }
    }
}