namespace Experimental1.Samples;

using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using Experimental1.Data;
using Experimental1.Shared;

public class Sample1
{
    public void SampleMethod1()
    {
        var source = new List<SourceData>(){
            new SourceData { Id = 1, Name = "Test1" },
            new SourceData { Id = 2, Name = "Test2" },
            new SourceData { Id = 3, Name = "Test3" }
        };

        var mapper = MapperFactory.CreateMapper<SourceData, DestinationData>();
        var destination = mapper.Map5(source);
        foreach (var item in destination)
        {
            Console.WriteLine($"Id: {item.Id}, Name: {item.Name}");
        }
    }
    public void SampleMethod2()
    {
        try
        {
            var source = new SourceData { Id = 1, Name = "Test", Description = "This is a test." };
            var mapper = new SimpleMapper<SourceData, Foo>();
            var destination = mapper.Map5(source);
            Console.WriteLine($"Id: {destination.Id}, Name: {destination.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public void SampleMethod3()
    {
        var type = typeof(SourceData);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            Console.WriteLine($"Property: {prop.Name}, Type: {prop.PropertyType.Name}, CanRead: {prop.CanRead}, CanWrite: {prop.CanWrite}");
        }

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            Console.WriteLine($"Field: {field.Name}, Type: {field.FieldType.Name}");
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            Console.WriteLine($"Method: {method.Name}, Return Type: {method.ReturnType.Name}");
            var parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                Console.WriteLine($"\tParameter: {param.Name}, Type: {param.ParameterType.Name}");
            }
        }
    }
}
