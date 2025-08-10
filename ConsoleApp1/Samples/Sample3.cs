using System.Linq.Expressions;
using System.Reflection;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;

namespace ConsoleApp1.Samples;

class Sample3
{
    public void SampleMethod()
    {
        var type = typeof(MapperFactory);
        var method = type.GetMethod("CreateMapper", BindingFlags.Public | BindingFlags.Static);
        if (method == null)
        {
            throw new InvalidOperationException($"Method CreateMapper not found in type {type.Name}.");
        }
        var genericMethod = method.MakeGenericMethod(typeof(SourceData), typeof(DestinationData));
        var methodCall = Expression.Call(genericMethod);

        var listType = typeof(List<>).MakeGenericType(typeof(SourceData));
        var destinationType = typeof(List<>).MakeGenericType(typeof(DestinationData));
        var sourceParameter = Expression.Parameter(listType, "source");
        var mapMethod = typeof(SimpleMapper<SourceData, DestinationData>).GetMethod(
            "Map5", BindingFlags.Public | BindingFlags.Instance, new Type[]{ listType});
        if (mapMethod == null)
        {
            throw new InvalidOperationException($"Method Map not found in type {typeof(SimpleMapper<SourceData, DestinationData>).Name}.");
        }
        var mapMethodCall = Expression.Call(
            Expression.Convert(methodCall, typeof(SimpleMapper<SourceData, DestinationData>)), 
            mapMethod, 
            sourceParameter);

        var lambda2 = Expression.Lambda<Func<List<SourceData>, List<DestinationData>>>(
            Expression.Convert(mapMethodCall, destinationType), 
            sourceParameter);
        var map = lambda2.Compile();
        Console.WriteLine(lambda2);

        var source = new List<SourceData>
        {
            new SourceData { Id = 1, Name = "Test" },
            new SourceData { Id = 2, Name = "Sample" }
        };
        var destination = map(source);
        foreach (var item in destination)
        {
            Console.WriteLine($"Id: {item.Id}, Name: {item.Name}");
        }
    }
}