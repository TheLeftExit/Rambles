using System.Diagnostics.CodeAnalysis;
using TheLeftExit.Rambles;
using TheLeftExit.Rambles.Demo.Templates;

Rambler.Run<RambleRenderer>(new RambleConfiguration(
    "content",
    "publish"
));

public class RambleRenderer : IRambleRenderer
{
    private const string baseUrl = "https://theleftexit.net";
    private const string relativeUrl = "/Rambles/";

    private static string GetHref(string filePath)
    {
        var trimmedPath = Path.ChangeExtension(filePath, null).ToLower();
        if (trimmedPath.ToLower().EndsWith("index"))
        {
            trimmedPath = trimmedPath.Substring(0, trimmedPath.ToLower().LastIndexOf("index"));
        }
        return relativeUrl + trimmedPath;
    }

    [SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.", Justification = "didn't ask")]
    public IEnumerable<RambleFileInfo> Render(IEnumerable<RambleInfo> rambles)
    {
        var typedRambles = rambles
            .Select(x => new RambleTemplate
            {
                FilePath = Path.ChangeExtension(x.Path, ".html"),
                Href = GetHref(x.Path),
                Content = x.Ramble.GetContent(),
                Title = x.Ramble.GetValue("Title") ?? throw new ArgumentNullException(nameof(RambleTemplate.Title)),
                HeaderIndex = int.TryParse(x.Ramble.GetValue("HeaderIndex"), out int headerIndex) ? headerIndex : null,
                Date = DateOnly.TryParse(x.Ramble.GetValue("Date"), out DateOnly date) ? date : null,
                HideFooter = bool.TryParse(x.Ramble.GetValue("HideFooter"), out bool hideFooter) ? hideFooter : null,
            }).ToArray();

        foreach (var ramble in typedRambles)
        {
            ramble.AllRambles = typedRambles;
            yield return new(ramble.FilePath, ramble.RenderAsync().Result);
        }

        var sitemap = new SitemapTemplate
        {
            AllRambles = typedRambles,
            BaseUrl = baseUrl
        };

        yield return new RambleFileInfo("sitemap.xml", sitemap.RenderAsync().Result);
    }
}