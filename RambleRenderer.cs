using System.Reflection;
using System.Text;
using System.Xml;

public class RambleRenderer : IRambleRenderer {
    private readonly string _pageTemplate;
    private readonly string _headerTemplate;
    private readonly string _footerTemplate;
    private readonly string _sitemapTemplate;
    private readonly string _sitemapEntryTemplate;

    private readonly RambleConfiguration _configuration;

    private string GetResourceString(string resourceName) {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)!;
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public RambleRenderer(RambleConfiguration configuration) {
        _pageTemplate = GetResourceString("TheLeftExit.Rambles.Templates.Page.txt");
        _headerTemplate = GetResourceString("TheLeftExit.Rambles.Templates.Header.txt");
        _footerTemplate = GetResourceString("TheLeftExit.Rambles.Templates.Footer.txt");
        _sitemapTemplate = GetResourceString("TheLeftExit.Rambles.Templates.Sitemap.txt");
        _sitemapEntryTemplate = GetResourceString("TheLeftExit.Rambles.Templates.SitemapEntry.txt");
        
        _configuration = configuration;
    }

    private string GetHref(string filePath) {
        var trimmedPath = Path.ChangeExtension(filePath, null).ToLower();
        if (trimmedPath == "index") return "/";
        return "/" + trimmedPath;
    }

    public IEnumerable<RambleFileInfo> Render(IEnumerable<RambleInfo> rambles) {
        var typedRambles = rambles
            .Select(x => new {
                FilePath = Path.ChangeExtension(x.Path, ".html"),
                LastWriteDate = XmlConvert.ToString(x.LastWriteDate, XmlDateTimeSerializationMode.Utc),
                Href = GetHref(x.Path),
                Content = x.Ramble.GetContent(),
                Title = x.Ramble.GetValue("Title"),
                HeaderIndex = int.TryParse(x.Ramble.GetValue("HeaderIndex"), out int headerIndex) ? headerIndex : (int?)null,
                Date = DateOnly.TryParse(x.Ramble.GetValue("Date"), out DateOnly date) ? date : (DateOnly?)null,
                HideFooter = bool.TryParse(x.Ramble.GetValue("HideFooter"), out bool hideFooter) ? hideFooter : (bool?)null,
            }).ToArray();

        var header = typedRambles
            .Where(x => x.HeaderIndex is not null)
            .OrderBy(x => x.HeaderIndex)
            .Select(x => string.Format(_headerTemplate, x.Href, x.Title))
            .Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine().Append(s), sb => sb.ToString());

        var footer = typedRambles
            .Where(x => x.Date is not null)
            .OrderByDescending(x => x.Date)
            .Select(x => string.Format(_footerTemplate, x.Date, x.Href, x.Title))
            .Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine().Append(s), sb => sb.ToString());

        foreach(var ramble in typedRambles) {
            var page = string.Format(_pageTemplate, ramble.Title, header, ramble.Content, footer);
            yield return new RambleFileInfo(ramble.FilePath, page);
        }

        var sitemapEntries = typedRambles
            .Where(x => x.Date is not null || x.HeaderIndex is not null)
            .Select(x => string.Format(_sitemapEntryTemplate, _configuration.RootUrl + x.Href, x.LastWriteDate))
            .Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine().Append(s), sb => sb.ToString());
        var sitemap = string.Format(_sitemapTemplate, sitemapEntries);
        yield return new RambleFileInfo("sitemap.xml", sitemap);
    }
}