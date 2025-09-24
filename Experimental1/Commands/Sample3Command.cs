using System.Threading.Tasks;
using Experimental1.Samples;

namespace Experimental1.Commands;

public class Sample3Command
{
    public Sample3Command() { }

    public ValueTask ExecuteAsync(object? context = null)
    {
        var s = new Sample3();
        s.Run();
        return ValueTask.CompletedTask;
    }
}
