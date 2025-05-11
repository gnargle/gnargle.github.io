// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using ParticleGen;
using System.Text.Json;

//we need to get every page then iterate through them to generate particle json pages for them.
//there's the index, then for folders theres entries, projects, diversions, diversions/hentaigames

var fileInfos = new List<Tuple<FileInfo, string>>();
var index = FindFile("index.html");
fileInfos.Add(new Tuple<FileInfo, string>(new FileInfo(index), "https://athene.gay"));

var folder = FindDirectory("entries");
var filePaths = Directory.EnumerateFiles(folder);
if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        if (Path.GetFileNameWithoutExtension(path).Equals("template", StringComparison.InvariantCultureIgnoreCase))
            continue;
        fileInfos.Add(new Tuple<FileInfo, string>(new FileInfo(path), "https://athene.gay/entries"));
    }
}

folder = FindDirectory("projects");
filePaths = Directory.EnumerateFiles(folder);
if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        if (Path.GetFileNameWithoutExtension(path).Equals("template", StringComparison.InvariantCultureIgnoreCase))
            continue;
        fileInfos.Add(new Tuple<FileInfo, string>(new FileInfo(path), "https://athene.gay/projects"));
    }
}

folder = FindDirectory("diversions");
filePaths = Directory.EnumerateFiles(folder);
if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        if (Path.GetFileNameWithoutExtension(path).Equals("template", StringComparison.InvariantCultureIgnoreCase))
            continue;
        fileInfos.Add(new Tuple<FileInfo, string>(new FileInfo(path), "https://athene.gay/diversions"));
    }
}

folder = FindDirectory("diversions/hentaigames");
filePaths = Directory.EnumerateFiles(folder);
if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        if (Path.GetFileNameWithoutExtension(path).Equals("template", StringComparison.InvariantCultureIgnoreCase))
            continue;
        fileInfos.Add(new Tuple<FileInfo, string>(new FileInfo(path), "https://athene.gay/diversions/hentaigames"));
    }
}

List<string> textTags = new List<string>() {"p", "h1", "h2", "h3", "h4"};

foreach (var file in fileInfos)
{
    var doc = new HtmlDocument();
    doc.Load(file.Item1.FullName);
    var particleRoot = new ParticleRoot()
    {
        Format = "particle",
        Title = doc.DocumentNode.SelectSingleNode("//title").InnerText,
        Content = new List<Content>()
    };
    
    TraverseHTML(doc.DocumentNode.SelectSingleNode("//body"), particleRoot, file.Item2);
    File.WriteAllText(file.Item1.FullName.Replace(".html", ".json"), JsonSerializer.Serialize(particleRoot));
}

void TraverseHTML(HtmlNode traversalNode, ParticleRoot particleRoot, string uriPath)
{
    if (textTags.Contains(traversalNode.Name))
    {
        particleRoot.Content.Add(new Content()
        {
            Type = "paragraph",
            Text = traversalNode.InnerText.Replace("  ", "").Replace("\n", " ").Replace("\r", " ").Trim(),
        });
    }

    if (traversalNode.Name == "a" && traversalNode.GetAttributeValue("href", "").EndsWith("html"))
    {
        particleRoot.Content.Add(new Content()
        {
            Type = "button",
            Label = traversalNode.InnerText.Replace("  ", "").Replace("\n", " ").Replace("\r", " ").Trim(),
            Action = $"{uriPath}/{traversalNode.GetAttributeValue("href", "").Replace(".html", ".json")}"
        });
    }
    if (traversalNode.HasChildNodes)
    {
        foreach (var node in traversalNode.ChildNodes)
        {
            TraverseHTML(node, particleRoot, uriPath);
        }
    }
}

string FindFile(string fileName)
{
    var file = Path.Combine(Directory.GetCurrentDirectory(), $"../{fileName}");
    if (!File.Exists(file))
    {
        file = Path.Combine(Directory.GetCurrentDirectory(), $"../../../../../{fileName}");
    }
    if (!File.Exists(file))
    {
        file = Path.Combine(Directory.GetCurrentDirectory(), $"../../../../{fileName}");
    }

    return file;
}

string FindDirectory(string folderName)
{
    var folder = Path.Combine(Directory.GetCurrentDirectory(), $"../{folderName}");
    if (!Directory.Exists(folder))
    {
        folder = Path.Combine(Directory.GetCurrentDirectory(), $"../../../../../{folderName}");
    }
    if (!Directory.Exists(folder))
    {
        folder = Path.Combine(Directory.GetCurrentDirectory(), $"../../../../{folderName}");
    }
    return folder;
}