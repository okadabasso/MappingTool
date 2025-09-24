namespace MappingTool.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.Extensions.Logging;
    public class MapperFactory<TSource, TDestination>
        where TSource : notnull
        where TDestination : notnull
    {
        private const int DefaultMaxRecursionDepth = 5;
        private int _maxRecursionDepth = DefaultMaxRecursionDepth;
        private int _currentRecursionDepth = 0;

        /// <summary>
        /// プロパティ初期化子のキャッシュ
        /// 
        /// var foo = new Foo { A = 1, B = 2 };
        /// </summary>
        private static readonly LRUCache<(Type, Type), Func<MappingContext, object, object>> _memberInitCache = new(100);
        /// <summary>
        /// コンストラクター初期化子のキャッシュ
        /// 
        /// var foo = new Foo(1, 2);
        /// </summary>
        private static readonly LRUCache<(Type, Type), Func<MappingContext, object, object>> _constructorCache = new(100);
        /// <summary>
        /// プロパティ代入のキャッシュ
        /// 
        /// foo.A = source.A;
        /// foo.B = source.B;
        /// ...
        /// </summary>
        private static readonly LRUCache<(Type, Type), Action<MappingContext, object, object>> _propertyAssignCache = new(100);

        private ITypeAnalyzer _typeAnalyzer = new TypeAnalyzer();
        public static MethodInfo EnumerableSelect(Type enumerableType, Type elementType) => typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
            .MakeGenericMethod(enumerableType, elementType);

        public static MethodInfo EnumerableToList(Type elementType) => typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "ToList" && m.GetParameters().Length == 1)
            .MakeGenericMethod(elementType);

        public static MethodInfo EnumerableToArray(Type elementType) => typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "ToArray" && m.GetParameters().Length == 1)
            .MakeGenericMethod(elementType);

        private readonly ILogger? _logger;

        static MapperFactory()
        {
        }
        public MapperFactory(bool allowRecursion = false, int maxRecursionDepth = DefaultMaxRecursionDepth, ILogger? logger = null)
        {
            if (allowRecursion)
            {
                _maxRecursionDepth = maxRecursionDepth;
            }
            else
            {
                _maxRecursionDepth = 1;
            }
            _logger = logger;
        }
        public IMapper<TSource, TDestination> CreateMapper()
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);
            var mappingContextParameter = Expression.Parameter(typeof(MappingContext), "context");

            Action<MappingContext, object, object> assign = GetOrCreatePropertyAssign(sourceType, destinationType, mappingContextParameter)!;
            Func<MappingContext, object, object> initializer = CreateObjectInitializer(
                typeof(TSource),
                typeof(TDestination),
                mappingContextParameter);

            Func<MappingContext, TSource, TDestination> objectInitializer = (context, source) => (TDestination)initializer(context, source)!;
            Action<MappingContext, TSource, TDestination> propertyAssign = (context, source, destination) => assign(context, source, destination);
            return new SimpleMapper<TSource, TDestination>(objectInitializer, propertyAssign);
        }
        public Func<MappingContext, object, object> CreateObjectInitializer(
            Type sourceType,
            Type destinationType,
            ParameterExpression mappingContextParameter
           )
        {
            if (EnterRecursion() == false)
            {
                Func<MappingContext, object, object> nullExpression =
                    (context, source) => null!;
                return nullExpression;
            }
            Func<MappingContext, object, object> objectInitializer = null!;


            if (destinationType.IsClass)
            {
                // `record` を識別するために主コンストラクターをチェック
                if (destinationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                        .Any(c => c.GetParameters().Length == destinationType.GetProperties().Length))
                {
                    objectInitializer = GetOrCreateConstructorInitializer(sourceType, destinationType, mappingContextParameter);
                    if (objectInitializer == null)
                    {
                        objectInitializer = GetOrCreatePropertyInitializer(sourceType, destinationType, mappingContextParameter);
                    }
                }
                else
                {
                    objectInitializer = GetOrCreatePropertyInitializer(sourceType, destinationType, mappingContextParameter);

                }
            }
            else if (destinationType.IsValueType && !destinationType.IsEnum)
            {
                objectInitializer = GetOrCreateConstructorInitializer(sourceType, destinationType, mappingContextParameter);
                if (objectInitializer == null)
                {
                    objectInitializer = GetOrCreatePropertyInitializer(sourceType, destinationType, mappingContextParameter);
                }
            }
            else
            {
                throw new InvalidOperationException($"Type {destinationType.Name} is not a class or struct.");
            }
            ExitRecursion();
            return objectInitializer;

        }

        private Action<MappingContext, object, object> CreatePropertyAssign(
            Type sourceType,
            Type destinationType,
            ParameterExpression mappingContextParameter)
        {

            var expressionList = new List<Expression>();

            var source = Expression.Parameter(typeof(object), "source");
            var destination = Expression.Parameter(typeof(object), "destination");

            foreach (var destinationProperty in destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!destinationProperty.CanWrite)
                {
                    continue;
                }
                var sourceProperty = sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null)
                {
                    var defaultParametr = Expression.Default(destinationProperty.PropertyType);
                    expressionList.Add(defaultParametr);
                    continue;
                }
                if (sourceProperty.CanRead)
                {
                    var sourcePropertyValue = CreatePropertyValueExpression(
                        destinationProperty.PropertyType,
                        sourceType,
                        sourceProperty,
                        source,
                        mappingContextParameter);

                    var destinationPropertyAccess = Expression.Property(Expression.Convert(destination, destinationType), destinationProperty);
                    var assign = Expression.Assign(destinationPropertyAccess, sourcePropertyValue);

                    expressionList.Add(assign);
                }

            }


            var block = Expression.Block(expressionList);
            var lambda = Expression.Lambda<Action<MappingContext, object, object>>(block, mappingContextParameter, source, destination);
            DebugView(lambda);
            return lambda.Compile();
        }
        private Func<MappingContext, object, object> CreateMemberInit(
            Type sourceType,
            Type destinationType,
            ParameterExpression mappingContextParameter)
        {
            var constructor = destinationType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {typeof(TDestination).Name} does not have a parameterless constructor.");
            }
            var newExpression = Expression.New(constructor);
            var source = Expression.Parameter(typeof(object), "source");

            var memberBindings = new List<MemberBinding>();

            foreach (var destinationProperty in destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!destinationProperty.CanWrite)
                {
                    continue;
                }

                var sourceProperty = sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null)
                {
                    var defaultParametr = Expression.Default(destinationProperty.PropertyType);
                    var binding = Expression.Bind(destinationProperty, defaultParametr);
                    memberBindings.Add(binding);
                    continue;
                }
                if (sourceProperty.CanRead)
                {
                    var expression = CreatePropertyValueExpression(
                        destinationProperty.PropertyType,
                        sourceType,
                        sourceProperty,
                        source,
                        mappingContextParameter);
                    var binding = Expression.Bind(destinationProperty, expression);
                    memberBindings.Add(binding);
                }
            }


            var memberInit = Expression.MemberInit(newExpression, memberBindings);
            var lambda = Expression.Lambda<Func<MappingContext, object, object>>(memberInit, mappingContextParameter, source);
            DebugView(lambda);

            return lambda.Compile();
        }
        private Func<MappingContext, object, object> CreateConstructorInititializer(
            Type sourceType,
            Type destinationType,
            ParameterExpression mappingContextParameter)
        {
            var constructor = GetPrimaryConstructor(destinationType);
            if (constructor == null)
            {
                return null!;
            }
            var sourceParameter = Expression.Parameter(typeof(object), "source");
            var parameters = constructor.GetParameters();
            var parameterExpressions = new ParameterExpression[parameters.Length];
            var arguments = new List<Expression>(parameters.Length);

            foreach (var parameter in parameters)
            {
                if (string.IsNullOrEmpty(parameter.Name))
                {
                    throw new InvalidOperationException("Parameter name is null or empty.");
                }
                var sourceProperty = sourceType.GetProperty(parameter.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProperty == null)
                {
                    arguments.Add(Expression.Default(parameter.ParameterType));
                    continue;
                }
                if (sourceProperty.CanRead)
                {
                    var expression = CreatePropertyValueExpression(
                    parameter.ParameterType,
                    sourceType,
                    sourceProperty,
                    sourceParameter,
                    mappingContextParameter);
                    arguments.Add(Expression.Convert(expression, parameter.ParameterType));

                }
            }
            var newExpression = Expression.New(constructor, arguments);

            var lambda = Expression.Lambda<Func<MappingContext, object, object>>(newExpression, mappingContextParameter, sourceParameter);
             DebugView(lambda);

            return lambda.Compile();
        }
        private ConstructorInfo? GetPrimaryConstructor(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            var validConstructors = constructors
                .Where(c => c.GetParameters().Length > 0)
                .Select(c => new
                {
                    Constructor = c,
                    MatchCount = GetMatchingParameterCount(c, type),
                    ParameterCount = c.GetParameters().Length
                })
                .Where(x => x.MatchCount >= 1)
                .OrderByDescending(x => x.MatchCount)
                .ThenByDescending(x => x.ParameterCount);

            return validConstructors.FirstOrDefault()?.Constructor;
        }

        private int GetMatchingParameterCount(ConstructorInfo constructor, Type type)
        {
            return constructor.GetParameters().Count(parameter =>
                !string.IsNullOrEmpty(parameter.Name) &&
                type.GetProperty(parameter.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null
            );
        }
        private Func<MappingContext, object, object> GetOrCreatePropertyInitializer(
            Type sourceType,
            Type destinationType,
            ParameterExpression mappingContextParameter)
        {
            var initializer = _memberInitCache.GetOrAdd((sourceType, destinationType), _ => CreateMemberInit(sourceType, destinationType, mappingContextParameter));
            return initializer;
        }

        private Func<MappingContext, object, object> GetOrCreateConstructorInitializer(
            Type sourceType,
            Type destinationType,
            ParameterExpression mappingContextParameter)
        {
            var initializer = _constructorCache.GetOrAdd((sourceType, destinationType), _ => CreateConstructorInititializer(sourceType, destinationType, mappingContextParameter));
            return initializer;
        }
        private Action<MappingContext, object, object> GetOrCreatePropertyAssign(
            Type sourceType,
            Type destinationType,
            ParameterExpression mappingContextParameter)
        {
            var initializer = _propertyAssignCache.GetOrAdd((sourceType, destinationType), _ => CreatePropertyAssign(sourceType, destinationType, mappingContextParameter));
            return initializer;
        }
        private Expression CreateDefaultValueExpression(Type type)
        {
            if (type.IsValueType)
            {
                return Expression.Default(type);
            }
            else
            {
                return Expression.Constant(null, type);
            }
        }
        private Expression CreatePropertyValueExpression(
            Type destinationPropertyType,
            Type sourceType,
            PropertyInfo sourceProperty,
            ParameterExpression sourceParameter,
            ParameterExpression mappingContextParameter)
        {
            if (sourceProperty == null || !sourceProperty.CanRead)
            {
                var defaultParametr = CreateDefaultValueExpression(destinationPropertyType);
                return defaultParametr;
            }

            var sourceAccess = Expression.Property(Expression.Convert(sourceParameter, sourceType), sourceProperty);
            if (_typeAnalyzer.IsPrimitiveType(destinationPropertyType))
            {
                return Expression.Convert(sourceAccess, destinationPropertyType);
            }
            if (_typeAnalyzer.IsNullableType(destinationPropertyType) && _typeAnalyzer.IsPrimitiveType(Nullable.GetUnderlyingType(destinationPropertyType)!))
            {
                return Expression.Convert(sourceAccess, destinationPropertyType);
            }
            if (_typeAnalyzer.IsPrimitiveArrayType(destinationPropertyType))
            {

                var listExpression = CreatePrimitiveArrayExpression(
                    sourceProperty.PropertyType,
                    destinationPropertyType,
                    sourceAccess,
                    mappingContextParameter
                );
                return Expression.Convert(listExpression, destinationPropertyType);
            }
            if (_typeAnalyzer.IsPrimitiveEnumerableType(destinationPropertyType))
            {
                var listExpression = CreatePrimitiveEnumerableExpression(
                    sourceProperty.PropertyType,
                    destinationPropertyType,
                    sourceAccess,
                    mappingContextParameter
                );
                return Expression.Convert(listExpression, destinationPropertyType);
            }
            if (_typeAnalyzer.IsComplexArrayType(destinationPropertyType))
            {
                var arrayExpression = CreateComplexArrayExpression(
                    sourceProperty.PropertyType,
                    destinationPropertyType,
                    sourceAccess,
                    mappingContextParameter
                );

                return Expression.Convert(arrayExpression, destinationPropertyType);
            }
            if (_typeAnalyzer.IsComplexEnumerableType(destinationPropertyType))
            {
                var listExpression = CreateComplexEnumerableExpression(
                    sourceProperty.PropertyType,
                    destinationPropertyType,
                    sourceAccess,
                    mappingContextParameter
                );
                return Expression.Convert(listExpression, destinationPropertyType);
            }

            if (_typeAnalyzer.IsComplexType(destinationPropertyType))
            {
                // ネストされたオブジェクトのマッピング
                var objectInitializer = CreateNestedObjectInitializer(
                    sourceProperty.PropertyType,
                    destinationPropertyType,
                    sourceAccess,
                    mappingContextParameter
                );
                return Expression.Convert(objectInitializer, destinationPropertyType);
            }

            var directAccess = Expression.Property(Expression.Convert(sourceParameter, sourceType), sourceProperty);
            return Expression.Convert(directAccess, destinationPropertyType);

        }
        private Expression CreateNestedObjectInitializer(
            Type sourcePropertyType,
            Type destinationPropertyType,
            Expression propertyExpression,
            ParameterExpression mappingContextParameter
           )
        {
            // `func` を呼び出す式を構築
            var objectInitializer = CreateObjectInitializer(sourcePropertyType, destinationPropertyType, mappingContextParameter);
            var invokeFunc = Expression.Invoke(
                Expression.Constant(objectInitializer), // `func` を式ツリー内で使用するために Constant に変換
                mappingContextParameter,
                propertyExpression
            );
            var circularCheck = CreateCircularCheck(
                destinationPropertyType,
                propertyExpression,
                mappingContextParameter
            );
            var circularOrObject = Expression.Condition(
                circularCheck,
                Expression.Constant(null, typeof(object)),
                Expression.Block(
                    CreateMarkAsMapped(
                        destinationPropertyType,
                        propertyExpression,
                        mappingContextParameter
                    ),
                    invokeFunc
                )
            );
            var nullOrObject = Expression.Condition(
                Expression.Equal(propertyExpression, Expression.Constant(null, sourcePropertyType)),
                Expression.Constant(null, typeof(object)),
                circularOrObject
            );


            return nullOrObject;
        }
        private Expression CreatePrimitiveArrayExpression(
            Type sourcePropertyType,
            Type destinationPropertyType,
            Expression sourceAccess,
            ParameterExpression mappingContextParameter
        )
        {
            return Expression.Convert(sourceAccess, destinationPropertyType);
        }
        private Expression CreatePrimitiveEnumerableExpression(
            Type sourcePropertyType,
            Type destinationPropertyType,
            Expression sourceAccess,
            ParameterExpression mappingContextParameter
        )
        {
            var sourceElementType = sourcePropertyType.GetGenericArguments()[0];
            var destinationElementType = destinationPropertyType.GetGenericArguments()[0];

            var toListCall = Expression.Call(
                EnumerableToList(destinationElementType),
                Expression.Convert(sourceAccess, typeof(IEnumerable<>).MakeGenericType(sourceElementType))
            );
            return Expression.Convert(toListCall, destinationPropertyType);
        }

        private Expression CreateComplexEnumerableExpression(
            Type sourcePropertyType,
            Type destinationPropertyType,
            Expression sourceAccess,
            ParameterExpression mappingContextParameter
)
        {
            var sourceElementType = sourcePropertyType.GetGenericArguments()[0];
            var destinationElementType = destinationPropertyType.GetGenericArguments()[0];

            var selectCall = CreateEnumerableSelectExpression(
                sourceElementType,
                destinationElementType,
                sourceAccess,
                mappingContextParameter
            );
            var toListCall = Expression.Call(
                EnumerableToList(destinationElementType),
                selectCall
            );
            return Expression.Convert(toListCall, destinationPropertyType);
        }
        private Expression CreateComplexArrayExpression(
            Type sourcePropertyType,
            Type destinationPropertyType,
            Expression sourceAccess,
            ParameterExpression mappingContextParameter
        )
        {
            var sourceElementType = sourcePropertyType.GetElementType()!;
            var destinationElementType = destinationPropertyType.GetElementType()!;

            var selectCall = CreateEnumerableSelectExpression(
                sourceElementType,
                destinationElementType,
                sourceAccess,
                mappingContextParameter
            );
            var toArrayCall = Expression.Call(
                EnumerableToArray(destinationElementType),
                selectCall
            );
            return Expression.Convert(toArrayCall, destinationPropertyType);
        }
        /// <summary>
        /// IEnumerable の各要素をマッピングする Select 式を作成する
        /// 
        /// x => x.Select(y => Map(y))
        /// </summary>
        /// <param name="sourceElementType"></param>
        /// <param name="destinationElementType"></param>
        /// <param name="sourceAccess"></param>
        /// <param name="mappingContextParameter"></param>
        /// <returns></returns>
        private Expression CreateEnumerableSelectExpression(
            Type sourceElementType,
            Type destinationElementType,
            Expression sourceAccess,
            ParameterExpression mappingContextParameter
        )
        {
            var objectInitializer = CreateObjectInitializer(
                sourceElementType,
                destinationElementType,
                mappingContextParameter
            );

            var sourceParam = Expression.Parameter(sourceElementType, "x");
            var selectorLambda = Expression.Lambda(
                Expression.Convert(
                    Expression.Invoke(
                        Expression.Constant(objectInitializer),
                        mappingContextParameter,
                        sourceParam
                    ),
                    destinationElementType
                ),
                sourceParam
            );

            var selectCall = Expression.Call(
                EnumerableSelect(sourceElementType, destinationElementType),
                Expression.Convert(sourceAccess, typeof(IEnumerable<>).MakeGenericType(sourceElementType)),
                selectorLambda
            );
            return selectCall;
        }
        private Expression CreateCircularCheck(
            Type destinationType,
            Expression propertyExpression,
            ParameterExpression mappingContextParameter
        )
        {
            // mapping context ですでにマップされているなら null を返す expressionを構築する

            var isMapped = typeof(MappingContext).GetMethod("IsMapped")!;
            var markAsMapped = typeof(MappingContext).GetMethod("MarkAsMapped")!;
            var checkMapped = Expression.Call(
                mappingContextParameter,
                isMapped,
                Expression.Convert(propertyExpression, typeof(object)) // 明示的に object 型に変換
            );

            return checkMapped;
        }
        private Expression CreateMarkAsMapped(
            Type destinationType,
            Expression propertyExpression,
            ParameterExpression mappingContextParameter
        )
        {
            var markAsMapped = typeof(MappingContext).GetMethod("MarkAsMapped")!;
            var mark = Expression.Call(
                mappingContextParameter,
                markAsMapped,
                Expression.Convert(propertyExpression, typeof(object)) // 明示的に object 型に変換
            );

            return mark;
        }

        private bool EnterRecursion()
        {
            if (_currentRecursionDepth >= _maxRecursionDepth)
            {
                return false;
            }
            _currentRecursionDepth++;
            return true;
        }
        private bool ExitRecursion()
        {
            _currentRecursionDepth = Math.Max(0, _currentRecursionDepth - 1);
            return true;
        }

        public void DebugView(Expression expr)
        {
            if (_logger != null)
            {
                // DebugView プロパティをリフレクションで取得
                var debugViewProp = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
                string debugView = (string)(debugViewProp?.GetValue(expr) ?? "");

                _logger?.LogDebug(debugView);
                    
            }
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