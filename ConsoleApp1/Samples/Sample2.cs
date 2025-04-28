namespace ConsoleApp1.Samples;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
class Sample2
{

    public void SampleMethod()
    {
        var factory = new MapperFactory<SourceData, DestinationData>();
        Expression<Func<SimpleMapper<SourceData, DestinationData>>> expression = () => factory.CreateMapper();
        Console.WriteLine(expression);

        var type = typeof(MapperFactory<,>);
        var genericType = type.MakeGenericType(typeof(SourceData), typeof(DestinationData));
        var newExpression = Expression.New(genericType);
        var method = genericType.GetMethod("CreateMapper", BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
        {
            throw new InvalidOperationException($"Method CreateMapper not found in type {genericType.Name}.");
        }
        var methodCall = Expression.Call(newExpression, method);
        var lambda = Expression.Lambda<Func<object>>(Expression.Convert(methodCall, typeof(SimpleMapper<SourceData, DestinationData>)));
        var compiledLambda = lambda.Compile();
        Console.WriteLine(lambda);

        var source = new SourceData { Id = 1, Name = "Test" };
        var destination = new DestinationData();
        var mapper = (SimpleMapper<SourceData, DestinationData>)compiledLambda.Invoke();
        mapper.Map(source, destination);
        Console.WriteLine($"Id: {destination.Id}, Name: {destination.Name}");
    }
}