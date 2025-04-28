namespace ConsoleApp1.Shared;

using System.Collections.Generic;

using System;
using System.Collections.Concurrent;

public class LRUCache<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    private readonly int _capacity;
    private readonly ConcurrentDictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _cacheMap;
    private readonly LinkedList<(TKey Key, TValue Value)> _lruList;

    public LRUCache(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        }

        _capacity = capacity;
        _cacheMap = new ConcurrentDictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>>();
        _lruList = new LinkedList<(TKey Key, TValue Value)>();
    }

    public TValue Get(TKey key)
    {
        if (_cacheMap.TryGetValue(key, out var node))
        {
            // 使用されたノードをリストの先頭に移動
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            return node.Value.Value;
        }

        throw new KeyNotFoundException("The given key was not present in the cache.");
    }

    public void Add(TKey key, TValue value)
    {
        if (_cacheMap.TryGetValue(key, out var existingNode))
        {
            // 既存のノードを更新し、リストの先頭に移動
            _lruList.Remove(existingNode);
            _lruList.AddFirst(existingNode);
            existingNode.Value = (key, value);
        }
        else
        {
            // 新しいノードを追加
            var newNode = new LinkedListNode<(TKey Key, TValue Value)>((key, value));
            _lruList.AddFirst(newNode);
            _cacheMap[key] = newNode;

            // 容量を超えた場合は最も古いノードを削除
            if (_cacheMap.Count > _capacity)
            {
                var lastNode = _lruList.Last;
                if (lastNode != null)
                {
                    _lruList.RemoveLast();
                    _cacheMap.TryRemove(lastNode.Value.Key, out _);
                }
            }
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_cacheMap.TryGetValue(key, out var node))
        {
            // 使用されたノードをリストの先頭に移動
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            value = node.Value.Value;
            return true;
        }

        value = default!;
        return false;
    }
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (_cacheMap.TryGetValue(key, out var node))
        {
            // キーが存在する場合、値を返し、LRUリストを更新
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            return node.Value.Value;
        }

        // キーが存在しない場合、新しい値を生成してキャッシュに追加
        var value = valueFactory(key);
        Add(key, value);
        return value;
    }

    public void Clear()
    {
        _cacheMap.Clear();
        _lruList.Clear();
    }
}