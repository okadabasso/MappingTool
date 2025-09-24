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


    private Action<MappingContext, TSource, TDestination> _propertyAssign = null!;
    public Func<MappingContext, TSource, TDestination> _objectInitializer = null!;
    private readonly bool _preserveReferences;

        private static Type _sourceType = typeof(TSource);
        private static Type _destinationType = typeof(TDestination);

        internal SimpleMapper(Func<MappingContext, TSource, TDestination> objectInitializer, Action<MappingContext, TSource, TDestination> propertyAssign, bool preserveReferences = false)
        {
            _objectInitializer = objectInitializer;
            _propertyAssign = propertyAssign;
            _preserveReferences = preserveReferences;
        }
        public void Map(TSource source, TDestination destination)
        {
            if (source == null || destination == null)
            {
                throw new ArgumentNullException("Source or destination cannot be null.");
            }
            var context = new MappingContext();
            if (_preserveReferences)
            {
                context.EnablePreserveReferences();
                context.SetMappedDestination(source, destination);
            }
            _propertyAssign(context, source, destination);
        }
        public TDestination Map(TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source cannot be null.");
            }
            var context = new MappingContext();
            if (_preserveReferences) context.EnablePreserveReferences();
            else context.MarkAsMapped(source);
            return _objectInitializer(context, source);
        }
        public IEnumerable<TDestination> Map(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source cannot be null.");
            }
            var context = new MappingContext();
            if (_preserveReferences) context.EnablePreserveReferences();
            var list = source.Select(item => _objectInitializer(context, item)).ToList();
            return list;
        }


    }
}
