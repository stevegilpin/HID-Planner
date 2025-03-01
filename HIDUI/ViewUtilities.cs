using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HIDLib;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace HIDUI;

internal static class ViewUtilities
{
    private static async Task<IStorageFolder> GetStartLocation(IStorageProvider storageProvider, bool inputFolder)
    {
        var start = inputFolder ? SettingsManager.GetDefaultInputsFolder() : SettingsManager.GetDefaultOutputFolder();
        var startPath = await storageProvider.TryGetFolderFromPathAsync(start);
        if (Directory.Exists(start) == false)
            startPath = null;

        return startPath;
    }
    
    public static async Task<string[]> ChooseMultipleFilePathsAsync(this Control sender, string title) {
        var storageProvider = TopLevel.GetTopLevel(sender)!.StorageProvider;
        var startPath = await GetStartLocation(storageProvider, true);
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = true,
            SuggestedStartLocation = startPath,
        });
        return files.Select(file => file.Path.LocalPath).ToArray();
    }
    
    public static async Task<string> ChooseOneFilePathAsync(this Control sender, string title) {
        var storageProvider = TopLevel.GetTopLevel(sender)!.StorageProvider;
        var startPath = await GetStartLocation(storageProvider, true);
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = startPath,
        });
        var filesArray = files.Select(file => file.Path.LocalPath).ToArray();
        return filesArray.Length == 0 ? null : filesArray[0];
    }

    public static async Task<string> ChooseFolderPathAsync(this Control sender, string title, bool chooseInput) {
        var storageProvider = TopLevel.GetTopLevel(sender)!.StorageProvider;
        var startPath = await GetStartLocation(storageProvider, chooseInput);
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = startPath,
        });
        var folderPaths = folders.Select(x => x.Path.LocalPath).ToArray();
        return folderPaths.Length == 0 ? null : folderPaths[0];
    }

    public static void OpenFolderInExplorer(string folderPath)
    {
        // open file explorer at output folder
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", folderPath);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", $"-R \"{folderPath}\"");
    }

    public static void NotifyException(this Control control, Exception exception, bool requireClose)
    {
        var nl = Environment.NewLine;
        var msg = requireClose ?
            $"The application hit an unexpected error and will exit now.{nl}{nl}Error details:{nl}{exception}" :
            $"The application hit an unexpected error. It is recommended to restart the application.{nl}{nl}Error details:{nl}{exception}";
        
        var box = MessageBoxManager.GetMessageBoxStandard("Error!", msg);
        box.ShowWindowDialogAsync((Window)control.GetVisualRoot())
            .ContinueWith(_ => {
                if (requireClose)
                    Environment.Exit(-1);
            });
    }

    public static T CheckTaskException<T>(this Control control, Task<T> task, bool requireClose)
    {
        if (task.Exception != null)
            Dispatcher.UIThread.Invoke(() => control.NotifyException(task.Exception, requireClose));
        return task.Result;
    }

    public static void CheckTaskException(this Control control, Task task, bool requireClose)
    {
        if (task.Exception != null)
            Dispatcher.UIThread.Invoke(() => control.NotifyException(task.Exception, requireClose));
    }
}