using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace HIDLib.HTML;

internal sealed class HtmlDoc
{
    public XElement Head { get; private set; }
    public List<string> StyleRuleSets { get; private set; }
    public XElement Body { get; private set; }
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

    public T Add<T>(T element) where T : XElement
    {
        Body.Add(element);
        return element;
    }
    public void AddToHead(XElement element) => Head.Add(element);

    public void AddCssRuleSet(string selector, List<string> styleRules)
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