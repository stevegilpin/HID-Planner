using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HIDLib;
using HIDUI.ViewModels;

namespace HIDUI.Views
{
    public partial class SurveyView : ContentControl
    {
        private SurveyViewModel ViewModel => DataContext as SurveyViewModel;
        
        public SurveyView()
        {
            InitializeComponent();
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

            // open file explorer at output folder
            if (OperatingSystem.IsWindows())
                Process.Start("explorer.exe", folder);
            else if (OperatingSystem.IsMacOS())
                Process.Start("open", $"-R \"{folder}\"");
        }

        private void ReconcileFilesOnClick(object sender, RoutedEventArgs e) => ViewModel.ReconcileFiles();
    }
}
