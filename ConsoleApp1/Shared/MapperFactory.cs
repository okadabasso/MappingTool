using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace ConsoleApp1.Shared;

public class MapperFactory<TSource, TDestination>
    where TSource : class
    where TDestination : class
{

    public SimpleMapper<TSource, TDestination> CreateMapper()
    {
        var sourceType = typeof(TSource);
        var destinationType = typeof(TDestination);


        // Create a new instance of the SimpleMapper class
        return new SimpleMapper<TSource, TDestination>();
    }
}
public class MapperFactory
{
    private static readonly ConcurrentDictionary<(Type, Type), ISimpleMapper> _mapperCache = new();

    public SimpleMapper<TSource, TDestination> CreateMapper<TSource, TDestination>()
        where TSource : notnull
        where TDestination : notnull
    {
        var mapper =  _mapperCache.GetOrAdd((typeof(TSource), typeof(TDestination)), (key) =>
        {
            var sourceType = key.Item1;
            var destinationType = key.Item2;

            // Create a new instance of the SimpleMapper class
            return new SimpleMapper<TSource, TDestination>();
        });
        // Check if the mapper already exists in the cache
        return (SimpleMapper<TSource, TDestination>)mapper;
    }
}
