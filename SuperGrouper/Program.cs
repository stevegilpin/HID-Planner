// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Objects;
using HIDLib;
using HIDLib.IO;
using HIDUI;
using SuperGrouper;

var inputFolder = SettingsManager.GetDefaultInputsFolder();
var inputFilePath = Path.Combine(inputFolder, "InstructorSupervisorAssignments.csv");
ConfirmInputIsReady(inputFilePath);

var rawData = CsvReadUtil.ReadFile<SupervisorScheduleData, SupervisorScheduleDataMapper>(inputFilePath);

var courseSchedule = new List<CourseSchedule>();

var courseGroups = rawData.GroupBy(x => new { x.CourseCode, x.SessionId }).ToList();
foreach (var group in courseGroups) {
    var supervisors = group.Select(x => x.SupervisorId).Where(x => string.IsNullOrWhiteSpace(x) == false).Distinct().ToList();
    var instructor = group.Select(x => x.InstructorId).Where(x => string.IsNullOrWhiteSpace(x) == false).Distinct().ToList();
    if (instructor.Count > 1)
        throw new Exception("Program requires no more than one instructor");

    courseSchedule.Add(new CourseSchedule {
        CourseCode = group.Key.CourseCode,
        SessionId = group.Key.SessionId,
        Instructor = instructor.Count == 0 ? null : instructor[0],
        Supervisor = supervisors.Count == 0 ? null : string.Join(';', supervisors),
    });
}

var outputFolder = SettingsManager.GetDefaultOutputFolder();
CsvWriteUtil.WriteCourseSchedule(courseSchedule, outputFolder);
Console.WriteLine($"Report saved to {outputFolder}");

return;


static void ConfirmInputIsReady(string filePath)
{
    Console.WriteLine("The schedule file will be read from this path:");
    Console.WriteLine(filePath);
    Console.WriteLine();
    Console.WriteLine("File description:");
    Console.WriteLine(ViewModelUtilities.GetCsvInfoText<SupervisorScheduleData, SupervisorScheduleDataMapper>(filePath));
    Console.WriteLine();

    Console.WriteLine("Continue (y/n)?");
    var response = Console.ReadLine() ?? string.Empty;
    if (response.ToLower() == "n")
        Environment.Exit(0);
    if (response.ToLower() != "y") {
        Console.WriteLine($"Unrecognized response: {response}");
        Environment.Exit(1);
    }
}