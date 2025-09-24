namespace Experimental1.Commands;

using Experimental1.Samples;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

[ConsoleAppFramework.RegisterCommands("sample2")]
public class Sample2Command
{
    private readonly ILogger<Sample2Command> _logger;
    public Sample2Command(ILogger<Sample2Command> logger)
    {
        _logger = logger;
    }

    [Command("method1")]
    public void Execute1()
    {
        var sample = new Sample2();
        sample.SampleMethod();
       
    }}
