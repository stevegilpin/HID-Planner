// See https://aka.ms/new-console-template for more information

using Data;
using HIDLib;
using HIDLib.HTML;
using HIDLib.IO;
using HIDLib.ReportGenerators;
using HIDUI;

var inputFolder = SettingsManager.GetDefaultInputsFolder();
var path = Path.Combine(inputFolder, "Supervisors.csv");
var latestRun = DataManager.LatestRun().RunId;
ConfirmInputIsReady(path, latestRun);

var supervisors = CsvReadUtil.ReadSupervisorInfo(path);
var reportData = DataManager.LoadDataSet(latestRun);

var outputFolder = SettingsManager.GetDefaultOutputFolder();
new SupervisorStudentSchedulesReport(reportData, supervisors).CreateDoc().WriteToFile(outputFolder);
Console.WriteLine($"Report saved to {outputFolder}");

return;

static void ConfirmInputIsReady(string path, int schedulingId)
{
    Console.WriteLine("The supervisor file will be read from this path:");
    Console.WriteLine(path);
    Console.WriteLine();
    Console.WriteLine("File description:");
    Console.WriteLine(ViewModelUtilities.GetSupervisorsCsvInfoText());
    Console.WriteLine();
    Console.WriteLine($"A report will be produced matching the supervisor information against student schedules from run ID {schedulingId}");
    
    Console.WriteLine("Continue (y/n)?");
    var response = Console.ReadLine() ?? string.Empty;
    if (response.ToLower() == "n")
        Environment.Exit(0);
    if (response.ToLower() != "y") {
        Console.WriteLine($"Unrecognized response: {response}");
        Environment.Exit(1);
    }
}