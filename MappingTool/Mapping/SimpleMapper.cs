using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Security.AccessControl;

namespace MappingTool.Mapping
{
    public class SimpleMapper<TSource, TDestination> : IMapper<TSource, TDestination>
        where TSource : notnull
        where TDestination : notnull
    {


        private Action<TSource, TDestination> _propertyAssign = null!;
        private Func<TSource, TDestination> _objectInitializer = null!;

        private static Type _sourceType = typeof(TSource);
        private static Type _destinationType = typeof(TDestination);

        internal SimpleMapper(Func<TSource, TDestination> objectInitializer, Action<TSource, TDestination> propertyAssign)
        {
            _objectInitializer = objectInitializer;
            _propertyAssign = propertyAssign;
        }
        public void Map(TSource source, TDestination destination)
        {
            if (source == null || destination == null)
            {
                throw new ArgumentNullException("Source or destination cannot be null.");
            }
            _propertyAssign(source, destination);
        }
        public TDestination Map(TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source cannot be null.");
            }
            return _objectInitializer(source);
        }
        public IEnumerable<TDestination> Map(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source cannot be null.");
            }

            var list = source.Select(item => _objectInitializer(item)).ToList();
            return list;
        }


    }
}
