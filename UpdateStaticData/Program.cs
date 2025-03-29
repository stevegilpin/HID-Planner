using Data;
using HIDLib;
using HIDLib.IO;
using HIDUI;

var inputFolder = SettingsManager.GetDefaultInputsFolder();
var roomsPath = Path.Combine(inputFolder, "Rooms.csv");
var supervisorsPath = Path.Combine(inputFolder, "Supervisors.csv");
var instructorsPath = Path.Combine(inputFolder, "Instructors.csv");
ConfirmInputIsReady(roomsPath, supervisorsPath, instructorsPath);

if (File.Exists(supervisorsPath)) {
    var rooms = CsvReadUtil.ReadRoomDescriptions(roomsPath);
    DataManager.UpdateRooms(rooms);
    Console.WriteLine($"Inserted {rooms.Count} rooms");
}
else {
    Console.WriteLine("The room descriptions file could not be found. Skipping.");
}

if (File.Exists(supervisorsPath)) {
    var supervisors = CsvReadUtil.ReadSupervisorInfo(supervisorsPath);
    DataManager.UpdateSupervisors(supervisors);
    Console.WriteLine($"Inserted {supervisors.Count} supervisors");
}
else {
    Console.WriteLine("The supervisors file could not be found. Skipping.");
}

if (File.Exists(instructorsPath)) {
    var instructors = CsvReadUtil.ReadInstructorInfo(instructorsPath);
    DataManager.UpdateInstructors(instructors);
    Console.WriteLine($"Inserted {instructors.Count} instructors");
}
else {
    Console.WriteLine("The instructors file could not be found. Skipping.");
}

return;

static void ConfirmInputIsReady(string roomsFilePath, string supervisorsFilePath, string instructorsFilePath)
{
    Console.WriteLine("The room descriptions file will be read from this path if it exists:");
    Console.WriteLine(roomsFilePath);
    Console.WriteLine();
    Console.WriteLine("File description:");
    Console.WriteLine(ViewModelUtilities.GetRoomsDescriptionCsvInfoText());
    Console.WriteLine();

    Console.WriteLine("The supervisors file will be read from this path:");
    Console.WriteLine(supervisorsFilePath);
    Console.WriteLine();
    Console.WriteLine("File description:");
    Console.WriteLine(ViewModelUtilities.GetSupervisorsCsvInfoText());
    Console.WriteLine();

    Console.WriteLine("The instructors file will be read from this path:");
    Console.WriteLine(instructorsFilePath);
    Console.WriteLine();
    Console.WriteLine("File description:");
    Console.WriteLine(ViewModelUtilities.GetInstructorsCsvInfoText());
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