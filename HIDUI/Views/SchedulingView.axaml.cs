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

    public async void ImportFilesOnClick(object sender, RoutedEventArgs args) {
        var filePaths = await ChooseMultipleFilePaths("Select Import Files");
        ViewModel.TrySetFilePaths(filePaths);
    }

    public async void TableRowSaveData(object sender, RoutedEventArgs args) {
        string runId = string.Empty;
        await Dispatcher.UIThread.InvokeAsync(() => { runId = ((Button)sender).Name; }, DispatcherPriority.Background);

        // todo store this data behind the scenes in Runs so we don't load it repeatedly
        var data = DataManager.LoadDataSet(Int32.Parse(runId));
        var docs = GetHtmlDocs(data);

        var outputFolder = Path.Combine(SettingsManager.GetDefaultOutputFolder(), $"Run {runId}");
        CsvWriteUtil.WriteResultsToCsv(data, outputFolder);
        foreach (var doc in docs)
            doc.WriteToFile(outputFolder);
        Console.WriteLine($"Reports written to: {outputFolder}");
        
        // open file explorer at output folder
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", outputFolder);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", $"-R \"{outputFolder}\"");
    }
    
    // TODO: These static functions could be moved to a service
    public static List<HtmlDoc> GetHtmlDocs(AssignmentDataSet data)
    {
        var generators = new IHtmlGenerator[] {
            new ProblematicAssignmentsReport(data),
            new ClassRosterReport(data),
            new StudentScheduleGridReport(data),
            new AssignmentAnalysisReport(data),
            new StudentScheduleOverviewReport(data),
        };

        return generators.Select(x => x.CreateDoc()).ToList();
    }

    public async Task<string[]> ChooseMultipleFilePaths(string title) {
        var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = true,
        });
        return files.Select(file => file.Path.LocalPath).ToArray();
    }

    private SchedulingRunAnalyticData SelectedRunInfo => RunGrid.SelectedItem as SchedulingRunAnalyticData;
    private void DataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedRunInfo == null) {
            ViewModel.RunDetails.Clear();
            RunNameTextBox.Text = string.Empty;
            RunNotesTextBox.Text = string.Empty;
            return;
        }

        ViewModel.ViewRunDetails(SelectedRunInfo.RunId);
        RunNameTextBox.Text = SelectedRunInfo.Name;
        RunNotesTextBox.Text = SelectedRunInfo.Notes;
    }

    private void SaveRunNotesButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (SelectedRunInfo == null)
            return;

        ViewModel.UpdateRunNameAndNotes(SelectedRunInfo.RunId, RunNameTextBox.Text, RunNotesTextBox.Text);
    }
}