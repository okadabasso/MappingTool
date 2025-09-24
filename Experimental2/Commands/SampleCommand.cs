using MappingTool.Mapping;
using ConsoleAppFramework;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;
namespace Experimental2.Commands;

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
        var mapper = new MapperFactory<SourceData, DestinationData>(allowRecursion: true).CreateMapper();
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
        var mapper = new MapperFactory<SourceData, DestinationData>(allowRecursion: true).CreateMapper();
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

        var context = new MappingContext();
        Func<MappingContext, SourceData, DestinationData> mapDestination = null!;
        mapDestination = (ctx, src) =>
        {
            if (src == null) return null!;
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
    [Command("method5")]
    public void Execute5()
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
        // 循環参照オブジェクト
        source.Detail.Parent = source; // Create circular reference
        var mapper = new MapperFactory<SourceData, DestinationData>(allowRecursion: true).CreateMapper();
        if (mapper == null)
        {
            Console.WriteLine("Mapper is null");
            return;
        }
        var destination = mapper.Map(source);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
        if (destination.Detail != null)
        {
            _logger.LogInformation("Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Id, destination.Detail.Name);
            if (destination.Detail.Parent != null)
            {
                _logger.LogInformation("Deep Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Parent.Id, destination.Detail.Parent.Name);
            }
            else
            {
                _logger.LogInformation("Deep Nested Mapping: Parent is null");
            }
        }

    }
    [Command("method6")]
    public void Execute6()
    {
        var source = new SourceRecord(1, "Source", new NestedSourceRecord(2, "Nested Source", null!));

        // 循環参照オブジェクト
        source = source with { Detail = source.Detail with { Parent = source } };
        var mapper = new MapperFactory<SourceRecord, DestinationRecord>(allowRecursion: true).CreateMapper();
        if (mapper == null)
        {
            Console.WriteLine("Mapper is null");
            return;
        }
        var destination = mapper.Map(source);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
        if (destination.Detail != null)
        {
            _logger.LogInformation("Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Id, destination.Detail.Name);
            if (destination.Detail.Parent != null)
            {
                _logger.LogInformation("Deep Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Parent.Id, destination.Detail.Parent.Name);
            }
            else
            {
                _logger.LogInformation("Deep Nested Mapping: Parent is null");
            }
        }

    }
    [Command("method7")]
    public void Execute7()
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
            },
            Details = new List<NestedSource>
            {
                new NestedSource { Id = 3, Name = "List Item 1", Parent = null! },
                new NestedSource { Id = 4, Name = "List Item 2", Parent = null! },
                new NestedSource { Id = 5, Name = "List Item 3", Parent = null! },

            }
        };
        // 循環参照オブジェクト
        source.Detail.Parent = source; // Create circular reference
        var mapper = new MapperFactory<SourceData, DestinationData>(allowRecursion: true).CreateMapper();
        if (mapper == null)
        {
            Console.WriteLine("Mapper is null");
            return;
        }
        var destination = mapper.Map(source);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
        if (destination.Detail != null)
        {
            _logger.LogInformation("Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Id, destination.Detail.Name);
            if (destination.Detail.Parent != null)
            {
                _logger.LogInformation("Deep Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Parent.Id, destination.Detail.Parent.Name);
            }
            else
            {
                _logger.LogInformation("Deep Nested Mapping: Parent is null");
            }
            if (destination.Details != null)
            {
                foreach (var item in destination.Details)
                {
                    _logger.LogInformation("List Item: Id={Id}, Name={Name}", item.Id, item.Name);
                }
            }
        }

    }
    [Command("method8")]
    public void Execute8()
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
            },
            Details = new List<NestedSource>
            {
                new NestedSource { Id = 3, Name = "List Item 1", Parent = null! },
                new NestedSource { Id = 4, Name = "List Item 2", Parent = null! },
                new NestedSource { Id = 5, Name = "List Item 3", Parent = null! },

            },
            IntValues = new int[] { 1, 2, 3, 4, 5 }
        };
        // 循環参照オブジェクト
        source.Detail.Parent = source; // Create circular reference
        var mapper = new MapperFactory<SourceData, DestinationData>(allowRecursion: true, logger: _logger).CreateMapper();
        if (mapper == null)
        {
            Console.WriteLine("Mapper is null");
            return;
        }
        var destination = mapper.Map(source);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}, IntValues={IntValues}", destination.Id, destination.Name, string.Join(", ", destination.IntValues));
        if (destination.Detail != null)
        {
            _logger.LogInformation("Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Id, destination.Detail.Name);
            if (destination.Detail.Parent != null)
            {
                _logger.LogInformation("Deep Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Parent.Id, destination.Detail.Parent.Name);
            }
            else
            {
                _logger.LogInformation("Deep Nested Mapping: Parent is null");
            }
            if (destination.Details != null)
            {
                foreach (var item in destination.Details)
                {
                    _logger.LogInformation("List Item: Id={Id}, Name={Name}", item.Id, item.Name);
                }
            }
        }

    }
    [Command("method9")]
    public void Execute9()
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
            },
            Details = new List<NestedSource>
            {
                new NestedSource { Id = 3, Name = "List Item 1", Parent = null! },
                new NestedSource { Id = 4, Name = "List Item 2", Parent = null! },
                new NestedSource { Id = 5, Name = "List Item 3", Parent = null! },

            },
            IntValues = new int[] { 1, 2, 3, 4, 5 }
        };
        // 循環参照オブジェクト
        source.Detail.Parent = source; // Create circular reference
        var destination = new DestinationData(source);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}, IntValues={IntValues}", destination.Id, destination.Name, string.Join(", ", destination.IntValues));
        if (destination.Detail != null)
        {
            _logger.LogInformation("Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Id, destination.Detail.Name);
            if (destination.Detail.Parent != null)
            {
                _logger.LogInformation("Deep Nested Mapping: Id={Id}, Name={Name}", destination.Detail.Parent.Id, destination.Detail.Parent.Name);
            }
            else
            {
                _logger.LogInformation("Deep Nested Mapping: Parent is null");
            }
            if (destination.Details != null)
            {
                foreach (var item in destination.Details)
                {
                    _logger.LogInformation("List Item: Id={Id}, Name={Name}", item.Id, item.Name);
                }
            }
        }

    }
    [Command("method10")]
    public void Execute10()
    {
        var source = new SourceStruct
        {
            Id = 1,
            Name = "Source",
        };
        var mapper = new MapperFactory<SourceStruct, DestinationStruct>(logger: _logger).CreateMapper();
        var destination = mapper.Map(source);

        _logger.LogInformation("Mapping completed: Id={Id}, Name={Name}", destination.Id, destination.Name);
    }


    public void DebugView(Expression expr)
    {
        // DebugView プロパティをリフレクションで取得
        var debugViewProp = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
        string debugView = (string)(debugViewProp?.GetValue(expr) ?? "");

        _logger.LogDebug(debugView);
    }
    class SourceData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public NestedSource? Detail { get; set; } = null;
        public List<NestedSource> Details { get; set; } = new List<NestedSource>();
        public int[] IntValues { get; set; } = Array.Empty<int>();
    }
    class DestinationData
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public NestedDestination? Detail { get; set; } = null;
        public List<NestedDestination> Details { get; set; } = new List<NestedDestination>();
        public int[] IntValues { get; set; } = Array.Empty<int>();
        public DestinationData()
        {

        }
        public DestinationData(SourceData source, ILogger? logger = null)
        {
            var mapper = new MapperFactory<SourceData, DestinationData>(allowRecursion: true, logger: logger).CreateMapper();
            mapper.Map(source, this);
        }
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
    record SourceRecord(int Id, string Name, NestedSourceRecord Detail);
    record NestedSourceRecord(int Id, string Name, SourceRecord Parent);
    record DestinationRecord(int Id, string Name, NestedDestinationRecord Detail);
    record NestedDestinationRecord(int Id, string Name, DestinationRecord Parent);

    public struct SourceStruct
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SourceStruct()
        {
            Id = 0;
            Name = "";   
        }
    }
    public struct DestinationStruct
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DestinationStruct()
        {
            Id = 0;
            Name = "";   
        }
    }
}
