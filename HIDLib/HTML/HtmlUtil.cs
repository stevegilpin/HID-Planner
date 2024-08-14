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
}

public class HtmlHeader(object content, int level = 1) : XElement($"h{level}", content);
public class HtmlBreak() : XElement("br");
public class HtmlPageBreak() : XElement("div", new XAttribute("class", "pagebreak"), string.Empty);
public class HtmlOrderedList() : XElement("ol", string.Empty);
public class HtmlListItem(object itemContent) : XElement("li", itemContent);

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
        tr:hover {{
           background-color: lightgray;
        }}
        ";
    }
}