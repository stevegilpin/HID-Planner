using System;
using System.IO;

namespace HIDUI.ViewModels;

public class FileSelectionViewModel : ViewModelBase
{
    public string FilePath { get; private set; }
    public string StatusColor => IsFileSelected ? "Green" : "Gray"; // todo manage from View
    public bool IsFileSelected => string.IsNullOrEmpty(FilePath) == false;
    public string FileName => IsFileSelected ? Path.GetFileName(FilePath) : string.Empty;

    public void RemoveFileSelection() => SetFilePath(null);

    public void SetFilePath(string filePath)
    {
        FilePath = filePath;
        OnPropertyChanged(nameof(IsFileSelected));
        OnPropertyChanged(nameof(FileName));
    }
}