using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Data;
using Data.Objects;
using HIDLib;
using HIDLib.HTML;
using HIDLib.IO;
using HIDLib.ReportGenerators;
using HIDUI.ViewModels;

namespace HIDUI.Views {

    public partial class SchedulingView : ContentControl
    {

        private SchedulingViewModel ViewModel => DataContext as SchedulingViewModel;

        public SchedulingView()
        {
            InitializeComponent();
            // DataContext = new SchedulingViewModel();
        }

        public async void ImportFilesOnClick(object sender, RoutedEventArgs args) {
            var filePaths = await ChooseMultipleFilePaths("Select Import Files");
            ViewModel.TrySetFilePaths(filePaths, out string errorMessage);
        }

        public async void ChangeDefaultInputFolderOnClick(object sender, RoutedEventArgs args) {
            var defaultInputsFolder = await ChooseFolderPath("Change Default Input Folder Location");
            ViewModel.TrySetDefaultInputFolderLocation(defaultInputsFolder, out string errorMessage);
        }

        public async void TableRowViewReports(object sender, RoutedEventArgs args) {
            string runId = string.Empty;
            await Dispatcher.UIThread.InvokeAsync(() => { runId = ((Button)sender).Name; }, DispatcherPriority.Background);
            
            // todo store this data behind the scenes in Runs so we don't load it repeatedly
            var data = DataManager.LoadDataSet(Int32.Parse(runId));
            var docs = GetHtmlDocs(data);
        }

        public async void TableRowSaveData(object sender, RoutedEventArgs args) {
            string runId = string.Empty;
            await Dispatcher.UIThread.InvokeAsync(() => { runId = ((Button)sender).Name; }, DispatcherPriority.Background);

            // todo store this data behind the scenes in Runs so we don't load it repeatedly
            var data = DataManager.LoadDataSet(Int32.Parse(runId));
            var docs = GetHtmlDocs(data);

            var outputFolder = SettingsManager.GetDefaultOutputFolder();
            CsvWriteUtil.WriteResultsToCsv(data, outputFolder);
            foreach (var doc in docs)
                doc.WriteToFile(outputFolder);
            Console.WriteLine($"Reports written to: {outputFolder}");
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

        public async Task<string> ChooseOneFilePath(string title) {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = title,
                AllowMultiple = false,
            });
            var filesArray = files.Select(file => file.Path.LocalPath).ToArray();
            if (filesArray.Length == 0) {
                return null;
            }
            return filesArray[0];
        }

        public async Task<string> ChooseFolderPath(string title) {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                Title = title,
                AllowMultiple = false,
            });
            var filesArray = files.Select(file => file.Path.LocalPath).ToArray();
            if (filesArray.Length == 0) {
                return null;
            }
            return filesArray[0];
        }
    }

    public class RunModel {
        public string RunId { get; set; }
        public DateTime RunTimeStamp { get; set; }
        
        public RunModel(string runId , DateTime runTimeStamp)
        {
            RunId = runId;
            RunTimeStamp = runTimeStamp;
        }
    }

}