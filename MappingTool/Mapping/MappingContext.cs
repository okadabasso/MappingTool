using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MappingTool.Mapping;

public class MappingContext
{
    public HashSet<object> MappedObjects { get; } = new HashSet<object>(ReferenceEqualityComparer.Instance);
    public bool IsMapped(object source)
    {
        return MappedObjects.Contains(source);
    }
    public void MarkAsMapped(object source)
    {
        MappedObjects.Add(source);
    }
}


