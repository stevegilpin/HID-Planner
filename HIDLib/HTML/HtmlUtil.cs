using System.IO;
using System.Xml.Linq;

namespace HIDLib.HTML;

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
    public static XElement PageBreak() => new("div", new XAttribute("class", "pagebreak"), string.Empty);
    public static XElement OrderedList() => new("ol", string.Empty);
    public static XElement ListItem(object itemContent) => new("li", itemContent);

    public static class CSS
    {
        public const string NumericCellClass = "numericCell";
        public static string PageBreak()
        {
            return """
                   @media print {
                       .pagebreak { page-break-after: always; }
                   }
                   """;
        }

        public static string BodyPadding()
        {
            return """
                   body {
                       padding: 20px;
                   }
                   """;
        }

        public static string TableStyles()
        {
            return
        @$"
        table {{
           width: 100%;
           table-layout: fixed;
        }}
        table, tr, td, th {{
           border-bottom: 1px solid black;
           border-collapse: collapse;
        }}
        td, th {{
           padding: 4px;
           text-align: left;
        }}
        tr .{NumericCellClass} {{
           text-align: center;
        }}
        tr:hover {{
           background-color: lightgray;
        }}
        ";
        }
    }
}