
using BenchmarkDotNet.Running;
using ConsoleApp1.Benchmarks;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;
using System.Numerics;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Reflection;

ConstructorCheck();

void ConstructorCheck(){
    Console.WriteLine("ConstructorCheck All Constructors");
    var t = typeof(DestinationRecord);
    var constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

    foreach (var constructor in constructors)
    {
        Console.WriteLine("constructor: " + constructor.Name);
        var parameters = constructor.GetParameters();
        foreach (var parameter in parameters)
        {
            Console.WriteLine($"Parameter: {parameter.Name}, Type: {parameter.ParameterType}");
        }
    }

    Console.WriteLine("ConstructorCheck Primary Constructor");
    var primaryConstructor = GetPrimaryConstructor();
    if (primaryConstructor != null)
    {
        Console.WriteLine($"Primary Constructor: {primaryConstructor.Name}");
        var parameters = primaryConstructor.GetParameters();
        foreach (var parameter in parameters)
        {
            Console.WriteLine($"Parameter: {parameter.Name}, Type: {parameter.ParameterType}");
        }
    }
    else
    {
        Console.WriteLine("No primary constructor found.");
    }

}
ConstructorInfo GetPrimaryConstructor(){
    var t = typeof(DestinationRecord);
     // すべてのパブリックなインスタンスコンストラクターを取得
    var constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

    // 主コンストラクターを特定
    var primaryConstructor = constructors.FirstOrDefault(c =>
    {
        var parameters = c.GetParameters();
        // 主コンストラクターの条件: パラメーターの数がプロパティの数と一致
        return parameters.Length == t.GetProperties().Length &&
               parameters.All(p => t.GetProperty(p.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null);
    });
    return primaryConstructor;
}
Func<SourceRecord, DestinationRecord> CreateDestinationInit()
{
    var t = typeof(DestinationRecord);
    var constructor = GetPrimaryConstructor();
    if (constructor == null)
    {
        throw new InvalidOperationException("No primary constructor found.");
    }
    var sourceType = typeof(SourceRecord);
    var sourceParameter = Expression.Parameter(typeof(SourceRecord), "source");
    var parameters = constructor.GetParameters();
    var parameterExpressions = new ParameterExpression[parameters.Length];
    var arguments = new Expression[parameters.Length];
    for (int i = 0; i < parameters.Length; i++)
    {
        var sourceProperty = sourceType.GetProperty(parameters[i].Name, BindingFlags.Public | BindingFlags.Instance);
        if (sourceProperty == null)
        {
            var d = Expression.Default(parameters[i].ParameterType);
            arguments[i] = Expression.Default(parameters[i].ParameterType);
        }
        else{
            arguments[i] = Expression.Property(sourceParameter, sourceProperty);
        }
    }
    var newExpression = Expression.New(constructor, arguments);

    var lambda = Expression.Lambda<Func<SourceRecord, DestinationRecord>>(newExpression, sourceParameter);
    return lambda.Compile();
}
Expression<Func<SourceRecord, DestinationRecord>> a = source => new DestinationRecord(source.Id, source.Name);

Console.WriteLine("a: ");
Console.WriteLine(a);

var source = new SourceRecord(1, "Test");
var init = CreateDestinationInit();
var destination = init(source);
Console.WriteLine("destination: ");
Console.WriteLine(destination);