using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace HIDUI;

internal static class ViewUtilities
{
    public static async Task<string[]> ChooseMultipleFilePathsAsync(this Visual sender, string title) {
        var files = await TopLevel.GetTopLevel(sender).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = true,
        });
        return files.Select(file => file.Path.LocalPath).ToArray();
    }
    
    public static async Task<string> ChooseOneFilePathAsync(this Visual sender, string title) {
        var files = await TopLevel.GetTopLevel(sender).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
        });
        var filesArray = files.Select(file => file.Path.LocalPath).ToArray();
        if (filesArray.Length == 0)
            return null;
        else
            return filesArray[0];
    }

    public static async Task<string> ChooseFolderPathAsync(this Visual sender, string title) {
        var folder = await TopLevel.GetTopLevel(sender).StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
            Title = title,
            AllowMultiple = false,
        });
        var folderPaths = folder.Select(folder => folder.Path.LocalPath).ToArray();
        if (folderPaths.Length == 0)
            return null;
        else
            return folderPaths[0];
    }

    public static void OpenFolderInExplorer(string folderPath)
    {
        // open file explorer at output folder
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", folderPath);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", $"-R \"{folderPath}\"");
    }
}