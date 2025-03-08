// See https://aka.ms/new-console-template for more information

using Data;
using HIDLib;
using HIDLib.IO;
using HIDUI;

var inputFolder = SettingsManager.GetDefaultInputsFolder();
var path = Path.Combine(inputFolder, "Rooms.csv");
ConfirmInputIsReady(path);

var rooms = CsvReadUtil.ReadRoomDescriptions(path);
DataManager.UpdateRooms(rooms);
Console.WriteLine($"Inserted {rooms.Count} rooms");

return;

static void ConfirmInputIsReady(string path)
{
    
    Console.WriteLine("The room descriptions file will be read from this path:");
    Console.WriteLine(path);
    Console.WriteLine();
    Console.WriteLine("File description:");
    Console.WriteLine(ViewModelUtilities.GetRoomsDescriptionCsvInfoText());
    Console.WriteLine("Continue (y/n)?");
    var response = Console.ReadLine() ?? string.Empty;
    if (response.ToLower() == "n")
        Environment.Exit(0);
    if (response.ToLower() != "y") {
        Console.WriteLine($"Unrecognized response: {response}");
        Environment.Exit(1);
    }
}