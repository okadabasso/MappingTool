using Microsoft.Extensions.Logging;
using  ConsoleAppFramework;
using Experimental1.Data;
using System.Linq.Expressions;
using System.Reflection;
using MappingTool.Mapping;
namespace Experimental1.Commands;

[ConsoleAppFramework.RegisterCommands("expression-compose")]
public class ExpressionComposeCommand
{
    private readonly ILogger<ExpressionComposeCommand> _logger;

    public ExpressionComposeCommand(ILogger<ExpressionComposeCommand> logger)
    {
        _logger = logger;
    }

    [Command("execute")]
    public void Execute()
    {
        var factory = new MapperFactory<SourceData, DestinationData>();
        Expression<Func<IMapper<SourceData, DestinationData>>> expression = () => factory.CreateMapper();
        Console.WriteLine(expression);

        var type = typeof(MapperFactory<,>);
        var genericType = type.MakeGenericType(typeof(SourceData), typeof(DestinationData));
        var constructor = genericType.GetConstructor(new Type[] { typeof(bool), typeof(int), typeof(ILogger), typeof(bool) });
        if (constructor == null)
        {
            throw new InvalidOperationException($"Constructor with specified parameters not found in type {genericType.Name}.");
        }
        var newExpression = Expression.New(constructor,
            Expression.Constant(true),          // allowRecursion
            Expression.Constant(5),             // maxDepth
            Expression.Constant(_logger),       // logger
            Expression.Constant(false)         //   preserveReference
        );
        var method = genericType.GetMethod("CreateMapper", BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
        {
            throw new InvalidOperationException($"Method CreateMapper not found in type {genericType.Name}.");
        }
        var methodCall = Expression.Call(newExpression,
            method);
        var lambda = Expression.Lambda<Func<object>>(
            Expression.Convert(methodCall, typeof(IMapper<SourceData, DestinationData>))
        );
        Console.WriteLine(lambda);

    }
}