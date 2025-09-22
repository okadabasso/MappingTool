namespace MappingTool.Mapping
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    public class MapperFactory<TSource, TDestination>
        where TSource : notnull
        where TDestination : notnull
    {
        /// <summary>
        /// プロパティ初期化子のキャッシュ
        /// 
        /// var foo = new Foo { A = 1, B = 2 };
        /// </summary>
        private static readonly LRUCache<(Type, Type), Func<TSource, TDestination>> _memberInitCache = new(100);
        /// <summary>
        /// コンストラクター初期化子のキャッシュ
        /// 
        /// var foo = new Foo(1, 2);
        /// </summary>
        private static readonly LRUCache<(Type, Type), Func<TSource, TDestination>> _constructorCache = new(100);
        /// <summary>
        /// プロパティ代入のキャッシュ
        /// 
        /// foo.A = source.A;
        /// foo.B = source.B;
        /// ...
        /// </summary>
        private static readonly LRUCache<(Type, Type), Action<TSource, TDestination>> _propertyAssignCache = new(100);

        /// <summary>
        /// マップ元の型
        /// </summary>
        private static Type _sourceType = typeof(TSource);
        /// <summary>
        /// マップ先の型
        /// </summary>
        private static Type _destinationType = typeof(TDestination);

        static MapperFactory()
        {
        }

        public IMapper<TSource, TDestination> CreateMapper()
        {
            Action<TSource, TDestination> propertyAssign = null!;
            Func<TSource, TDestination> objectInitializer = null!;
      
            propertyAssign = GetOrCreatePropertyAssign();

            if (_destinationType.IsClass)
            {
                // `record` を識別するために主コンストラクターをチェック
                if (_destinationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                        .Any(c => c.GetParameters().Length == _destinationType.GetProperties().Length))
                {
                    objectInitializer = GetOrCreateConstructorInitializer();
                    if (objectInitializer == null)
                    {
                        objectInitializer = GetOrCreatePropertyInitializer();
                    }
                }
                else
                {
                    objectInitializer = GetOrCreatePropertyInitializer();

                }
            }
            else if (_destinationType.IsValueType && !_destinationType.IsEnum)
            {
                objectInitializer = GetOrCreateConstructorInitializer();
                if (objectInitializer == null)
                {
                    objectInitializer = GetOrCreatePropertyInitializer();
                }
            }
            else
            {
                throw new InvalidOperationException($"Type {_destinationType.Name} is not a class or struct.");
            }

            return new SimpleMapper<TSource, TDestination>( objectInitializer, propertyAssign);
        }

        private Action<TSource, TDestination> CreatePropertyAssign()
        {

            var expressionList = new List<Expression>();

            var source = Expression.Parameter(_sourceType, "source");
            var destination = Expression.Parameter(_destinationType, "destination");

            foreach (var destinationProperty in _destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!destinationProperty.CanWrite)
                {
                    continue;
                }

                var sourceProperty = _sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null)
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
        private Func<TSource, TDestination> CreateObjecetInitializer()
        {
            var constructor = _destinationType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {typeof(TDestination).Name} does not have a parameterless constructor.");
            }
            var newExpression = Expression.New(constructor);
            var source = Expression.Parameter(typeof(TSource), "source");

            var memberBindings = new List<MemberBinding>();

            foreach (var destinationProperty in _destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!destinationProperty.CanWrite)
                {
                    continue;
                }

                var sourceProperty = _sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null)
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
        private Func<TSource, TDestination> CreateConstructorInititializer()
        {
            var constructor = GetPrimaryConstructor();
            if (constructor == null)
            {
                return null!;
            }
            var sourceParameter = Expression.Parameter(_sourceType, "source");
            var parameters = constructor.GetParameters();
            var parameterExpressions = new ParameterExpression[parameters.Length];
            var arguments = new List<Expression>(parameters.Length);

            foreach (var parameter in parameters)
            {
                if (string.IsNullOrEmpty(parameter.Name))
                {
                    throw new InvalidOperationException("Parameter name is null or empty.");
                }
                var sourceProperty = _sourceType.GetProperty(parameter.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null)
                {
                    arguments.Add(Expression.Default(parameter.ParameterType));
                }
                else
                {
                    arguments.Add(Expression.Property(sourceParameter, sourceProperty));
                }
            }
            var newExpression = Expression.New(constructor, arguments);

            var lambda = Expression.Lambda<Func<TSource, TDestination>>(newExpression, sourceParameter);
            return lambda.Compile();
        }
        private ConstructorInfo? GetPrimaryConstructor()
        {
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

        private Func<TSource, TDestination> GetOrCreatePropertyInitializer()
        {
            var initializer = _memberInitCache.GetOrAdd((_sourceType, _destinationType), _ => CreateObjecetInitializer());
            return initializer;
        }

        private Func<TSource, TDestination> GetOrCreateConstructorInitializer()
        {
            var initializer = _constructorCache.GetOrAdd((_sourceType, _destinationType), _ => CreateConstructorInititializer());
            return initializer;
        }
        private Action<TSource, TDestination> GetOrCreatePropertyAssign()
        {
            var initializer = _propertyAssignCache.GetOrAdd((_sourceType, _destinationType), _ => CreatePropertyAssign());
            return initializer;
        }

    }

    public interface IMapper<TSource, TDestination>
        where TSource : notnull
        where TDestination : notnull
    {
        TDestination Map(TSource source);
        IEnumerable<TDestination> Map(IEnumerable<TSource> source);
        void Map(TSource source, TDestination destination);
    }
}
