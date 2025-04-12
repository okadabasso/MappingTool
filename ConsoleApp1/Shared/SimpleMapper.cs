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
        
        Dictionary<string, Func<TSource, object>> _getters;
        Dictionary<string, Action<TDestination, object>> _setters;
        List<Action<TSource, TDestination>> _assigns;

        private static Func<TDestination> _destinationConstructor;
        private static Func<TSource, TDestination> _destinationInit;

        private static Type sourceType = typeof(TSource);
        private static Type destinationType = typeof(TDestination);

        static SimpleMapper()
        {
            // コンストラクタでキャッシュを初期化
            var getters = GetPropertyGetter(typeof(TSource));
            var setters = GetPropertySetter(typeof(TDestination));
            var assigns = GetPropertyAssign();

            _destinationConstructor = CreateDestinationConstructor();
            _destinationInit = CreateDestinationInit();
        }
        public SimpleMapper()
        {
        }

        public void Map(TSource source, TDestination destination)
        {

            foreach (var sourceProperty in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = sourceProperty.GetValue(source, null);
                if (value != null)
                {
                    var destinationProperty = destinationType.GetProperty(sourceProperty.Name);
                    if (destinationProperty != null)
                    {
                        destinationProperty.SetValue(destination, value, null);
                    }
                }
            }
        }
        public void Map2(TSource source, TDestination destination)
        {
            var getters = GetPropertyGetter(sourceType);
            var setters = GetPropertySetter(destinationType);

            foreach (var getter in getters)
            {
                var value = getter.Value(source);
                if (setters.ContainsKey(getter.Key))
                {
                    var setter = setters[getter.Key];
                    setter(destination, value);
                }
            }
        }
        public void Map3(TSource source, TDestination destination)
        {
            var assigns = GetPropertyAssign();

            foreach (var assign in assigns)
            {
                assign(source, destination);
            }
        }
        public TDestination Map4(TSource source)
        {
            var destination = _destinationConstructor();
            var assigns = GetPropertyAssign();

            foreach (var assign in assigns)
            {
                assign(source, destination);
            }
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
                    var setter = CreatePropertySetter(property);

                    map[property.Name] = setter;
                }
                return map;
            });
        }
        private static List<Action<TSource, TDestination>> GetPropertyAssign()
        {
            return _assignCache.GetOrAdd((sourceType, destinationType), types =>
            {
                var properties = types.Item1.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var map = new List<Action<TSource, TDestination>>();
                foreach (var sourceProperty in properties)
                {
                    var destinationProperty = types.Item2.GetProperty(sourceProperty.Name);
                    if(destinationProperty == null)
                    {
                        continue;
                    }
                    var assign = CreatePropertyAssign(sourceProperty, destinationProperty);

                    map.Add(assign);
                }
                return map;
            });
        }
        private static Func<TSource, object> CreatePropertyGetter(PropertyInfo property)
        {

            var parameter = Expression.Parameter(sourceType, "source");
            var propertyAccess = Expression.Convert(Expression.Property(parameter, property), typeof(object));
            var lambda = Expression.Lambda<Func<TSource, object>>(propertyAccess, parameter);

            return lambda.Compile();
        }
        private static Action<TDestination, object> CreatePropertySetter(PropertyInfo property)
        {

            var targetType = destinationType;
            var propertyType = property.PropertyType;

            // パラメータ: (TDestination target, object value)
            var targetParam = Expression.Parameter(targetType, "target");
            var valueParam = Expression.Parameter(typeof(object), "value");

            // (propertyType)value
            var castedValue = Expression.Convert(valueParam, propertyType);

            // target.Property
            var propertyExpr = Expression.Property(targetParam, property);

            // target.Property = (propertyType)value
            var assignExpr = Expression.Assign(propertyExpr, castedValue);

            // ラムダ式を作成: (target, value) => target.Property = (propertyType)value;
            var lambda = Expression.Lambda<Action<TDestination, object>>(assignExpr, targetParam, valueParam);

            return lambda.Compile();
        }

        private static Action<TSource, TDestination> CreatePropertyAssign(PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        {
            
            var source = Expression.Parameter(typeof(TSource), "source");
            var sourceAccess = Expression.Property(source, sourceProperty);

            var destination = Expression.Parameter(destinationType, "destination");
            
            var assign = Expression.Assign(Expression.Property(destination, destinationProperty), sourceAccess);

            var lambda = Expression.Lambda<Action<TSource, TDestination>>(assign, source, destination);

            return lambda.Compile();
        }

        private static Func<TDestination> CreateDestinationConstructor()
        {
            if (_destinationConstructor == null)
            {
                var constructor = destinationType.GetConstructor(Type.EmptyTypes);
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
            var constructor = destinationType.GetConstructor(Type.EmptyTypes);
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
                if (destinationProperty != null)
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
