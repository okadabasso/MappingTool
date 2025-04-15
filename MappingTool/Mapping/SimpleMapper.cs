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
        private static Action<TSource, TDestination> _assignProperties = null!;
        private static Func<TSource, TDestination> _objectInitializer = null!;

        private static Type _sourceType = typeof(TSource);
        private static Type _destinationType = typeof(TDestination);

        static SimpleMapper()
        {
            if (_destinationType.IsClass)
            {
                // `record` を識別するために主コンストラクターをチェック
                if (_destinationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                        .Any(c => c.GetParameters().Length == _destinationType.GetProperties().Length))
                {
                    _objectInitializer = CreateConstructorInititializer();
                    if (_objectInitializer == null)
                    {
                        _objectInitializer = CreatePropertyInitializer();
                    }
                }
                else{
                    _assignProperties = CreatePropertyAssign();
                    _objectInitializer = CreatePropertyInitializer();
    
                }
            }
            else if (_destinationType.IsValueType && !_destinationType.IsEnum)
            {
                _objectInitializer = CreateConstructorInititializer();
                if(_objectInitializer == null)
                {
                    _objectInitializer = CreatePropertyInitializer();
                }
            }
        
        }
        public SimpleMapper()
        {
        }

        public void Map(TSource source, TDestination destination)
        {
            if (source == null || destination == null)
            {
                throw new ArgumentNullException("Source or destination cannot be null.");
            }
            _assignProperties(source, destination);
        }
        public TDestination Map(TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source cannot be null.");
            }
            var destination = _objectInitializer(source);
            return destination;
        }
        public IEnumerable<TDestination> Map(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source cannot be null.");
            }
            var list = new List<TDestination>(source.Count());
            foreach (var item in source)
            {
                var destination = _objectInitializer(item);
                list.Add(destination);
            }
            
            return list;
        }

        private static Action<TSource, TDestination> CreatePropertyAssign()
        {

            var expressionList = new List<Expression>();

            var source = Expression.Parameter(_sourceType, "source");
            var destination = Expression.Parameter(_destinationType, "destination");

            foreach(var destinationProperty in _destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!destinationProperty.CanWrite)
                {
                    continue;
                }
                
                var sourceProperty = _sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(sourceProperty == null)
                {
                    var defaultParametr = Expression.Default(destinationProperty.PropertyType);
                    expressionList.Add(defaultParametr);
                    continue;
                }
                if (sourceProperty.CanRead)
                {
                    var sourceAccess = Expression.Property(source, sourceProperty);
                    var destinationAccess = Expression.Property(destination, destinationProperty);
                    var assign = Expression.Assign(destinationAccess, Expression.Convert(sourceAccess, destinationProperty.PropertyType));

                    expressionList.Add(assign);
                }
                
            }

            
            var block = Expression.Block(expressionList);
            var lambda = Expression.Lambda<Action<TSource, TDestination>>(block, source, destination);

            return lambda.Compile();
        }
        private static Func<TSource, TDestination> CreatePropertyInitializer()
        {
            var constructor = _destinationType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {typeof(TDestination).Name} does not have a parameterless constructor.");
            }
            var newExpression = Expression.New(constructor);
            var source = Expression.Parameter(typeof(TSource), "source");

            var memberBindings = new List<MemberBinding>();

            foreach(var destinationProperty in _destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!destinationProperty.CanWrite)
                {
                    continue;
                }
                
                var sourceProperty = _sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(sourceProperty == null)
                {
                    var defaultParametr = Expression.Default(destinationProperty.PropertyType);
                    var binding = Expression.Bind(destinationProperty, defaultParametr);
                    memberBindings.Add(binding);
                    continue;
                }
                if (sourceProperty.CanRead)
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
        private static Func<TSource, TDestination> CreateConstructorInititializer()
        {
            var constructor = GetPrimaryConstructor();
            if (constructor == null)
            {
                return null;
            }
            var sourceParameter = Expression.Parameter(_sourceType, "source");
            var parameters = constructor.GetParameters();
            var parameterExpressions = new ParameterExpression[parameters.Length];
            var arguments = new List<Expression>(parameters.Length);

            foreach (var parameter in parameters)
            {
                if(string.IsNullOrEmpty(parameter.Name))
                {
                    throw new InvalidOperationException("Parameter name is null or empty.");
                }
                var sourceProperty = _sourceType.GetProperty(parameter.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null)
                {
                    arguments.Add(Expression.Default(parameter.ParameterType));
                }
                else{
                    arguments.Add(Expression.Property(sourceParameter, sourceProperty));
                }
            }
            var newExpression = Expression.New(constructor, arguments);

            var lambda = Expression.Lambda<Func<TSource, TDestination>>(newExpression, sourceParameter);
            return lambda.Compile();
        }
        private static ConstructorInfo? GetPrimaryConstructor(){
            // すべてのパブリックなインスタンスコンストラクターを取得
            var constructors = _destinationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // 主コンストラクターを特定
            var primaryConstructor = constructors.FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                // 主コンストラクターの条件: パラメーターの数がプロパティの数と一致
                return parameters.Length == _destinationType.GetProperties().Length &&
                    parameters.All(p => _destinationType.GetProperty(p.Name ?? "", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null);
            });
            return primaryConstructor;
        }

    }
}
