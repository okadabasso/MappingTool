namespace ConsoleApp1.Commands;

using ConsoleApp1.Samples;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

[ConsoleAppFramework.RegisterCommands("sample3")]
public class Sample3Command
{
    private readonly ILogger<Sample3Command> _logger;
    public Sample3Command(ILogger<Sample3Command> logger)
    {
        _logger = logger;
    }

    [Command("method1")]
    public void Execute1()
    {
        var sample = new Sample3();
        sample.SampleMethod();
       
    }}
