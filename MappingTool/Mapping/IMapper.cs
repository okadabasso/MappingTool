using System.Collections.Generic;

namespace MappingTool.Mapping;
public interface IMapper<TSource, TDestination>
    where TSource : notnull
    where TDestination : notnull
{
    TDestination Map(TSource source);
    IEnumerable<TDestination> Map(IEnumerable<TSource> source);
    void Map(TSource source, TDestination destination);
}
