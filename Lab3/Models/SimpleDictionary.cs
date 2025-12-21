using System.Collections;

namespace Models;

public class SimpleDictionary<TKey, TValue> : IDictionary<TKey, TValue>, 
                                              IEnumerable<KeyValuePair<TKey, TValue>>,
                                              IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private List<KeyValuePair<TKey, TValue>>[] _buckets;
    private int _count;
    private const int DefaultCapacity = 16;

    public SimpleDictionary() : this(DefaultCapacity) { }

    public SimpleDictionary(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        
        _buckets = new List<KeyValuePair<TKey, TValue>>[capacity];
        for (int i = 0; i < capacity; i++)
        {
            _buckets[i] = new List<KeyValuePair<TKey, TValue>>();
        }
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue? value))
                return value;
            throw new KeyNotFoundException();
        }
        set
        {
            int bucketIndex = GetBucketIndex(key);
            var bucket = _buckets[bucketIndex];

            for (int i = 0; i < bucket.Count; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(bucket[i].Key, key))
                {
                    bucket[i] = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }
            }

            bucket.Add(new KeyValuePair<TKey, TValue>(key, value));
            _count++;
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>();
            foreach (var bucket in _buckets)
            {
                foreach (var pair in bucket)
                {
                    keys.Add(pair.Key);
                }
            }
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>();
            foreach (var bucket in _buckets)
            {
                foreach (var pair in bucket)
                {
                    values.Add(pair.Value);
                }
            }
            return values;
        }
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    public int Count => _count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException("Элемент с таким ключом уже существует");
        
        int bucketIndex = GetBucketIndex(key);
        _buckets[bucketIndex].Add(new KeyValuePair<TKey, TValue>(key, value));
        _count++;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        foreach (var bucket in _buckets)
        {
            bucket.Clear();
        }
        _count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        int bucketIndex = GetBucketIndex(item.Key);
        var bucket = _buckets[bucketIndex];
        
        foreach (var pair in bucket)
        {
            if (EqualityComparer<TKey>.Default.Equals(pair.Key, item.Key) &&
                EqualityComparer<TValue>.Default.Equals(pair.Value, item.Value))
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        int bucketIndex = GetBucketIndex(key);
        var bucket = _buckets[bucketIndex];
        
        foreach (var pair in bucket)
        {
            if (EqualityComparer<TKey>.Default.Equals(pair.Key, key))
            {
                return true;
            }
        }
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < _count)
            throw new ArgumentException("Недостаточно места в массиве");

        int currentIndex = arrayIndex;
        foreach (var bucket in _buckets)
        {
            foreach (var pair in bucket)
            {
                array[currentIndex++] = pair;
            }
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var bucket in _buckets)
        {
            foreach (var pair in bucket)
            {
                yield return pair;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Remove(TKey key)
    {
        int bucketIndex = GetBucketIndex(key);
        var bucket = _buckets[bucketIndex];
        
        for (int i = 0; i < bucket.Count; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(bucket[i].Key, key))
            {
                bucket.RemoveAt(i);
                _count--;
                return true;
            }
        }
        
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        int bucketIndex = GetBucketIndex(item.Key);
        var bucket = _buckets[bucketIndex];
        
        for (int i = 0; i < bucket.Count; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(bucket[i].Key, item.Key) &&
                EqualityComparer<TValue>.Default.Equals(bucket[i].Value, item.Value))
            {
                bucket.RemoveAt(i);
                _count--;
                return true;
            }
        }
        
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int bucketIndex = GetBucketIndex(key);
        var bucket = _buckets[bucketIndex];
        
        foreach (var pair in bucket)
        {
            if (EqualityComparer<TKey>.Default.Equals(pair.Key, key))
            {
                value = pair.Value;
                return true;
            }
        }
        
        value = default!;
        return false;
    }

    private int GetBucketIndex(TKey key)
    {
        int hashCode = key.GetHashCode();
        if (hashCode < 0) hashCode = -hashCode; 
        return hashCode % _buckets.Length;
    }
}