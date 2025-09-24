using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MappingTool.Mapping;

public class MappingContext
{
    // Backing set for legacy 'IsMapped' checks when PreserveReferences is not used
    public HashSet<object> MappedObjects { get; } = new HashSet<object>(ReferenceEqualityComparer.Instance);

    // Optional map to preserve source->destination mapping when enabled
    public Dictionary<object, object>? PreservedReferences { get; private set; }

    public void EnablePreserveReferences()
    {
        if (PreservedReferences == null)
        {
            PreservedReferences = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
        }
    }

    public bool TryGetMappedDestination(object source, out object? destination)
    {
        if (PreservedReferences != null)
        {
            return PreservedReferences.TryGetValue(source, out destination);
        }
        destination = null;
        return false;
    }

    public void SetMappedDestination(object source, object destination)
    {
        if (PreservedReferences != null)
        {
            PreservedReferences[source] = destination;
            return;
        }
        // Fallback to legacy behavior
        MappedObjects.Add(source);
    }

    public bool IsMapped(object source)
    {
        if (PreservedReferences != null)
        {
            return PreservedReferences.ContainsKey(source);
        }
        return MappedObjects.Contains(source);
    }

    public void MarkAsMapped(object source)
    {
        if (PreservedReferences != null)
        {
            // When preserving references, marking without a destination is not meaningful; add with null placeholder
            PreservedReferences.TryAdd(source, null!);
            return;
        }
        MappedObjects.Add(source);
    }
}


