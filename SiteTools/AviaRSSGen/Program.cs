// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using RssFeedGenerator;

if (!File.Exists("seenEntries.txt"))
{
    File.WriteAllText("seenEntries.txt", string.Empty);
}

var lines = File.ReadAllLines("seenEntries.txt");
var seenEntries = new Dictionary<string, DateTime>();
foreach (var line in lines)
{
    var arr = line.Split('|');
    seenEntries.Add(arr[0], DateTime.Parse(arr[1]));
}

var client = new HttpClient()
{
    BaseAddress = new Uri("https://xunxar.neocities.org/")
};
var resp = await client.GetAsync("");
var htmlStream = await resp.Content.ReadAsStreamAsync();

var myRSS = new rss();

myRSS.version = 2.0m;

myRSS.channel = new rssChannel
{
    title = "xunxar.neocities.org entries",
    description = "xunxar.neocities.org articles",
    language = "en-GB",
    link = "https://xunxar.neocities.org/",
    item = new List<rssChannelItem>(),
    link1 = new link
    {
        href = "https://xunxar.neocities.org/feed.xml",
        rel = "self",
        type = "application/rss+xml",
    }
};

List<string> textTags = new List<string>() { "p", "h1", "h2", "h3", "h4", "blockquote", "em" };

var doc = new HtmlDocument();
doc.Load(htmlStream);

var articleNodes = doc.DocumentNode.SelectNodes("//div[@class='post']");
foreach (var node in articleNodes)
{
    var item = new rssChannelItem();
    var articleHtml = string.Empty;
    foreach (var child in node.ChildNodes)
    {
        if (child.Name == "h2" || child.Name == "h3")
        {
            item.title = child.InnerText;
        }
        else
        {
            articleHtml += child.InnerText;
        }
    }
    
    item.description = articleHtml.Trim();
    
    //some sanity checks
    if (string.IsNullOrWhiteSpace(item.title) || string.IsNullOrWhiteSpace(item.description))
        continue;
    
    if (seenEntries.ContainsKey(item.title))
    {
        item.pubDate = seenEntries[item.title].ToString("r");
    } else
    {
        var date = DateTime.Now;
        item.pubDate = DateTime.Now.ToString("r");
        seenEntries.Add(item.title, date);
    }
    item.guid = new rssChannelItemGuid()
    {
        isPermaLink = false,
        Value = item.title
    };
    myRSS.channel.item.Add(item);
}

var output = Generator.SerializeRSS(myRSS);

File.WriteAllText("feed.xml", output);

string seenEntriesString = string.Empty;
foreach (var entry in seenEntries.Keys)
{
    seenEntriesString += $"{entry}|{seenEntries[entry]}\n";
}
File.WriteAllText("seenEntries.txt", seenEntriesString);