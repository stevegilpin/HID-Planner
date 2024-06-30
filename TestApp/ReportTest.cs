using System.IO;
using System.Linq;
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
        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        foreach (var name in sheetNames)
            InsertWorksheet(spreadsheetDocument, name);

        return spreadsheetDocument;
    }
    
    // https://learn.microsoft.com/en-us/office/open-xml/spreadsheet/how-to-insert-a-new-worksheet-into-a-spreadsheet?tabs=cs
    // Given a document name, inserts a new worksheet.
    private static Sheet InsertWorksheet(SpreadsheetDocument spreadSheet, string sheetName = null)
    {
        // Open the document for editing.
        var workbookPart = spreadSheet.WorkbookPart;
        if (workbookPart == null) {
            workbookPart = spreadSheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
        }

        // Add a blank WorksheetPart.
        WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        newWorksheetPart.Worksheet = new Worksheet(new SheetData());

        Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>() ??
                        workbookPart.Workbook.AppendChild(new Sheets());
        string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

        // Get a unique ID for the new worksheet.
        uint sheetId = 1;
        if (sheets.Elements<Sheet>().Count() > 0) {
            sheetId = (sheets.Elements<Sheet>().Select(s => s.SheetId?.Value).Max() + 1) ??
                      (uint)sheets.Elements<Sheet>().Count() + 1;
        }

        // Give the new worksheet a name.
        sheetName ??= "Sheet" + sheetId;

        // Append the new worksheet and associate it with the workbook.
        Sheet sheet = new Sheet { Id = relationshipId, SheetId = sheetId, Name = sheetName };
        sheets.Append(sheet);

        return sheet;
    }
}