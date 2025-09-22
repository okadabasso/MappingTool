using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MappingTool.Mapping;

public class MappingContext
{
    public static readonly int DefaultMaxRecursionDepth = 10;
    public HashSet<object> MappedObjects { get; } = new HashSet<object>(ReferenceEqualityComparer.Instance);

    private int _currentRecursionDepth = 0;
    public int MaxRecursionDepth { get; set; } = DefaultMaxRecursionDepth;
    public void EnterRecursion()
    {
        _currentRecursionDepth++;
        if (_currentRecursionDepth > MaxRecursionDepth)
        {
            throw new InvalidOperationException($"Maximum recursion depth of {MaxRecursionDepth} exceeded.");
        }
    }
    public void ExitRecursion()
    {
        _currentRecursionDepth--;
    }
    public bool IsMapped(object source)
    {
        return MappedObjects.Contains(source);
    }
}


