using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace Experimental1.Shared;

public class MapperFactory<TSource, TDestination>
    where TSource : class
    where TDestination : class
{

    public SimpleMapper<TSource, TDestination> CreateMapper()
    {
        var sourceType = typeof(TSource);
        var destinationType = typeof(TDestination);


        // Create a new instance of the SimpleMapper class
        return new SimpleMapper<TSource, TDestination>();
    }
}
/// <summary>
/// SimpleMapper を生成するクラス
/// </summary>
public class MapperFactory
{
    private static readonly ConcurrentDictionary<(Type, Type), ISimpleMapper> _mapperCache = new();
    private static readonly LRUCache<(Type, Type), object> _objectInitializerCache = new(100);
    private static readonly LRUCache<(Type, Type), object> _constructorInitializerCache = new(100);
    private static readonly LRUCache<(Type, Type), object> _propertyAssignCache = new(100);

    public static SimpleMapper<TSource, TDestination> CreateMapper<TSource, TDestination>()
        where TSource : notnull
        where TDestination : notnull
    {
        var mapper = _mapperCache.GetOrAdd((typeof(TSource), typeof(TDestination)), (key) =>
        {
            var destinationType = key.Item2;

            // プロパティ割り当てロジックを作成
            var propertyAssign = CreatePropertyAssign<TSource, TDestination>();

            // 初期化ロジックを取得
            var objectInitializer = GetObjectInitializer<TSource, TDestination>(destinationType);
            if (objectInitializer == null)
            {
                throw new InvalidOperationException($"Type {destinationType.Name} does not have a suitable constructor or initializer.");
            }

            // SimpleMapper を生成
            return new SimpleMapper<TSource, TDestination>(objectInitializer, propertyAssign);

        });
        return (SimpleMapper<TSource, TDestination>)mapper;
    }
    private static Func<TSource, TDestination>? GetObjectInitializer<TSource, TDestination>(Type destinationType)
    {
        // 値型の場合
        if (destinationType.IsValueType && !destinationType.IsEnum && !destinationType.IsPrimitive)
        {
            return CreateConstructorInititializer<TSource, TDestination>() ?? CreateObjectInitializer<TSource, TDestination>();
        }

        // クラス型の場合
        if (destinationType.IsClass)
        {
            return CreateConstructorInititializer<TSource, TDestination>() ?? CreateObjectInitializer<TSource, TDestination>();
        }

        // その他の場合は null を返す
        return null;
    }
    /// <summary>
    /// オブジェクト初期化子を生成する
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Func<TSource, TDestination> CreateObjectInitializer<TSource, TDestination>()
    {
        Type sourceType = typeof(TSource);
        Type destinationType = typeof(TDestination);

        var constructor = destinationType.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
        {
            throw new InvalidOperationException($"Type {typeof(TDestination).Name} does not have a parameterless constructor.");
        }
        var newExpression = Expression.New(constructor);
        var source = Expression.Parameter(typeof(TSource), "source");

        var memberBindings = new List<MemberBinding>();

        foreach (var destinationProperty in destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!destinationProperty.CanWrite)
            {
                continue;
            }
            var sourceProperty = sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var binding = CreateMemberBinding(source, sourceProperty, destinationProperty);
            if(binding != null)
            {
                memberBindings.Add(binding);
            }
        }


        var memberInit = Expression.MemberInit(newExpression, memberBindings);
        var lambda = Expression.Lambda<Func<TSource, TDestination>>(memberInit, source);
        return lambda.Compile();
    }
    static MemberBinding? CreateMemberBinding(Expression sourceParameeter, PropertyInfo? sourceProperty, PropertyInfo destinationProperty)
    {
        if (sourceProperty == null)
        {
            var defaultParametr = Expression.Default(destinationProperty.PropertyType);
            var binding = Expression.Bind(destinationProperty, defaultParametr);
            return binding;
        }
        if (sourceProperty.CanRead)
        {
            var sourceAccess = Expression.Property(sourceParameeter, sourceProperty);
            var binding = Expression.Bind(destinationProperty, sourceAccess);
            return binding;
        }
        return null;

    }
    private static Action<TSource, TDestination> CreatePropertyAssign<TSource, TDestination>()
    {
        Type sourceType = typeof(TSource);
        Type destinationType = typeof(TDestination);

        var expressionList = new List<Expression>();

        var source = Expression.Parameter(sourceType, "source");
        var destination = Expression.Parameter(destinationType, "destination");

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
    private static Func<TSource, TDestination> CreateConstructorInititializer<TSource, TDestination>()
    {
        Type sourceType = typeof(TSource);
        Type destinationType = typeof(TDestination);
        var constructor = GetPrimaryConstructor(destinationType);
        if (constructor == null)
        {
            return null!;
        }
        var sourceParameter = Expression.Parameter(sourceType, "source");
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
    private static ConstructorInfo? GetPrimaryConstructor(Type t)
    {
        // すべてのパブリックなインスタンスコンストラクターを取得
        var constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        // 主コンストラクターを特定
        var primaryConstructor = constructors.FirstOrDefault(c =>
        {
            var parameters = c.GetParameters();
            // 主コンストラクターの条件: パラメーターの数がプロパティの数と一致
            return parameters.Length == t.GetProperties().Length &&
                parameters.All(p => t.GetProperty(p.Name ?? "", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null);
        });
        return primaryConstructor;
    }

}
