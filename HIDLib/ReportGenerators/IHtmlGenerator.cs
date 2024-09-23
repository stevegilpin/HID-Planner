using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

internal interface IHtmlGenerator
{
    HtmlDoc CreateDoc();
}