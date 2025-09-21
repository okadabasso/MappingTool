namespace ConsoleApp1.Commands;

using ConsoleApp1.Samples;
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

    [Command("method1")]
    public void Execute1()
    {
        var sample = new Sample1();
        sample.SampleMethod1();
       
    }
    [Command("method3")]
    public void Execute3()
    {
        var sample = new Sample1();
        sample.SampleMethod3();

    }
    [Command("method4")]
    public void Execute4()
    {
        var sample = new Sample1();
        sample.SampleMethod4();

    }
}
