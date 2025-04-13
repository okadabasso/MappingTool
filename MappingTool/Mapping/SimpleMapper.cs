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
    public class SimpleMapper<TSource, TDestination>
    {
        private static Action<TSource, TDestination> _assignAction;
        private static Func<TSource, TDestination> _destinationInit;

        private static Type _sourceType = typeof(TSource);
        private static Type _destinationType = typeof(TDestination);

        static SimpleMapper()
        {
            // コンストラクタでキャッシュを初期化
            _assignAction = CreatePropertyAssign();
            _destinationInit = CreateDestinationInit();
        }
        public SimpleMapper()
        {
        }

        public void Map(TSource source, TDestination destination)
        {
            _assignAction(source, destination);
        }
        public TDestination Map(TSource source)
        {
            var destination = _destinationInit(source);
            return destination;
        }
        public TDestination MapStruct(TSource source)
        {
            var destination = _destinationInit(source);
            return destination;
        }
        public TDestination MapRecord(TSource source)
        {
            var destination = _destinationInit(source);
            return destination;
        }
        public IEnumerable<TDestination> Map(IEnumerable<TSource> source)
        {
            var list = new List<TDestination>(source.Count());
            foreach (var item in source)
            {
                var destination = _destinationInit(item);
                list.Add(destination);
            }
            
            return list;
        }

        private static Action<TSource, TDestination> CreatePropertyAssign()
        {

            var expressionList = new List<Expression>();

            var source = Expression.Parameter(_sourceType, "source");
            var destination = Expression.Parameter(_destinationType, "destination");

            foreach (var sourceProperty in _sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var destinationProperty = _destinationType.GetProperty(sourceProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                if (destinationProperty != null && destinationProperty.CanWrite)
                {
                    var sourceAccess = Expression.Property(source, sourceProperty);
                    var destinationAccess = Expression.Property(destination, destinationProperty);
                    var assign = Expression.Assign(destinationAccess, sourceAccess);

                    expressionList.Add(assign);
                }
            }

            var block = Expression.Block(expressionList);
            var lambda = Expression.Lambda<Action<TSource, TDestination>>(block, source, destination);

            return lambda.Compile();
        }
        private static Func<TSource, TDestination> CreateDestinationInit()
        {
            var constructor = _destinationType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {typeof(TDestination).Name} does not have a parameterless constructor.");
            }
            var newExpression = Expression.New(constructor);
            var source = Expression.Parameter(typeof(TSource), "source");

            var memberBindings = new List<MemberBinding>();
            foreach (var sourceProperty in typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var destinationProperty = typeof(TDestination).GetProperty(sourceProperty.Name);
                if (destinationProperty != null && destinationProperty.CanWrite)
                {
                    var sourceAccess = Expression.Property(source, sourceProperty);
                    var binding = Expression.Bind(destinationProperty, sourceAccess);
                    memberBindings.Add(binding);
                }

            }
            var memberInit = Expression.MemberInit(newExpression, memberBindings);
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(memberInit, source);

            return lambda.Compile();
        }
    }
}
