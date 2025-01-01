using System;
using System.IO;
using HIDLib.HTML;
using HIDLib.IO;
using HIDLib.ReportGenerators;

namespace HIDUI.BusinessLogic;

public class CourseOptionsController
{
    public string CourseInfoFilePath { get; private set; }
    public string DocumentHeader { get; set; }
    public string EventDescription { get; set; }

    public void SetFilePath(string filePath)
    {
        CourseInfoFilePath = ControllerUtils.GetMatchingPath([filePath], CourseInfoFilePath, CsvReadUtil.HeaderMatchesCourseInfo);
    }

    public bool IsInputReady() => string.IsNullOrEmpty(CourseInfoFilePath) ||
                                  string.IsNullOrEmpty(DocumentHeader) ||
                                  string.IsNullOrEmpty(EventDescription);

    public void SaveHtmlFile(string folder)
    {
        if (IsInputReady() == false)
            return;
        
        var courseData = CsvReadUtil.ReadCourseInfo(CourseInfoFilePath);
        var courseOptionsReport = new CourseOptionsReport(courseData, DocumentHeader, EventDescription);
        courseOptionsReport.CreateDoc().WriteToFile(folder);
    }

    public void SaveTextFile(string folder)
    {
        if (IsInputReady() == false)
            return;

        var courseData = CsvReadUtil.ReadCourseInfo(CourseInfoFilePath);
        var courseOptionsReport = new CourseOptionsReport(courseData, DocumentHeader, EventDescription);
        var fileContent = courseOptionsReport.CreateTextFileContent();
        File.WriteAllLines(Path.Combine(folder, "Course Options.txt"), [fileContent]);
    }
}