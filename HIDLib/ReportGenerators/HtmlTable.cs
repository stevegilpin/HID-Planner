using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HIDLib.ReportGenerators;

public class HtmlTable
{
    private HashSet<int> NumericColumns = new();
    private List<List<XElement>> Data = new();
    private List<XElement> ColGroups = new();
    public string CaptionAboveTable;
    public string CaptionBelowTable;

    public HtmlTable(int[] numericColumns = null)
    {
        NumericColumns = (numericColumns ?? []).ToHashSet();
    }

    public List<XElement> AddColumnHeaders(List<object> headers) => AddRow(headers, "th");

    public XElement AddColGroup(int span, params (string name, string value)[] attributes)
    {
        var group = new XElement("col", new XAttribute("span", span));
        foreach (var attr in attributes)
            group.Add(new XAttribute(attr.name, attr.value));

        ColGroups.Add(group);
        return group;
    }
    public List<XElement> AddRowData(List<object> rowData) => AddRow(rowData, "td");
    public void AddRow(List<XElement> rowData)
    {
        Data.Add(rowData);
    }

    private List<XElement> AddRow(List<object> content, string cellType)
    {
        var row = new List<XElement>();
        Data.Add(row);
        var elements = new List<XElement>();
        for (int i = 0; i < content.Count; i++) {
            var cell = new XElement(cellType, content[i]);
            row.Add(cell);
            if (NumericColumns.Contains(i))
                cell.Add(new XAttribute("class", HtmlUtil.CSS.NumericCellClass));
        }

        return elements;
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

        foreach (var rowData in Data) {
            var row = new XElement("tr");
            table.Add(row);
            foreach (var cell in rowData)
                row.Add(cell);
        }

        return table;
    }
}