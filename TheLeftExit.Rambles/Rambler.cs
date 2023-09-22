namespace TheLeftExit.Rambles;

public record RambleInfo(string Path, Ramble Ramble);
public record RambleFileInfo(string Path, string Content);
public record RambleConfiguration(string FromPath, string ToPath);

public interface IRambleRenderer
{
    IEnumerable<RambleFileInfo> Render(IEnumerable<RambleInfo> rambles);
}

public static class Rambler
{
    private static readonly RambleFileManager fileManager = new RambleFileManager();
    private static readonly RambleParser parser = new RambleParser();

    public static void Run(IRambleRenderer renderer, RambleConfiguration configuration)
    {
        var markdownFiles = fileManager.ReadAllRambles(configuration.FromPath).ToArray();
        var parsedFiles = markdownFiles.Select(x => new RambleInfo(x.Path, parser.Parse(x.Content))).ToArray();
        var renderedFiles = renderer.Render(parsedFiles).ToArray();
        fileManager.CopyNonRamblesAndWriteRambles(configuration.FromPath, configuration.ToPath, renderedFiles);
    }

    public static void Run<TRenderer>(RambleConfiguration configuration) where TRenderer : IRambleRenderer, new()
    {
        Run(new TRenderer(), configuration);
    }
}