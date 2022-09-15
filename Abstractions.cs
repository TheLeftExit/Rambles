using System.Diagnostics.CodeAnalysis;

public record RambleInfo(string Path, IRamble Ramble, DateTime LastWriteDate);

public record RambleFileInfo(string Path, string Content, DateTime LastWriteDate = default);

public interface IRambleFileManager {
    IEnumerable<RambleFileInfo> ReadAllRambles(string fromPath);
    void CopyNonRamblesAndWriteRambles(string fromPath, string toPath, IEnumerable<RambleFileInfo> rambles);
}

public interface IRamble {
    string GetContent();
    string? GetValue(string key);
}

public interface IRambleParser {
    IRamble Parse(string text);
}

public interface IRambleRenderer {
    IEnumerable<RambleFileInfo> Render(IEnumerable<RambleInfo> rambles);
}

public class RambleConfiguration {
    public string FromPath { get; set; } = default!;
    public string ToPath { get; set; } = default!;
    public string RootUrl { get; set; } = default!;
    public string SiteName { get; set; } = default!;
}