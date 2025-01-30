using RssFeedGenerator;
using System;
using System.Collections.Generic;
using System.IO;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Scanning entries folder for latest files");

var filePaths = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "../../../../entries"));
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

filePaths = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "../../../../projects"));

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
    item = new List<rssChannelItem>()
};

foreach (var file in fileInfos)
{
    var item = new rssChannelItem()
    {
        title = Path.GetFileNameWithoutExtension(file.Name),
        pubDate = file.CreationTimeUtc.ToString(),
    };
    if (file.FullName.Contains("entries"))
    {
        item.link = "https://athene.gay/entries/" + Path.GetFileName(file.Name);
    }
    else if (file.FullName.Contains("projects"))
    {
        item.link = "https://athene.gay/projects/" + Path.GetFileName(file.Name);
    }
    myRSS.channel.item.Add(item);
}

var output = Generator.SerializeRSS(myRSS);

var rssPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../../feed.xml");

if (File.Exists(rssPath))
{
    File.Delete(rssPath);
}

Console.WriteLine("RSS generated, outputting to console and file");
Console.WriteLine(output);

File.WriteAllText(rssPath, output);