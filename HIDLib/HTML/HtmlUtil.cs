using System.IO;
using System.Xml.Linq;

namespace HIDLib.HTML;

public static class HtmlUtil
{
    public static void WriteToFile(this HtmlDoc content, string folder)
    {
        var filePath = Path.Combine(folder, $"{content.Title}.html");
        Directory.CreateDirectory(folder);
        using var f = new StreamWriter(filePath);
        f.Write(content.ToString());
    }

    internal static void AddAttribute(this XElement element, string name, string value)
    {
        var attr = element.Attribute(name);
        if (attr == null)
            element.Add(new XAttribute(name, value));
        else
            attr.Value += $" {value};";
    }

    internal static void AddStyle(this XElement element, string value) => element.AddAttribute("style", value);
}

internal class HtmlHeader(object content, int level = 1) : XElement($"h{level}", content);
internal class HtmlBreak() : XElement("br");
internal class HtmlPageBreak() : XElement("div", new XAttribute("class", "pagebreak"), string.Empty);
internal class HtmlOrderedList() : XElement("ol", string.Empty);

internal class HtmlList() : XElement("ul", string.Empty)
{
    public HtmlListItem AddItem(object itemContent)
    {
        var item = new HtmlListItem(itemContent);
        Add(item);
        return item;
    }
}
internal class HtmlListItem(object itemContent) : XElement("li", itemContent);
internal class HtmlParagraph(string text) : XElement("p", text);

internal static class CSS
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

    public static string TableStyles(string dataAlign = "left", string headerAlign = "left")
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
        td {{
           padding: 4px;
           text-align: {dataAlign};
        }}
        th {{
           padding: 4px;
           text-align: {headerAlign};
        }}
        tr:hover {{
           background-color: lightgray;
        }}
        ";
    }

    public static string BodyBackgroundColorInPrint()
    {
        return
            """
            body{
              -webkit-print-color-adjust:exact !important;
              print-color-adjust:exact !important;
            }
            """;
    }

    public static string EllipsisOverflow()
    {
        return
            """
            * {
                text-overflow: ellipsis;
                overflow: scroll;
                white-space: nowrap;
            }
            """;
    }

    public static string GlobalFontSize(int size)
    {
        return
            @$"
            * {{
                font-size: {size}px;
            }}
            ";
    }
}