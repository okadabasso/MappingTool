using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace MappingTool.Helpers;

public interface ITypeAnalyzer
{
    public bool IsPrimitiveType(Type type);
    public bool IsNullableType(Type type);
    public bool IsNullablePrimitiveType(Type type) => IsNullableType(type) && IsPrimitiveType(Nullable.GetUnderlyingType(type)!);
    public bool IsNullableComplexType(Type type) => IsNullableType(type) && IsComplexType(Nullable.GetUnderlyingType(type)!);
    public bool IsPrimitiveEnumerableType(Type type);
    public bool IsComplexEnumerableType(Type type);
    public bool IsPrimitiveArrayType(Type type);
    public bool IsNullablePrimitiveArrayType(Type type) => IsPrimitiveArrayType(type) && IsNullableType(type.GetElementType()!);
    public bool IsNullableComplexArrayType(Type type) => IsComplexArrayType(type) && IsNullableType(type.GetElementType()!);
    public bool IsComplexArrayType(Type type);
    public bool IsComplexType(Type type);
    public bool IsDictionaryType(Type type);
    public bool IsNumber(Type type);
    public Type GetEnumerableElementType(Type enumerableType);

}

public class TypeAnalyzer : ITypeAnalyzer
{
            /// <summary>
        /// A cache to store the method information for different types and method names.
        /// This is used to avoid reflection overhead for frequently used methods.
        /// </summary>
        private static readonly ConcurrentDictionary<(Type, string, Type[]), MethodInfo> _methodCache = new();
        /// <summary>
        /// A cache to store the constructor information for different types and parameter types.
        /// </summary>
        private static readonly ConcurrentDictionary<(Type, Type[]), ConstructorInfo> _constructorCache = new();

    /// <summary>
    /// Checks if the specified type is a primitive type.
    /// Primitive types include primitive types, enums, strings, DateTime, and DateTimeOffset.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsPrimitiveType(Type type)
    {
        return type.IsPrimitive
        || type.IsEnum
        || type == typeof(decimal)
        || type == typeof(string)
        || type == typeof(DateTime)
        || type == typeof(DateTimeOffset)
        || type == typeof(Guid)
        || type == typeof(TimeSpan);
    }
    /// <summary>
    /// Checks if the specified type is a nullable type.
    /// Nullable types are generic types with Nullable<> as their generic type definition.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    /// <summary>
    /// Checks if the specified type is a primitive enumerable type.
    /// Primitive enumerable types include arrays and generic collections that implement IEnumerable<T> with primitive or nullable types as their element type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsPrimitiveEnumerableType(Type type)
    {
        // 配列型の場合
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType != null && (IsPrimitiveType(elementType) || IsNullableType(elementType));
        }

        // ジェネリック型であり、IEnumerable<> を実装している場合

        if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
        {
            var elementType = type.GetGenericArguments()[0];
            return IsPrimitiveType(elementType) || IsNullableType(elementType);
        }

        // 非ジェネリック IEnumerable の場合は false を返す
        return false;
    }
    /// <summary>
    /// Checks if the specified type is a complex enumerable type.
    /// Complex enumerable types include arrays and generic collections that implement IEnumerable<T> with complex types as their element type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsComplexEnumerableType(Type type)
    {
        // 配列型の場合
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType != null && (IsPrimitiveType(elementType) || IsNullableType(elementType));
        }

        // ジェネリック型であり、IEnumerable<> を実装している場合
        if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
        {
            var elementType = type.GetGenericArguments()[0];
            return IsComplexType(elementType);
        }

        // 非ジェネリック IEnumerable の場合は false を返す
        return false;
    }
    public bool IsPrimitiveArrayType(Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType != null && (IsPrimitiveType(elementType) || IsNullableType(elementType));
        }
        return false;
    }
    public bool IsComplexArrayType(Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType != null && IsComplexType(elementType);
        }
        return false;
    }
    /// <summary>
    /// Checks if the specified type is a complex type.
    /// Complex types include classes and structs that are not primitive, enum, or nullable types.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsComplexType(Type type)
    {
        return (type.IsClass || type.IsValueType) // Includes classes and structs
            && !type.IsPrimitive
            && !type.IsEnum
            && (!type.IsGenericType || type.IsGenericType && type.GetGenericTypeDefinition() != typeof(Nullable<>));
    }
    /// <summary>
    /// Checks if the specified type is a dictionary type.
    /// Dictionary types are generic types with Dictionary<TKey, TValue> as their generic type definition.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsDictionaryType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    public bool IsNumber(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;

        return t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) ||
               t == typeof(decimal) || t == typeof(short) || t == typeof(byte) ||
               t == typeof(sbyte) || t == typeof(ushort) || t == typeof(uint);   
    }
    /// <summary>
    /// Gets the element type of an enumerable collection.
    /// </summary>
    /// <param name="enumerableType"></param>
    /// <returns></returns>
    public Type GetEnumerableElementType(Type enumerableType)
    {
        if (enumerableType.IsGenericType)
        {
            return enumerableType.GetGenericArguments()[0];
        }
        if (enumerableType.IsArray)
        {
            return enumerableType.GetElementType() ?? typeof(object); // 配列の場合、要素の型は object とする
        }

        return typeof(object); // デフォルトの型
    }
        public MethodInfo GetMethodOrThrow(Type type, string methodName, Type[] parameterTypes, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return _methodCache.GetOrAdd((type, methodName, parameterTypes), key =>
            {
                var method = key.Item1.GetMethod(key.Item2, flags, key.Item3);
                if (method == null)
                {
                    throw new InvalidOperationException($"{type.FullName} Method '{key.Item2}' with parameters ({string.Join(", ", key.Item3.Select(t => t.Name))}) not found in type '{key.Item1.FullName}'.");
                }
                return method;
            });
        }
        public ConstructorInfo GetConstructorOrThrow(Type type, Type[] parameterTypes)
        {
            return _constructorCache.GetOrAdd((type, parameterTypes), key =>
            {
                var constructor = key.Item1.GetConstructor(key.Item2);
                if (constructor == null)
                {
                    throw new InvalidOperationException($"{type.FullName} Constructor with parameters ({string.Join(", ", key.Item2.Select(t => t.Name))}) not found in type '{key.Item1.FullName}'.");
                }
                return constructor;
            });
        }

}