using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configBuilder = new ConfigurationBuilder();
configBuilder.AddJsonFile("config.json");
var config = configBuilder.Build();
var rambleConfig = config.Get<RambleConfiguration>();

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton(rambleConfig);
serviceCollection.AddSingleton<IRambleFileManager, RambleFileManager>();
serviceCollection.AddSingleton<IRambleParser, RambleParser>();
serviceCollection.AddSingleton<IRambleRenderer, RambleRenderer>();
serviceCollection.AddSingleton<Application>();

var serviceProvider = serviceCollection.BuildServiceProvider();
var application = serviceProvider.GetRequiredService<Application>();
application.Run();

public class Application {
    private readonly RambleConfiguration _configuration;
    private readonly IRambleFileManager _fileManager;
    private readonly IRambleParser _parser;
    private readonly IRambleRenderer _renderer;

    public Application(IRambleFileManager fileManager, IRambleParser parser, IRambleRenderer renderer, RambleConfiguration configuration) {
        _configuration = configuration;
        _fileManager = fileManager;
        _parser = parser;
        _renderer = renderer;
    }

    public void Run() {
        var markdownFiles = _fileManager.ReadAllRambles(_configuration.FromPath).ToArray();
        var parsedFiles = markdownFiles.Select(x => new RambleInfo(x.Path, _parser.Parse(x.Content), x.LastWriteDate)).ToArray();
        var renderedFiles = _renderer.Render(parsedFiles).ToArray();
        _fileManager.CopyNonRamblesAndWriteRambles(_configuration.FromPath, _configuration.ToPath, renderedFiles);
    }
}