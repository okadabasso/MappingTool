using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using ConsoleApp1.Data;
using System.Security.AccessControl;

namespace ConsoleApp1.Shared
{
    public class SimpleMapper<TSource, TDestination>
    {
        private static ConcurrentDictionary<Type, Dictionary<string, Func<TSource, object>>> _getterCache = new ConcurrentDictionary<Type, Dictionary<string, Func<TSource, object>>>();
        private static ConcurrentDictionary<Type, Dictionary<string, Action<TDestination, object>>> _setterCache = new ConcurrentDictionary<Type, Dictionary<string, Action<TDestination, object>>>();

        private static ConcurrentDictionary<(Type, Type), List<Action<TSource, TDestination>>> _assignCache
            = new ConcurrentDictionary<(Type, Type), List<Action<TSource, TDestination>>>();
        
        private static Dictionary<string, Func<TSource, object>> _getters;
        private static Dictionary<string, Action<TDestination, object>> _setters;
        private static List<Action<TSource, TDestination>> _assigns;
        private static Action<TSource, TDestination> _assignAction;

        private static Func<TDestination> _destinationConstructor;
        private static Func<TSource, TDestination> _destinationInit;

        private static Type _sourceType = typeof(TSource);
        private static Type _destinationType = typeof(TDestination);

        static SimpleMapper()
        {
            // コンストラクタでキャッシュを初期化
            _getters = GetPropertyGetter(typeof(TSource));
            _setters = GetPropertySetter(typeof(TDestination));
            _assigns = GetPropertyAssign();
            _assignAction = CreatePropertyAssign2();
            _destinationConstructor = CreateDestinationConstructor();
            _destinationInit = CreateDestinationInit();
        }
        public SimpleMapper()
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
        public void Map2(TSource source, TDestination destination)
        {
            foreach (var getter in _getters)
            {
                var value = getter.Value(source);
                if (_setters.ContainsKey(getter.Key))
                {
                    var setter = _setters[getter.Key];
                    setter(destination, value);
                }
            }
        }
        public void Map3(TSource source, TDestination destination)
        {
            _assignAction(source, destination);
        }
        public TDestination Map4(TSource source)
        {
            var destination = _destinationConstructor();
            _assignAction(source, destination);
            return destination;
        }
        public TDestination Map5(TSource source)
        {
            var destination = _destinationInit(source);
            return destination;
        }
        public IEnumerable<TDestination> Map5(IEnumerable<TSource> source)
        {
            var list = new List<TDestination>(source.Count());
            foreach (var item in source)
            {
                var destination = _destinationInit(item);
                list.Add(destination);
            }
            
            return list;
        }
        private static Dictionary<string, Func<TSource, object>> GetPropertyGetter(Type sourceType)
        {
            return _getterCache.GetOrAdd(sourceType, type =>
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var map = new Dictionary<string, Func<TSource, object>>();
                foreach (var property in properties)
                {
                    var getter = CreatePropertyGetter(property);

                    map[property.Name] = getter;
                }
                return map;
            });
        }
        private static Dictionary<string, Action<TDestination, object>> GetPropertySetter(Type destinationType)
        {
            return _setterCache.GetOrAdd(destinationType, type =>
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var map = new Dictionary<string, Action<TDestination, object>>();
                foreach (var property in properties)
                {
                    if (property.CanWrite)
                    {
                        var setter = CreatePropertySetter(property);
                        map[property.Name] = setter;
                    }
                }
                return map;
            });
        }
        private static List<Action<TSource, TDestination>> GetPropertyAssign()
        {
            return _assignCache.GetOrAdd((_sourceType, _destinationType), types =>
            {
                var properties = types.Item1.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var map = new List<Action<TSource, TDestination>>();
                foreach (var sourceProperty in properties)
                {
                    var destinationProperty = types.Item2.GetProperty(sourceProperty.Name);
                    if(destinationProperty != null && destinationProperty.CanWrite)
                    {
                        var assign = CreatePropertyAssign(sourceProperty, destinationProperty);
                        map.Add(assign);
                    }
                }
                return map;
            });
        }
        private static Func<TSource, object> CreatePropertyGetter(PropertyInfo property)
        {

            var parameter = Expression.Parameter(_sourceType, "source");
            var propertyAccess = Expression.Convert(Expression.Property(parameter, property), typeof(object));
            var lambda = Expression.Lambda<Func<TSource, object>>(propertyAccess, parameter);

            return lambda.Compile();
        }
        private static Action<TDestination, object> CreatePropertySetter(PropertyInfo property)
        {

            var targetType = _destinationType;

            // パラメータ: (TDestination target, object value)
            var destinationParameter = Expression.Parameter(_destinationType, "target");
            var valueParameter = Expression.Parameter(typeof(object), "value");


            // target.Property = (propertyType)value
            var assign = Expression.Assign(Expression.Property(destinationParameter, property), Expression.Convert(valueParameter, property.PropertyType));

            // ラムダ式を作成: (target, value) => target.Property = (propertyType)value;
            var lambda = Expression.Lambda<Action<TDestination, object>>(assign, destinationParameter, valueParameter);

            return lambda.Compile();
        }

        private static Action<TSource, TDestination> CreatePropertyAssign(PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        {
            
            var source = Expression.Parameter(typeof(TSource), "source");
            var sourceAccess = Expression.Property(source, sourceProperty);

            var destination = Expression.Parameter(_destinationType, "destination");
            
            var assign = Expression.Assign(Expression.Property(destination, destinationProperty), sourceAccess);

            var lambda = Expression.Lambda<Action<TSource, TDestination>>(assign, source, destination);

            return lambda.Compile();
        }
        private static Action<TSource, TDestination> CreatePropertyAssign2()
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
        private static Func<TDestination> CreateDestinationConstructor()
        {
            if (_destinationConstructor == null)
            {
                var constructor = _destinationType.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new InvalidOperationException($"Type {typeof(TDestination).Name} does not have a parameterless constructor.");
                }
                var newExpression = Expression.New(constructor);
                var lambda = Expression.Lambda<Func<TDestination>>(newExpression);
                _destinationConstructor = lambda.Compile();
            }
            return _destinationConstructor;
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
