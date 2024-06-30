using System;
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
        using var doc = CreateSpreadsheet(SpreadsheetTestFilePath);
        var sheet = InsertWorksheet(doc, "Foo");
        WriteToCell("Hello, world!", "A", 1, doc.WorkbookPart, sheet);
        sheet = InsertWorksheet(doc, "Bar");
        WriteToCell("Hello, world!", "B", 2, doc.WorkbookPart, sheet);
        doc.Save();
    }

    // https://learn.microsoft.com/en-us/office/open-xml/spreadsheet/how-to-create-a-spreadsheet-document-by-providing-a-file-name?tabs=cs-0%2Ccs-100%2Ccs-1%2Ccs#sample-code
    public static SpreadsheetDocument CreateSpreadsheet(string filePath)
    {
        // Create a spreadsheet document by supplying the filepath.
        // By default, AutoSave = true, Editable = true, and Type = xlsx.
        var spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
        WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        return spreadsheetDocument;
    }
    
    // https://learn.microsoft.com/en-us/office/open-xml/spreadsheet/how-to-insert-a-new-worksheet-into-a-spreadsheet?tabs=cs
    // Given a document name, inserts a new worksheet.
    private static WorksheetPart InsertWorksheet(SpreadsheetDocument spreadSheet, string sheetName = null)
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

        return newWorksheetPart;
    }

    // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
    // If the cell already exists, returns it. 
    private static Cell InsertCell(string columnName, uint rowIndex, WorksheetPart worksheetPart)
    {
        Worksheet worksheet = worksheetPart.Worksheet;
        SheetData? sheetData = worksheet.GetFirstChild<SheetData>();
        string cellReference = columnName + rowIndex;

        // If the worksheet does not contain a row with the specified row index, insert one.
        Row row;

        if (sheetData?.Elements<Row>().Where(r => r.RowIndex is not null && r.RowIndex == rowIndex).Count() != 0)
        {
            row = sheetData!.Elements<Row>().Where(r => r.RowIndex is not null && r.RowIndex == rowIndex).First();
        }
        else
        {
            row = new Row() { RowIndex = rowIndex };
            sheetData.Append(row);
        }

        // If there is not a cell with the specified column name, insert one.  
        if (row.Elements<Cell>().Where(c => c.CellReference is not null && c.CellReference.Value == columnName + rowIndex).Count() > 0)
        {
            return row.Elements<Cell>().Where(c => c.CellReference is not null && c.CellReference.Value == cellReference).First();
        }
        else
        {
            // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
            Cell? refCell = null;

            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference?.Value, cellReference, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            Cell newCell = new Cell() { CellReference = cellReference };
            row.InsertBefore(newCell, refCell);

            return newCell;
        }
    }

    private static void WriteToCell(string cellValue, string columnName, uint rowIndex, WorkbookPart workbookPart, WorksheetPart worksheetPart)
    {
        // Get the SharedStringTablePart. If it does not exist, create a new one.
        SharedStringTablePart shareStringPart;
        if (workbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            shareStringPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
        else
            shareStringPart = workbookPart.AddNewPart<SharedStringTablePart>();

        // Insert the text into the SharedStringTablePart.
        int index = InsertSharedStringItem(cellValue, shareStringPart);
        var cell = InsertCell(columnName, rowIndex, worksheetPart);
        cell.CellValue = new CellValue(index.ToString());
        cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
    }
    
    // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
    // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
    private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
    {
        // If the part does not contain a SharedStringTable, create one.
        if (shareStringPart.SharedStringTable is null)
        {
            shareStringPart.SharedStringTable = new SharedStringTable();
        }

        int i = 0;

        // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
        foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
        {
            if (item.InnerText == text)
            {
                return i;
            }

            i++;
        }

        // The text does not exist in the part. Create the SharedStringItem and return its index.
        shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
        shareStringPart.SharedStringTable.Save();

        return i;
    }
}