using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

public class RambleParser : IRambleParser {
    private readonly MarkdownPipeline _markdownPipeline;

    public RambleParser() {
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UseMediaLinks()
            .Build();
    }

    private static IReadOnlyDictionary<string, string> ParseYaml(ReadOnlySpan<char> text) {
        var result = new Dictionary<string, string>();
        foreach (var line in text.EnumerateLines()) {
            if (line.Trim() == "---") continue;
            var delimiterIndex = line.IndexOf(':');
            if (delimiterIndex == -1) {
                // Log a warning
                continue;
            }
            var key = line.Slice(0, delimiterIndex).Trim().ToString();
            var value = line.Slice(delimiterIndex + 1).Trim().ToString();
            result.Add(key, value);
        }
        return result;
    }

    public IRamble Parse(string text) {
        var document = Markdown.Parse(text, _markdownPipeline);
        var html = document.ToHtml(_markdownPipeline);
        
        var frontmatterBlock = document.Descendants<YamlFrontMatterBlock>().SingleOrDefault();
        if (frontmatterBlock is null) return new Ramble(html);
        
        var frontmatter = text.AsSpan().Slice(frontmatterBlock.Span.Start, frontmatterBlock.Span.Length);
        var metadata = ParseYaml(frontmatter);
        return new Ramble(html, metadata);
        
    }

    private class Ramble : IRamble {
        private readonly string _content;
        private readonly IReadOnlyDictionary<string, string>? _metadata;

        public Ramble(string content, IReadOnlyDictionary<string, string>? metadata = null) {
            _content = content;
            _metadata = metadata;
        }

        public string GetContent() => _content;
        public string? GetValue(string key) => _metadata?.GetValueOrDefault(key);
    }
}