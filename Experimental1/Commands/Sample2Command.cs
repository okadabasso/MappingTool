using System.Threading.Tasks;
using Experimental1.Samples;

namespace Experimental1.Commands;

public class Sample2Command
{
    public Sample2Command() { }

    public ValueTask ExecuteAsync(object? context = null)
    {
        var s = new Sample2();
        s.Run();
        return ValueTask.CompletedTask;
    }
}
