using System.Linq.Expressions;

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
public class MapperConfiguration<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    public void CreateMap<TSourceMember, TDestinationMember>(Expression<Func<TSource, TSourceMember>> sourceMember,
        Expression<Func<TDestination, TDestinationMember>> destinationMember)
    {
        // Implementation for creating a map between source and destination members
    }
}
