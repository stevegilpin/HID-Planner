using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HIDLib.ReportGenerators;

namespace HIDLib.HTML;

public class HtmlTable
{
    public List<RowData> Data = new();
    public List<XElement> ColGroups = new();
    public string CaptionAboveTable;
    public string CaptionBelowTable;

    public RowData AddColumnHeaders(List<object> headers) => AddRow(headers, "th");

    public XElement AddColGroup(int span, params (string name, string value)[] attributes)
    {
        var group = new XElement("col", new XAttribute("span", span));
        foreach (var attr in attributes)
            group.Add(new XAttribute(attr.name, attr.value));

        ColGroups.Add(group);
        return group;
    }
    public RowData AddRowData(List<object> rowData) => AddRow(rowData, "td");
    public void AddRow(List<XElement> rowData) => Data.Add(new RowData(rowData));

    private RowData AddRow(List<object> content, string cellType)
    {
        var cells = content.Select(x => new XElement(cellType, x)).ToList();
        var row = new RowData(cells);
        Data.Add(row);
        return row;
    }

    public XElement ToXmlNode()
    {
        var table = new XElement("table");

        if (string.IsNullOrEmpty(CaptionAboveTable) == false)
            table.Add(new XElement("caption", CaptionAboveTable));
        if (string.IsNullOrEmpty(CaptionBelowTable) == false)
            table.Add(new XElement("caption", CaptionBelowTable, new XAttribute("style", "caption-side: bottom; text-align: right; padding: 5px;")));

        if (ColGroups.Count != 0)
            table.Add(new XElement("colgroup", ColGroups));

        foreach (var row in Data)
            table.Add(row.RowContainer);

        return table;
    }
}

public class RowData
{
    public XElement RowContainer { get; init; }
    public List<XElement> Data { get; init; }

    public RowData(List<object> data, string cellType = "td") : this(data.Select(x => new XElement(cellType, x)).ToList()) { }    
    public RowData(List<XElement> data)
    {
        Data = data.ToList();
        RowContainer = new XElement("tr", Data);
    }
}