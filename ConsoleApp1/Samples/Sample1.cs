namespace ConsoleApp1.Samples;

using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;

public class Sample1
{
    private static readonly LRUCache<(Type, Type), object> _propertyInitializerCache = new(100);
    private static readonly LRUCache<(Type, Type), object> _constructorInitializerCache = new(100);
    private static readonly LRUCache<(Type, Type), object> _propertyAssignCache = new(100);

    public static void SampleMethod4(){
        var list = new List<string> { "a", "b", "c" };
        var mapper = new SimpleMapper<string, string>();
        var result = mapper.Map5(list);
        foreach (var item in result)
        {
            Console.WriteLine(item);
        }
    }
    public static void SampleMethod1(){
        var intType = typeof(int);
        var stringType = typeof(string);
        var dateTimeType = typeof(DateTime);

        var intNullType = typeof(int?);
        var dateTimeNullType = typeof(DateTime?);
        TypeCheck(intType);
        TypeCheck(stringType);
        TypeCheck(dateTimeType);

        TypeCheck(intNullType);
        TypeCheck(dateTimeNullType);
        TypeCheck(typeof(List<string>));
    
    }
    private static void TypeCheck(Type t){
        if(t.IsGenericType)        {
            var genericType = t.GetGenericTypeDefinition();
            if (genericType == typeof(Nullable<>))
            {
                
                Console.WriteLine($"Type: {t.Name} is Nullable<> type. {t.GetGenericArguments()[0].Name}");
            }
            else if (genericType == typeof(List<>) || genericType == typeof(IList<>))
            {
                
                var elementType = t.GetGenericArguments()[0];
                Console.WriteLine($"Type: {t.Name} is List<> or IList<> type. {elementType.Name}");
            }
            else if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>))
            {
                Console.WriteLine($"Type: {t.Name} is Dictionary<> or IDictionary<> type.");
            }
            else
            {
                Console.WriteLine($"Type: {t.Name} is not a recognized generic type.");
            }
        }
        else{
            Console.WriteLine($"Type: {t.Name} is value type: {t.IsValueType} is primitive: {t.IsPrimitive} is class: {t.IsClass} is collection {t.IsCollection()}");

        }

    }
    public static void SampleMethod2()
    {
        var source = new SourceData { Id = 1, Name = "Test" };
        
        var type = typeof(Sample1);
        var method = type.GetMethod("CreateMapperStatic", BindingFlags.Public | BindingFlags.Static);
        if (method == null)
        {
            throw new InvalidOperationException($"Method CreateMapperStatic not found in type {type.Name}.");
        }
        var genericMethod = method.MakeGenericMethod(typeof(SourceData), typeof(DestinationData));
        var methodCall = Expression.Call(genericMethod);
        var lambda = Expression.Lambda<Func<SimpleMapper<SourceData, DestinationData>>>(methodCall);
        var compiledLambda = lambda.Compile();
        var mapper = compiledLambda.Invoke();
        var destination = mapper.Map5(source);


        Console.WriteLine($"Id: {destination.Id}, Name: {destination.Name}");
    }
    public static void SampleMethod3()
    {
        var source = new SourceData { Id = 1, Name = "Test" };
        var destination = new DestinationData();
        var mapper = Sample1.CreateMapperStatic<SourceData, DestinationData>();
        mapper.Map(source, destination);
        Console.WriteLine($"Id: {destination.Id}, Name: {destination.Name}");
    }

    public static SimpleMapper<TSource, TDestination> CreateMapperStatic<TSource, TDestination>()
        where TSource : notnull, new()
        where TDestination : notnull, new()
    {
        var propertyAssign = _propertyAssignCache.GetOrAdd((typeof(TSource), typeof(TDestination)), (key) =>
        {
            return CreatePropertyAssign<TSource, TDestination>();
        });
        var propertyInitializer = _propertyInitializerCache.GetOrAdd((typeof(TSource), typeof(TDestination)), (key) =>
        {
            return CreatePropertyInitializer<TSource, TDestination>();
        });
        return new SimpleMapper<TSource, TDestination>((Func<TSource, TDestination>)propertyInitializer, (Action<TSource, TDestination>) propertyAssign);
    }
    private static Func<TSource, TDestination> CreatePropertyInitializer<TSource, TDestination>()
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


}