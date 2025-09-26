namespace Experimental1.Commands;

using Experimental1.Samples;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using DryIoc;

[ConsoleAppFramework.RegisterCommands("sample1")]
public class Sample1Command
{
    private readonly ILogger<Sample1Command> _logger;
    private readonly Sample1 _sample1;
    public Sample1Command(IContainer container, ILogger<Sample1Command> logger)
    {
        _logger = logger;
        _sample1 = container.Resolve<Sample1>();
    }

    [Command("map-list")]
    public void MapList()
    {
        _logger.LogInformation("Executing Sample1Command MapList");
        _sample1.MapList();
       
    }
    [Command("map-object")]
    public void MapObject()
    {
        _logger.LogInformation("Executing Sample1Command MapObject");
        _sample1.MapSingleObject();

    }
}
