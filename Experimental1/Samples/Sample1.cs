namespace Experimental1.Samples;

using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using Experimental1.Data;
using Microsoft.Extensions.Logging;
using MappingTool.Mapping;
public class Sample1
{
    private readonly ILogger<Sample1> _logger;

    public Sample1(ILogger<Sample1> logger)
    {
        _logger = logger;
    }
    public void MapList()
    {
        var source = new List<SourceData>(){
            new SourceData { Id = 1, Name = "Test1" },
            new SourceData { Id = 2, Name = "Test2" },
            new SourceData { Id = 3, Name = "Test3" }
        };

        var mapper  = new MapperFactory<SourceData, DestinationData>().CreateMapper();
        var destination = mapper.Map(source);
        foreach (var item in destination)
        {
            Console.WriteLine($"Id: {item.Id}, Name: {item.Name}");
        }
    }
    public void MapSingleObject()
    {
        try
        {
            var source = new SourceData { Id = 1, Name = "Test", Description = "This is a test." };
            var mapper = new MapperFactory<SourceData, DestinationData>().CreateMapper();
            var destination = mapper.Map(source);
            Console.WriteLine($"Id: {destination.Id}, Name: {destination.Name} website: {destination.Website}"); // Website should be default Uri
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
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
