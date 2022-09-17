# Rambles

I wrote Rambles because I wanted a simple static content generator like Hexo, but didn't want to learn its specifics or stray outside of the .NET environment.

## How it works

 - Source/target paths, root URL and website header are taken from the `config.json` file in the working directory. The schema must match [`RambleConfiguration`](Abstractions.cs#L25) class declaration.
    - Note that the `config.json` file is not present and must be created manually. For testing purposes, you can create and inject a `RambleConfiguration` instance directly.
 - All `.md` files in the source directory are parsed as markdown documents with YAML-like metadata (see: [`RambleParser.cs`](RambleParser.cs)).
 - All documents ("rambles") are aggregated and rendered (see: [`RambleRenderer.cs`](RambleRenderer.cs)).
 - All rendered documents are dumped to the target directory. All other files are copied as-is.

I personally use a private repository to store the website's contents and a GitHub Actions workflow that fetches this repository, builds and runs Rambles, and publishes the output directly to GitHub Pages (see: [https://theleftexit.net/](https://theleftexit.net/)).

## Metadata and page structure

`RambleParser` scans the metadata block for entries matching the `{0}: {1}` pattern (hence YAML-like). All such entries are added to a `Dictionary<string, string>`. This logic is not specific to any metadata schema or rendering implementation.

`RambleRenderer` aggregates all documents using site-specific logic, optionally adds/removes certain ones, and returns the resulting collection.

My rendering implementation uses [`string.Format`](https://learn.microsoft.com/en-us/dotnet/api/system.string.format)-compatible templates stored as embedded resources and adds a sitemap. It recognizes the following metadata properties:
 - `Title` - used wherever a page title is required.
 - `HeaderIndex` - if specified, indicates that the page should be added to the site header at a specific position.
 - `Date` - if specified, indicates that the page should be added to the site footer and positioned according to its date.
 - `HideFooter` - if specified and set to `true`, removes the site footer from the page.

The `Title` property is required. If both `HeaderIndex` and `Date` properties are missing, the page is considered private and is not included in the sitemap.

## Other stuff

As with my other projects, this project is [unlicensed](https://unlicense.org/). I hope someone finds it useful for learning purposes, or maybe even as a base for their own website.

If you have any suggestions on code quality or pretty much anything else, feel free to open an issue.
