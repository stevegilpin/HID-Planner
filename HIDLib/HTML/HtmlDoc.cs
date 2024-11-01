using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace HIDLib.HTML;

public sealed class HtmlDoc
{
    internal XElement Head { get; private set; }
    internal List<string> StyleRuleSets { get; private set; }
    internal XElement Body { get; private set; }
    public string Title { get; init; }

    public HtmlDoc(string docTitle)
    {
        Head = new XElement("head");
        Body = new XElement("body");
        StyleRuleSets = new List<string>();
        Title = docTitle;
        AddToHead(new XElement("meta", new XAttribute("charset", "UTF-8")));
        AddToHead(new XElement("title", docTitle));
    }

    internal T Add<T>(T element) where T : XElement
    {
        Body.Add(element);
        return element;
    }
    internal void AddToHead(XElement element) => Head.Add(element);

    internal void AddCssRuleSet(string selector, List<string> styleRules)
    {
        string ruleset =
            $@"
{selector} {{
    {string.Join($";{Environment.NewLine}", styleRules)}
}}
";
        StyleRuleSets.Add(ruleset);
    }

    public override string ToString()
    {
        Head.Add(new XElement("style", string.Join(Environment.NewLine, StyleRuleSets)));

        var doc = new XDocument(
            new XDocumentType("html", null, null, null),
            new XElement("html",
                new XAttribute("lang", "en"),
                Head,
                Body
            ));

        return doc.ToString();
    }
}