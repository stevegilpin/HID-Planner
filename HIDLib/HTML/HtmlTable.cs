using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HIDLib.HTML;

internal class HtmlTable : XElement
{
    public List<HtmlRow> Data => Elements().OfType<HtmlRow>().ToList();
    
    private readonly XElement ColGroups = new("colgroup");
    public HtmlTable(List<object> headers = null) : base("table")
    {
        if (headers != null)
            AddHeaderRow(headers);
        Add(ColGroups);
    }

    public HtmlRow AddHeaderRow(List<object> headerData) => AddRow(headerData, true);
    public HtmlRow AddRowData(List<object> rowData) => AddRow(rowData, false);
    public void AddRow(List<HtmlCell> rowData) => Add(new HtmlRow(rowData));

    private HtmlRow AddRow(List<object> content, bool headerCells)
    {
        var cells = content.Select(x => new HtmlCell(x, headerCells)).ToList();
        var row = new HtmlRow(cells);
        Add(row);
        return row;
    }
    
    public HtmlCaption AddCaption(string text, HtmlCaption.TextAlign textAlign)
    {
        var caption = new HtmlCaption(text);
        caption.Add(new XAttribute("style", $"caption-side: bottom; text-align: {textAlign.ToString().ToLower()}; padding: 5px;"));
        return caption;
    }
    
    public XElement AddColGroup(int span, params (string name, string value)[] attributes)
    {
        var group = new XElement("col", new XAttribute("span", span));
        foreach (var attr in attributes)
            group.Add(new XAttribute(attr.name, attr.value));

        ColGroups.Add(group);
        return group;
    }
}

internal class HtmlRow : XElement
{
    public List<HtmlCell> Data => Elements().OfType<HtmlCell>().ToList();

    public HtmlRow(List<object> data, bool headerCell) : this(data.Select(x => new HtmlCell(x, headerCell)).ToList()) { }    
    public HtmlRow(List<HtmlCell> data) : base("tr")
    {
        foreach (var cell in data)
            Add(cell);
    }
}

internal class HtmlCell(object Content, bool HeaderCell = false) : XElement(HeaderCell ? "th" : "td", Content);

internal class HtmlCaption(string Text) : XElement("caption", Text)
{
    public enum TextAlign
    {
        Left,
        Center,
        Right
    }
}