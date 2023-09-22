using System.Text;
using TheLeftExit.Rambles;

Rambler.Run<RambleRenderer>(new RambleConfiguration("content", "publish"));

public class RambleRenderer : IRambleRenderer {
    private const string baseUrl = "https://theleftexit.net";
    private const string relativeUrl = "/Rambles/";

    private static string GetHref(string filePath) {
        var trimmedPath = Path.ChangeExtension(filePath, null).ToLower();
        if (trimmedPath == "index") return relativeUrl;
        return relativeUrl + trimmedPath;
    }

    public IEnumerable<RambleFileInfo> Render(IEnumerable<RambleInfo> rambles) {
        var typedRambles = rambles
            .Select(x => new {
                FilePath = Path.ChangeExtension(x.Path, ".html"),
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

        var footerTable = typedRambles
            .Where(x => x.Date is not null)
            .OrderByDescending(x => x.Date)
            .Select(x => string.Format(_footerEntryTemplate, x.Date, x.Href, x.Title))
            .Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine().Append(s), sb => sb.ToString());

        var footer = string.IsNullOrEmpty(footerTable) ? "" : string.Format(_footerTemplate, footerTable);

        foreach(var ramble in typedRambles) {
            var page = string.Format(
                _pageTemplate,
                ramble.Title,
                header,
                ramble.Content,
                ramble.HideFooter ?? false ? "" : footer
            );
            yield return new RambleFileInfo(ramble.FilePath, page);
        }

        var sitemapEntries = typedRambles
            .Where(x => x.Date is not null || x.HeaderIndex is not null)
            .Select(x => string.Format(_sitemapEntryTemplate, baseUrl + x.Href))
            .Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine().Append(s), sb => sb.ToString());
        var sitemap = string.Format(_sitemapTemplate, sitemapEntries);
        yield return new RambleFileInfo("sitemap.xml", sitemap);
    }

    private readonly string _pageTemplate =
"""
<!DOCTYPE html>
<html lang='en'>
    <head>
    <title>{0}</title>
    <link rel='stylesheet' href='https://theleftexit.net/Rambles/style.css'>
    </head>
    <body>
    <div class='page'>
        <div class='title'>
        <h1>Rambles</h1>
        </div>
        <div class='header'>
        <ul>{1}
        </ul>
        </div>
        <div class='content'>
{2}
        </div>
{3}
    </div>
    </body>
</html>
""";

    private readonly string _headerTemplate =
"""
          <li>
            <a href='{0}'>{1}</a>
          </li>
""";

    private readonly string _footerTemplate =
"""
      <hr>
      <div class='footer'>
          <table>{0}
          </table>
      </div>
""";

    private readonly string _footerEntryTemplate =
"""
          <tr>
            <td class='footer-date'>
              {0}
            </td>
            <td class='footer-title'>
              <a href='{1}'>{2}</a>
            </td>
          </tr>
""";

    private readonly string _sitemapTemplate =
"""
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">{0}
</urlset>
""";

    private readonly string _sitemapEntryTemplate =
"""
    <url>
        <loc>{0}</loc>
    </url>
""";
}