using ConsoleAppFramework;
using Experimental1.Data;
namespace Experimental1.Commands;

[ConsoleAppFramework.RegisterCommands("typecheck")]
class TypeCheckCommand
{
    [Command("execute")]
    public void Execute()
    {
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

    }
    [Command("checkuri")]
    public void CheckUri()
    {
        TypeCheck(typeof(Uri));
        var constructors = typeof(Uri).GetConstructors();
        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            Console.WriteLine($"Constructor: {constructor.Name}");
            foreach (var parameter in parameters)
            {
                Console.WriteLine($"Parameter: {parameter.Name} Type: {parameter.ParameterType.Name}");
            }
        }
    }
    private static void TypeCheck(Type t)
    {
        if (t.IsGenericType)
        {
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
        else
        {
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

}