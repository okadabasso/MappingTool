using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Experimental1.Samples;

namespace Experimental1.Benchmarks;

public class MappingBenchmark
{
    [Benchmark]
    public void SampleMapping()
    {
        var s = new Sample1();
        s.Run();
    }
}
