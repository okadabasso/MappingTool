namespace Experimental1.Commands;

using Experimental1.Samples;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

[ConsoleAppFramework.RegisterCommands("sample1")]
public class Sample1Command
{
    private readonly ILogger<Sample1Command> _logger;
    public Sample1Command(ILogger<Sample1Command> logger)
    {
        _logger = logger;
    }

    [Command("map-list")]
    public void Execute1()
    {
        var sample = new Sample1();
        sample.SampleMethod1();
       
    }
    [Command("map-object")]
    public void Execute2()
    {
        var sample = new Sample1();
        sample.SampleMethod2();

    }
}
