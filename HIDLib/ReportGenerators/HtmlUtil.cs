using System;
using System.IO;
using System.Xml.Linq;

namespace HIDLib.ReportGenerators;

public static class HtmlUtil
{
    public static void WriteToFile(this HtmlDoc content, string folder)
    {
        var filePath = Path.Combine(folder, $"{content.Title}.html");
        using var f = new StreamWriter(filePath);
        f.Write(content.ToString());
    }

    public static XElement Header(object content, int level = 1) => new($"h{level}", content);
    public static XElement Break() => new("br");
    public static XElement PageBreak() => new("div", new XAttribute("class", "pagebreak"));
    public static XElement List() => new("ul");
    public static XElement ListItem(object itemContent) => new("li", itemContent);

    public static class CSS
    {
        public static string PageBreak()
        {
            return """
                   @media print {
                       .pagebreak { page-break-after: always; }
                   }
                   """;
        }
    }
}