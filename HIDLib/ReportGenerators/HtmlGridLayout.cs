using System.Linq;
using System.Xml.Linq;

namespace HIDLib.ReportGenerators;

public class HtmlGridLayout(int numColumns) : XElement("div", GetContainerStyle(numColumns))
{
    public void AddCell(params object[] cellContent)
    {
        var div = new XElement("div", cellContent);
        div.Add(GetCellStyle());
        Add(div);
    }

    private static XAttribute GetContainerStyle(int numColumns)
    {
        var colWidth = string.Join(' ', Enumerable.Repeat("auto", numColumns));
        var inlineStyle = $"display: grid; grid-template-columns: {colWidth};";
        return new XAttribute("style", inlineStyle);
    }

    private static XAttribute GetCellStyle()
    {
        var inlineStyle = $"padding: 10px;";
        return new XAttribute("style", inlineStyle);
    }
}