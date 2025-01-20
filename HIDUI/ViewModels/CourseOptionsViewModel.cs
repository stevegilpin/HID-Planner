using System;
using System.IO;
using HIDLib.HTML;
using HIDLib.IO;
using HIDLib.ReportGenerators;
using HIDUI.BusinessLogic;

namespace HIDUI.ViewModels;

public class CourseOptionsViewModel : ViewModelBase
{
    private readonly CourseOptionsModel Model = new();
    
    public bool IsInputReady => Model.IsInputReady();
    public string FilePath => Model.CourseInfoFilePath;

    public void SetCoursesFile(string filePath)
    {
        Model.SetFilePath(filePath);
        OnPropertyChanged(nameof(IsInputReady));
        OnPropertyChanged(nameof(FilePath));
    }
    
    public void RemoveCoursesFile() => SetCoursesFile(null);

    public void SetDocumentHeader(string text)
    {
        Model.DocumentHeader = text;
        OnPropertyChanged(nameof(IsInputReady));
    }

    public void SetEventDescription(string text)
    {
        Model.EventDescription = text;
        OnPropertyChanged(nameof(IsInputReady));
    }

    public void SaveHtmlFile(string folder) => Model.SaveHtmlFile(folder);
    public void SaveTextFile(string folder) => Model.SaveTextFile(folder);
}