using Markdig.Extensions.Tables;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using TheLeftExit.Rambles;

Rambler.Run<RambleRenderer>(new RambleConfiguration(
    "content",
    "publish"
));

public record TypedRamble(
    // Utility fields
    string FilePath,
    string Href,
    string Content,
    // Renderer fields
    string Title,
    int? HeaderIndex,
    DateTime? Date,
    string? Category,
    string? Footer
);

public static class StringEnumerableExtensions
{
    public static string Concatenate(this IEnumerable<string> strings)
    {
        return !strings.Any()
            ? ""
            : strings.Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine().Append(s), sb => sb.ToString());
    }
}

public class RambleRenderer : IRambleRenderer {
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

    public IEnumerable<RambleFileInfo> Render(IEnumerable<RambleInfo> rambles)
    {
        var typedRambles = rambles
            .Select(x => new TypedRamble
            (
                FilePath: Path.ChangeExtension(x.Path, ".html"),
                Href: GetHref(x.Path),
                Content: x.Ramble.GetContent(),
                Title: x.Ramble.GetValue("Title") ?? throw new ArgumentNullException(),
                HeaderIndex: int.TryParse(x.Ramble.GetValue("HeaderIndex"), out int headerIndex) ? headerIndex : null,
                Date: DateTime.TryParse(x.Ramble.GetValue("Date"), out DateTime date) ? date : null,
                Category: x.Ramble.GetValue("Category"),
                Footer: x.Ramble.GetValue("Footer") ?? x.Ramble.GetValue("Category")
            )).ToList();

        var headerRambles = typedRambles
            .Where(x => x.HeaderIndex is not null)
            .OrderBy(x => x.HeaderIndex);

        var header = headerRambles
            .Select(x => string.Format(_headerTemplate, x.Href, x.Title))
            .Concatenate();

        var footers = typedRambles
            .Where(x => x.Date is not null && x.Category is not null)
            .GroupBy(x => x.Category!)
            .ToDictionary(
                group => group.Key,
                group => string.Format(
                    _footerTemplate,
                    group
                        .OrderByDescending(x => x.Date)
                        .Select(x => string.Format(_footerEntryTemplate, x.Date?.ToString("MMMM d, yyyy 'at' h tt"), x.Href, x.Title))
                        .Concatenate()
                    )
            );

        foreach (var ramble in typedRambles)
        {
            var page = string.Format(
                _pageTemplate,
                ramble.Title,
                header,
                ramble.Content,
                footers.GetValueOrDefault(ramble.Footer ?? "") ?? "",
                relativeUrl
            );
            yield return new RambleFileInfo(ramble.FilePath, page);
        }

        var sitemap = string.Format(
            _sitemapTemplate,
            typedRambles
                .Where(x => x.Date is not null || x.HeaderIndex is not null)
                .Select(x => string.Format(_sitemapEntryTemplate, baseUrl + x.Href))
                .Concatenate()
            );
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