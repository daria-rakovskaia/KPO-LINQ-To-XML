using System.Xml;
using System.Xml.Linq;

Task5("myTxt.txt", "task5.xml");
Task15("task15.xml");
Task25("task15.xml", "task25.xml");
Task35("task35.xml", "task35Outp.xml");
Task45("task15.xml", "task45.xml");
Task55("task55_inp.xml", "task55_outp.xml");
Task65("task65_inp.xml", "task65_outp.xml");
Task75("task75_inp.xml", "task75_outp.xml");
Task85("task85_inp.xml", "task85_outp.xml");
Task61("task61_inp.xml", "task61_outp.xml");

// 5
void Task5(string txtFile, string xmlFile)
{
    string[] txtLines = File.ReadAllLines(txtFile);

    var root = new XElement("root");
    var xmlDocument = new XDocument(new XElement("root",
        txtLines.Select((txtLine, lineIndex) =>
            new XElement("line", new XAttribute("num", (lineIndex + 1)), txtLine.Split(' ')
                    .Select((txtWord, wordIndex) =>
                        new XElement("word", new XAttribute("num", (wordIndex + 1)), txtWord))))));
    xmlDocument.Save(xmlFile);
}

// 15
void Task15(string xmlFile)
{
    var xmlDocument = XDocument.Load(xmlFile);
    var elements = xmlDocument.Root.Elements();
    var names = elements.Where(elem1 => elem1.Elements().Count(elem2 => elem2.Attributes().Count() >= 2) > 0)
                        .Select(elem1 => elem1.Name.LocalName + " " + elem1.Elements().Count(elem2 => elem2.Attributes().Count() >= 2))
                        .OrderBy(name => name);
    foreach (var name in names)
        Console.WriteLine(name);
}

// 25
void Task25(string xmlFile, string xmlFile1)
{
    var xmlDocument = XDocument.Load(xmlFile);
    xmlDocument.Root.Descendants().Where(elem => (elem.Parent == xmlDocument.Root
        || elem.Parent.Parent == xmlDocument.Root) && elem.Attributes().Count() > 1)
        .ToList().ForEach(elem => elem.Attributes().Remove());
    xmlDocument.Save(xmlFile1);
}

// 35
void Task35(string xmlFile, string xmlOutput)
{
    var xmlDocument = XDocument.Load(xmlFile);
    xmlDocument.Root.Elements().Elements().ToList()
        .ForEach(elem2 => elem2.SetAttributeValue("child-count", elem2.Elements().Count()));
    xmlDocument.Save(xmlOutput);
}

// 45
void Task45(string xmlFile, string xmlFile1)
{
    var xmlDocument = XDocument.Load(xmlFile);
    xmlDocument.Descendants().Where(elem => elem.Attributes().Count() > 0).ToList()
        .ForEach(elem => elem.AddFirst(new XElement("odd-attr-count", elem.DescendantsAndSelf().Attributes().Count() % 2 != 0)));
    xmlDocument.Save(xmlFile1);
}

// 55
void Task55(string xmlFile, string xmlOutput)
{
    var xmlDocument = XDocument.Load(xmlFile);
    xmlDocument.Root.Elements().Elements().ToList().ForEach(elem => elem.Name = elem.Name.LocalName);
    xmlDocument.Save(xmlOutput);
}

// 65
void Task65(string xmlFile, string xmlOutput)
{
    var xmlDocument = XDocument.Load(xmlFile);

    var elements = from client in xmlDocument.Root.Elements()
                   let date = DateTime.Parse(client.Element("info").Element("date").Value)
                   let time = XmlConvert.ToTimeSpan(client.Element("info").Element("time").Value)
                   group time.TotalMinutes by new { Year = date.Year, Id = client.Name.LocalName.Substring(2) } into element
                   orderby element.Key.Year ascending, element.Key.Id ascending
                   select new { Year = element.Key.Year, Id = element.Key.Id, TotalTime = element.Sum() };

    var result = new XDocument(new XElement("root", elements.GroupBy(element => element.Year, 
        (year, nodes) =>
                    new XElement("year", new XAttribute("value", year), nodes.Select(node =>
                        new XElement("total-time", new XAttribute("id", node.Id), node.TotalTime))))));

    result.Save(xmlOutput);
}

// 75
void Task75(string xmlFile, string xmlOutput)
{
    var xmlDocument = XDocument.Load(xmlFile);

    var result = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
    var root = new XElement("root");
    result.Add(root);

    result.Root.ReplaceNodes(xmlDocument.Root.Elements()
       .GroupBy(s => s.Name.LocalName.Split('_')[1])
       .Select(s => new XElement(s.Key, s.ToList()
            .GroupBy(s => s.Element("brand").Value)
            .Select(s => new XElement("brand" + s.Key,
                new XAttribute("station-count", s.Count()),
                (s.Elements().Select(s => int.Parse(s.Value)).Sum() - int.Parse(s.Key) * s.Count()) / s.Count())))));

    result.Root.Elements().ToList().ForEach(node =>
    {
        var brands = node.Elements().Select(s => s.Name).ToList();
        if (!brands.Contains("brand98"))
            node.Add(new XElement("brand98", new XAttribute("station-count", 0), 0));
        if (!brands.Contains("brand95"))
            node.Add(new XElement("brand95", new XAttribute("station-count", 0), 0));
        if (!brands.Contains("brand92"))
            node.Add(new XElement("brand92", new XAttribute("station-count", 0), 0));
    });

    result.Root.ReplaceNodes(result.Root.Elements().OrderBy(x => x.Name.LocalName));
    result.Root.Elements().ToList().ForEach(node => node.ReplaceNodes(node.Elements().OrderByDescending(x => x.Name.LocalName.Split("brand")[1])));

    result.Save(xmlOutput);
}

// 85
void Task85(string xmlFile, string xmlOutput)
{
    var xmlDocument = XDocument.Load(xmlFile);

    var result = xmlDocument.Descendants("info")
                .OrderBy(info => (int)info.Attribute("class"))
                .ThenBy(info => (string)info.Attribute("name"))
                .ThenBy(info => (string)info.Attribute("subject"))
                .ThenByDescending(info => (int)info.Attribute("mark"))
                .GroupBy(info => (int)info.Attribute("class"))
                .OrderBy(classGroup => classGroup.Key)
                .Select(classGroup => new XElement("class", new XAttribute("number", classGroup.Key),
                    classGroup.GroupBy(pupilGroup => (string)pupilGroup.Attribute("name"))
                    .OrderBy(pupilGroup => pupilGroup.Key)
                    .Select(pupilGroup => new XElement("pupil", new XAttribute("name", pupilGroup.Key),
                        pupilGroup.GroupBy(subjectGroup => (string)subjectGroup.Attribute("subject"))
                            .OrderBy(subjectGroup => subjectGroup.Key)
                            .Select(subjectGroup => new XElement("subject", new XAttribute("name", subjectGroup.Key),
                                    subjectGroup.Select(info => new XElement("mark", (int)info.Attribute("mark")))))))));

    var resultDocument = new XDocument(new XDeclaration("1.0", "utf-8", null));
    var root = new XElement("root");
    resultDocument.Add(root);
    resultDocument.Root.ReplaceNodes(result);

    resultDocument.Save(xmlOutput);
}

// 61
void Task61(string xmlFile, string xmlOutput)
{
    var xmlDocument = XElement.Load(xmlFile);
    xmlDocument.Elements()
    .ToList()
    .ForEach(item =>
    {
        DateTime date = DateTime.Parse(item.Element("date").Value);
        var id = item.Element("id").Value;
        var time = item.Element("time").Value;
        item.Add(new XAttribute("id", id));
        item.Add(new XAttribute("year", date.Year));
        item.Add(new XAttribute("month", date.Month));
        item.Name = "time";
        item.Value = time;
    });
    new XDocument(xmlDocument).Save(xmlOutput);
}