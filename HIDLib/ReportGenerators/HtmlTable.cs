using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HIDLib.ReportGenerators;

public class HtmlTable
{
    private HashSet<int> NumericColumns = new();
    private Dictionary<(int row, int col), XElement> Data = new();

    public HtmlTable(int[] numericColumns = null)
    {
        NumericColumns = (numericColumns ?? []).ToHashSet();
    }

    public XElement AddData(int row, int col, object content) => WriteCell(row, col, content, "td");
    public XElement AddHeader(int row, int col, object content) => WriteCell(row, col, content, "th");

    public List<XElement> AddColumnHeaders(object[] headers)
    {
        var elements = new List<XElement>();
        for (int i = 0; i < headers.Length; i++)
            elements.Add(AddHeader(0, i, headers[i]));

        return elements;
    }

    private XElement WriteCell(int row, int col, object content, string cellType)
    {
        var cell = new XElement(cellType, content);
        Data[(row, col)] = cell;
        if (NumericColumns.Contains(col))
            cell.Add(new XAttribute("class", HtmlUtil.CSS.NumericCellClass));

        return cell;
    }

    public XElement ToXmlNode()
    {
        var table = new XElement("table");
        var rows = Data.GroupBy(x => x.Key.row).OrderBy(g => g.Key);
        foreach (var rowData in rows) {
            var row = new XElement("tr");
            table.Add(row);
            foreach (var cell in rowData.OrderBy(x => x.Key.col))
                row.Add(cell.Value);
        }

        return table;
    }
}