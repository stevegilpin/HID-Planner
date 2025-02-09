using System;
using System.IO;
using HIDLib.HTML;
using HIDLib.IO;
using HIDLib.ReportGenerators;

namespace HIDUI.BusinessLogic;

public class CourseOptionsModel
{
    public string CourseInfoFilePath { get; private set; }
    public string DocumentHeader { get; set; }
    public string EventDescription { get; set; }

    public void SetFilePath(string filePath)
    {
        CourseInfoFilePath = ControllerUtils.GetMatchingPath([filePath], null, CsvReadUtil.HeaderMatchesCourseInfo);
    }

    public bool IsInputReady() => string.IsNullOrEmpty(CourseInfoFilePath) == false &&
                                  string.IsNullOrEmpty(DocumentHeader) == false &&
                                  string.IsNullOrEmpty(EventDescription) == false;

    public void SaveHtmlFile(string folder)
    {
        if (IsInputReady() == false)
            throw new Exception("Input is not ready");
        
        var courseData = CsvReadUtil.ReadCourseInfo(CourseInfoFilePath);
        var courseOptionsReport = new CourseOptionsReport(courseData, DocumentHeader, EventDescription);
        courseOptionsReport.CreateDoc().WriteToFile(folder);
    }

    public void SaveTextFile(string folder)
    {
        if (IsInputReady() == false)
            throw new Exception("Input is not ready");

        var courseData = CsvReadUtil.ReadCourseInfo(CourseInfoFilePath);
        var courseOptionsReport = new CourseOptionsReport(courseData, DocumentHeader, EventDescription);
        var fileContent = courseOptionsReport.CreateTextFileContent();
        File.WriteAllLines(Path.Combine(folder, "Course Options.txt"), [fileContent]);
    }
}