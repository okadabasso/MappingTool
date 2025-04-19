namespace MappingTool.Mapping
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ReflectionMapper<TSource, TDestination>
        where TSource : notnull
        where TDestination : notnull, new()
    {

        private readonly Type _sourceType = typeof(TSource);
        private readonly Type _destinationType = typeof(TDestination);

        public ReflectionMapper()
        {
        }
        public void Map(TSource source, TDestination destination)
        {

            foreach (var sourceProperty in _sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = sourceProperty.GetValue(source, null);
                if (value != null)
                {
                    var destinationProperty = _destinationType.GetProperty(sourceProperty.Name);
                    if (destinationProperty != null && destinationProperty.CanWrite)
                    {
                        destinationProperty.SetValue(destination, value, null);
                    }
                }
            }
        }
        public TDestination Map(TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var destination = new TDestination();
            foreach (var sourceProperty in _sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = sourceProperty.GetValue(source, null);
                if (value != null)
                {
                    var destinationProperty = _destinationType.GetProperty(sourceProperty.Name);
                    if (destinationProperty != null && destinationProperty.CanWrite)
                    {
                        destinationProperty.SetValue(destination, value, null);
                    }
                }
            }
            return destination;
        }

    }
}