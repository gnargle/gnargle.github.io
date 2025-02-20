using OpenGraphNet;
using RssFeedGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Scanning entries folder for latest files");

var folder = String.Empty;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    folder = Path.Combine(Directory.GetCurrentDirectory(), "../entries");
}
else
{
    folder = Path.Combine(Directory.GetCurrentDirectory(), "../../../../entries");
}
var filePaths = Directory.EnumerateFiles(folder);
var fileInfos = new List<FileInfo>();

if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        if (Path.GetFileNameWithoutExtension(path).Equals("template", StringComparison.InvariantCultureIgnoreCase))
            continue;
        var fInfo = new FileInfo(path);
        fileInfos.Add(fInfo);
    }
}

Console.WriteLine("Scanning projects folder for latest files");

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    folder = Path.Combine(Directory.GetCurrentDirectory(), "../projects");
}
else
{
    folder = Path.Combine(Directory.GetCurrentDirectory(), "../../../../projects");
}

filePaths = Directory.EnumerateFiles(folder);

if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        var fInfo = new FileInfo(path);
        fileInfos.Add(fInfo);
    }
}

fileInfos = fileInfos.OrderByDescending(f => f.CreationTimeUtc).Take(10).ToList();

var myRSS = new rss();

myRSS.version = 2.0m;

myRSS.channel = new rssChannel
{
    title = "athene.gay entries",
    description = "blog entries for athene.gay",
    language = "en-GB",
    link = "https://athene.gay",
    item = new List<rssChannelItem>(),
    link1 = new link
    {
        href = "https://athene.gay/feed.xml",
        rel = "self",
        type = "application/rss+xml",
    }
};

foreach (var file in fileInfos)
{
    var htmlString = File.ReadAllText(file.FullName);
    OpenGraph graph = OpenGraph.ParseHtml(htmlString);
    var publishDate = DateTime.Parse(graph.Metadata["article:published_time"].First());
    var item = new rssChannelItem()
    {
        title = graph.Title,
        description = graph.Metadata["og:description"].First(),
        pubDate = publishDate.ToString("r"),
    };
    if (file.FullName.Contains("entries"))
    {
        item.link = "https://athene.gay/entries/" + Path.GetFileName(file.Name);
    }
    else if (file.FullName.Contains("projects"))
    {
        item.link = "https://athene.gay/projects/" + Path.GetFileName(file.Name);
    }
    item.guid = new rssChannelItemGuid()
    {
        isPermaLink = true,
        Value = item.link
    };
    myRSS.channel.item.Add(item);
}

var output = Generator.SerializeRSS(myRSS);

var rssPath = String.Empty;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    rssPath = Path.Combine(Directory.GetCurrentDirectory(), "../feed.xml");
}
else
{
    rssPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../../feed.xml");
}

if (File.Exists(rssPath))
{
    File.Delete(rssPath);
}

Console.WriteLine("RSS generated, outputting to console and file");
Console.WriteLine(output);

File.WriteAllText(rssPath, output);