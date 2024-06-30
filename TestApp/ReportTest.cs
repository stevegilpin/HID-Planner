using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TestApp;

public static class ReportTest
{
    public static readonly string SpreadsheetTestFilePath = Path.Combine(DataSimulator.SampleDataFolder, "SpreadsheetTest.xlsx");

    public static void RunTest()
    {
        using var doc = CreateSpreadsheet(SpreadsheetTestFilePath, ["Foo", "Bar"]);
    }

    // https://learn.microsoft.com/en-us/office/open-xml/spreadsheet/how-to-create-a-spreadsheet-document-by-providing-a-file-name?tabs=cs-0%2Ccs-100%2Ccs-1%2Ccs#sample-code
    public static SpreadsheetDocument CreateSpreadsheet(string filePath, string[] sheetNames)
    {
        // Create a spreadsheet document by supplying the filepath.
        // By default, AutoSave = true, Editable = true, and Type = xlsx.
        var spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);

        // Add a WorkbookPart to the document.
        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        // Add a WorksheetPart to the WorkbookPart.
        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        // Add Sheets to the Workbook.
        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        for (int i = 0; i < sheetNames.Length; i++) {
            // Append a new worksheet and associate it with the workbook.
            var id = (uint)i + 1;
            Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = id, Name = sheetNames[i] };
            sheets.Append(sheet);
        }

        return spreadsheetDocument;
    }
}