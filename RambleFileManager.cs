internal class RambleFileManager : IRambleFileManager {
    public void CopyNonRamblesAndWriteRambles(string fromPath, string toPath, IEnumerable<RambleFileInfo> rambles) {
        var allRelativeDirs = Directory
            .EnumerateDirectories(fromPath, "*", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(fromPath, x));
        foreach (var relativeDir in allRelativeDirs) {
            var targetPath = Path.Combine(toPath, relativeDir);
            Directory.CreateDirectory(targetPath);
        }

        var allNonRamblePaths = Directory
            .EnumerateFiles(fromPath, "*", SearchOption.AllDirectories)
            .Where(x => Path.GetExtension(x) != ".md");
        foreach(var path in allNonRamblePaths) {
            var relativePath = Path.GetRelativePath(fromPath, path);
            var targetPath = Path.Combine(toPath, relativePath);
            File.Copy(path, targetPath, true);
        }

        foreach(var ramble in rambles) {
            var targetPath = Path.Combine(toPath, ramble.Path);
            File.WriteAllText(targetPath, ramble.Content);
        }
    }

    public IEnumerable<RambleFileInfo> ReadAllRambles(string fromPath) {
        var allRamblePaths = Directory.EnumerateFiles(fromPath, "*.md", SearchOption.AllDirectories);
        foreach (var path in allRamblePaths) {
            var text = File.ReadAllText(path);
            var relativePath = Path.GetRelativePath(fromPath, path);
            yield return new RambleFileInfo(relativePath, text);
        }
    }
}