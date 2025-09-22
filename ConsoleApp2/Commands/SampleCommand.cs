namespace ConsoleApp2.Commands;

using MappingTool.Mapping;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

[ConsoleAppFramework.RegisterCommands("sample")]
public class Sample1Command
{
    private readonly ILogger<Sample1Command> _logger;
    public Sample1Command(ILogger<Sample1Command> logger)
    {
        _logger = logger;
    }

    [Command("method1")]
    public void Execute1()
    {
        var source = new SourceData { Id = 1, Name = "Source" };
        var destination = new DestinationData();

        var context = new MappingContext();
        var mapper = MapperFactory<SourceData, DestinationData>.CreateMapper();
        mapper.Map(source, destination);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
    }
    class SourceData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
    class DestinationData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
