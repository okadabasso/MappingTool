namespace ConsoleApp1.Commands;

using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

[ConsoleAppFramework.RegisterCommands()]
public class SampleCommand
{
    private readonly ILogger<SampleCommand> _logger;
    public SampleCommand(ILogger<SampleCommand> logger)
    {
        _logger = logger;
    }

    [Command("sample")]
    public void Execute()
    {
        _logger.LogInformation("Sample command executed.");
        // Command implementation
    }
}
