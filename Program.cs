
using HtmlAgilityPack;
using System.Xml;

const string SiteUrl = "https://bing.gifposter.com/";
const string DestinationFolder = "C:\\Wallpaper\\";

if (!Directory.Exists(DestinationFolder)) Directory.CreateDirectory(DestinationFolder);

// Get Html
var client = new HttpClient
{
    Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite)
};

string htmlBody = await client.GetStringAsync(SiteUrl + "sitemap.xml");

// From String
var xml = new XmlDocument();
xml.LoadXml(htmlBody);

XmlNodeList xmlNodes = xml.GetElementsByTagName("url");

foreach (XmlNode node in xmlNodes)
{
    if (node["loc"] == null) continue;

    string url = node["loc"]!.InnerText;

    if (url.StartsWith(SiteUrl + "wallpaper-") && url.EndsWith(".html"))
    {
        string html = await client.GetStringAsync(url);

        // From String
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        HtmlNode htmlNode = doc.DocumentNode.SelectSingleNode("//a[@class='icon download']");

        string link = htmlNode.Attributes["href"].Value;
        string downloadLink = SiteUrl + link;

        // You have to create the folder
        string destinationFile = DestinationFolder + link.Replace("/bingImages/", "");

        if (File.Exists(destinationFile))
        {
            Console.WriteLine($"Already downloaded... '{downloadLink}'");
            continue;
        }

        var uri = new Uri(downloadLink);

        await client.DownloadFileTaskAsync(uri, destinationFile);
        Console.WriteLine($"File downloaded... '{downloadLink}'");
    }
}

public static class HttpClientExtension
{
    public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string fileName)
    {
        try
        {
            Stream stream = await client.GetStreamAsync(uri);
            var fileStream = new FileStream(fileName, FileMode.CreateNew);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
    }
}
