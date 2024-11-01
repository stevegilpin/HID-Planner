using Data.Objects;
using HIDLib.HTML;

namespace HIDLib.ReportGenerators;

public interface IHtmlGenerator
{
    HtmlDoc CreateDoc();
}