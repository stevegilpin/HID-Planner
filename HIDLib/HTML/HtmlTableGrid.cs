using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HIDLib.HTML;

internal class HtmlTableGrid() : XElement("table", new XAttribute("class", GRID_CLASS_NAME))
{
    private const string GRID_CLASS_NAME = "grid-container";
    private const string DATA_CLASS_NAME = "grid-data";

    public static string Css() =>
        @$"
    @media print {{
        .{DATA_CLASS_NAME} {{
          break-inside: avoid;
        }}
    }}

    .{DATA_CLASS_NAME} {{
        padding: 10px;
    }}
    
    .{GRID_CLASS_NAME} {{
        border: hidden;
    }}

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
    .{DATA_CLASS_NAME} tr:hover {{
        background-color: lightgray;
    }}
    ";

    public RowData AddRow(List<XElement> rowData)
    {
        rowData.ForEach(x => x.AddAttribute("class", DATA_CLASS_NAME));
        var row = new RowData(rowData.Cast<object>().ToList());
        row.RowContainer.AddAttribute("class", GRID_CLASS_NAME);
        Add(row.RowContainer);
        return row;
    }
}