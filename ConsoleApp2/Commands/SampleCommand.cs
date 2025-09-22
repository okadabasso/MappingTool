using MappingTool.Mapping;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;
namespace ConsoleApp2.Commands;

[ConsoleAppFramework.RegisterCommands("sample")]
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
        var source = new SourceData { Id = 1, Name = "Source" };
        var destination = new DestinationData();

        var context = new MappingContext();
        var mapper = new MapperFactory<SourceData, DestinationData>().CreateMapper();
        mapper.Map(source, destination);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
    }

    [Command("method2")]
    public void Execute2()
    {
        var source = new SourceData
        {
            Id = 1,
            Name = "Source",
            Detail = new NestedSource
            {
                Id = 2,
                Name = "Nested Source",
                Parent = null!
            }
        };
        source.Detail.Parent = source; // Create circular reference
        var context = new MappingContext();
        var mapper = new MapperFactory<SourceData, DestinationData>().CreateMapper();
        var destination = mapper.Map(source);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
        if (destination.Detail != null)
        {
            _logger.LogInformation("Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Id, destination.Detail.Name);
            if (destination.Detail.Parent != null)
            {
                _logger.LogInformation("Deep Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Parent.Id, destination.Detail.Parent.Name);
            }
        }
    }
    [Command("method3")]
    public void Execute3()
    {
        var source = new SourceData
        {
            Id = 1,
            Name = "Source",
            Detail = new NestedSource
            {
                Id = 2,
                Name = "Nested Source",
                Parent = null!
            }
        };
        source.Detail.Parent = source; // Create circular reference

        var context = new MappingContext { MaxRecursionDepth = 2 };
        Func<MappingContext, SourceData, DestinationData> mapDestination = null!;
        mapDestination = (ctx, src) =>
        {
            if (src == null) return null!;
            ctx.EnterRecursion();
            var dest = new DestinationData
            {
                Id = src.Id,
                Name = src.Name,
                Detail = (src.Detail != null && !ctx.IsMapped(src.Detail))
                    ? new NestedDestination
                    {
                        Id = src.Detail.Id,
                        Name = src.Detail.Name,
                        Parent = (src.Detail != null && !ctx.IsMapped(src.Detail))
                            ? new DestinationData
                            {
                                Id = src.Detail.Parent.Id,
                                Name = src.Detail.Parent.Name,
                                Detail = null!
                            }
                            : null!
                    }
                    : null!
            };
            ctx.MappedObjects.Add(src);
            ctx.ExitRecursion();
            return dest;
        };
        var lambda = (Func<SourceData, DestinationData>)(src => mapDestination(context, src));
        var destination = lambda(source);

        Console.WriteLine(lambda);
        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
        if (destination.Detail != null)
        {
            _logger.LogInformation("Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Id, destination.Detail.Name);
            if (destination.Detail.Parent != null)
            {
                _logger.LogInformation("Deep Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Parent.Id, destination.Detail.Parent.Name);
            }
        }

    }
    [Command("method4")]
    public void Execute4()
    {
        // mapping context ですでにマップされているなら null を返す expressionを構築する
        var destinationType = typeof(DestinationData);
        var sourceParameter = Expression.Parameter(typeof(object), "source");
        var contextParameter = Expression.Parameter(typeof(MappingContext), "context");

        var isMapped = typeof(MappingContext).GetMethod("IsMapped")!;
        var markAsMapped = typeof(MappingContext).GetMethod("MarkAsMapped")!;
        var checkMapped = Expression.Call(
            contextParameter,
            isMapped,
                Expression.Convert(sourceParameter, typeof(object)) // 明示的に object 型に変換
        );

        var ifelse = Expression.Condition(
            checkMapped,
            Expression.Constant(null, destinationType), // 型を明示
            Expression.Block(
                Expression.Call(contextParameter, markAsMapped, sourceParameter),
                Expression.New(typeof(DestinationData).GetConstructor(Type.EmptyTypes)!)
       
            )
        );

        var expr3 = Expression.Lambda<Func<MappingContext, object, DestinationData>>(
            Expression.Block(ifelse),
            contextParameter,
            sourceParameter
        );

        Console.WriteLine(expr3);
        DebugView(expr3);

        var source = new SourceData { Id = 1, Name = "Source" };
        var context = new MappingContext();
        var destination = expr3.Compile().Invoke(context, source);
        Console.WriteLine($"{destination.Id}, {destination.Name}");

    }
    private void DebugView(Expression expr)
    {
        // DebugView プロパティをリフレクションで取得
        var debugViewProp = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
        string debugView = (string)debugViewProp.GetValue(expr);

        Console.WriteLine(debugView);
    }
    class SourceData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public NestedSource? Detail { get; set; } = null;
    }
    class DestinationData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public NestedDestination? Detail { get; set; } = null;
    }
    class NestedSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public SourceData Parent { get; set; } = null!;
    }
    class NestedDestination
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DestinationData Parent { get; set; } = null!;
    }
}
