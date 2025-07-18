﻿using OpenGraphNet;
using RssFeedGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Scanning entries folder for latest files");

var folder = FindDirectory("entries");

var filePaths = Directory.EnumerateFiles(folder, "*.html");
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

folder = FindDirectory("projects");

filePaths = Directory.EnumerateFiles(folder, "*.html");

if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        var fInfo = new FileInfo(path);
        fileInfos.Add(fInfo);
    }
}

Console.WriteLine("Scanning diversions folder for latest files");

folder = FindDirectory("diversions");

filePaths = Directory.EnumerateFiles(folder, "*.html");

if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        var fInfo = new FileInfo(path);
        fileInfos.Add(fInfo);
    }
}

fileInfos = fileInfos.OrderByDescending(f => f.CreationTimeUtc).Take(20).ToList();

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
    else if (file.FullName.Contains("diversions"))
    {
        item.link = "https://athene.gay/diversions/" + Path.GetFileName(file.Name);
    }
    item.guid = new rssChannelItemGuid()
    {
        isPermaLink = true,
        Value = item.link
    };
    myRSS.channel.item.Add(item);
}

var output = Generator.SerializeRSS(myRSS);

var rssPath = Path.Combine(Directory.GetCurrentDirectory(), "../feed.xml");

if (!File.Exists(rssPath))
{
    rssPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../../feed.xml");
}

if (File.Exists(rssPath))
{
    File.Delete(rssPath);
}

Console.WriteLine("Main RSS generated, outputting to file");

File.WriteAllText(rssPath, output);

Console.WriteLine("Generating hentai subseries feed");

folder = FindDirectory("diversions/hentaigames");

filePaths = Directory.EnumerateFiles(folder, "*.html");
fileInfos = new List<FileInfo>();

if (filePaths.Any())
{
    foreach (var path in filePaths)
    {
        if (Path.GetFileNameWithoutExtension(path).Equals("template", StringComparison.InvariantCultureIgnoreCase))
            continue;
        if (Path.GetFileNameWithoutExtension(path).Equals("list", StringComparison.InvariantCultureIgnoreCase))
            continue;
        var fInfo = new FileInfo(path);
        fileInfos.Add(fInfo);
    }
}

fileInfos = fileInfos.OrderByDescending(f => f.CreationTimeUtc).Take(20).ToList();

myRSS = new rss();

myRSS.version = 2.0m;

myRSS.channel = new rssChannel
{
    title = "athene.gay - hentai games subseries entries",
    description = "entries in the hentai game sub-series for athene.gay",
    language = "en-GB",
    link = "https://athene.gay/hentaigames/list.html",
    item = new List<rssChannelItem>(),
    link1 = new link
    {
        href = "https://athene.gay/hentaigames.xml",
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
        link = "https://athene.gay/diversions/hentaigames/" + Path.GetFileName(file.Name)
    };
    item.guid = new rssChannelItemGuid()
    {
        isPermaLink = true,
        Value = item.link
    };
    myRSS.channel.item.Add(item);
}

output = Generator.SerializeRSS(myRSS);

rssPath = Path.Combine(Directory.GetCurrentDirectory(), "../hentaigames.xml");

if (!File.Exists(rssPath))
{
    rssPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../../hentaigames.xml");
}

if (File.Exists(rssPath))
{
    File.Delete(rssPath);
}

Console.WriteLine("Hentai Games RSS generated, outputting to file");

File.WriteAllText(rssPath, output);


string FindDirectory(string folderName)
{
    var folder = Path.Combine(Directory.GetCurrentDirectory(), $"../{folderName}");
    if (!Directory.Exists(folder))
    {
        folder = Path.Combine(Directory.GetCurrentDirectory(), $"../../../../{folderName}");
    }
    return folder;
}