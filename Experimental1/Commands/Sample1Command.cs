using System.Threading.Tasks;
using Experimental1.Samples;

namespace Experimental1.Commands;

// Lightweight stub command (no dependency on ConsoleAppFramework)
public class Sample1Command
{
    public Sample1Command()
    {
    }

    public ValueTask ExecuteAsync(object? context = null)
    {
        var s = new Sample1();
        s.Run();
        return ValueTask.CompletedTask;
    }
}
