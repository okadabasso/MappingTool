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
            if (t.IsValueType)
            {
                Console.WriteLine($"Type: {t.Name} is value type. primitive: {t.IsPrimitive} nested: {t.IsNested}");
            }
            else if (t.IsPrimitive)
            {
                Console.WriteLine($"Type: {t.Name} is primitive type. primitive: {t.IsPrimitive} nested: {t.IsNested}");
            }
            else if (t.IsClass)
            {
                 var isRecord = t.GetMethods().Any(m => m.Name == "<Clone>$");
                if (isRecord)
                {
                    Console.WriteLine($"Type: {t.Name} is record type. primitive: {t.IsPrimitive} nested: {t.IsNested}");
                }
                else
                {
                    Console.WriteLine($"Type: {t.Name} is class type. primitive: {t.IsPrimitive} nested: {t.IsNested}");
                }
            }
            else
            {
                Console.WriteLine($"Type: {t.Name} is not a recognized type. primitive: {t.IsPrimitive} nested: {t.IsNested}");
            }

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
    public static void SampleMethod3(){
        var list = new List<string> { "a", "b", "c" };
        var mapper = new SimpleMapper<string, string>();
        var result = mapper.Map5(list);
        foreach (var item in result)
        {
            Console.WriteLine(item);
        }
    }
    public static void SampleMethod4(){
        TypeCheck(typeof(int));
        TypeCheck(typeof(string));
        TypeCheck(typeof(DateTime));

        TypeCheck(typeof(int?));
        TypeCheck(typeof(DateTime?));
        TypeCheck(typeof(List<string>));
        
        TypeCheck(typeof(SourceData));
        TypeCheck(typeof(SourceStruct));
        TypeCheck(typeof(SourceRecord));
        TypeCheck(typeof(Foo));
        TypeCheck(typeof(Baz));
    
    }
    public static void SampleMethod1()
    {
        var source = new List<SourceData>(){
            new SourceData { Id = 1, Name = "Test1" },
            new SourceData { Id = 2, Name = "Test2" },
            new SourceData { Id = 3, Name = "Test3" }
        };
        
        var mapper = MapperFactory.CreateMapper<SourceData, DestinationData>();
        var destination = mapper.Map5(source);
        foreach (var item in destination)
        {
            Console.WriteLine($"Id: {item.Id}, Name: {item.Name}");
        }
    }


    class Foo{
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Bar Bar { get; set; } = new Bar();
        public Foo(int id, string name)
        {
            Id = id;
            Name = name;
        }
        public Foo()
        {
            Id = 0;
            Name = string.Empty;
        }

    }
    class Bar{
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    class Baz{
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
    }

}