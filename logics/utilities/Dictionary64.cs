using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// A Dictionary whose hashcode is a 64 bit number. The amount of possible element is way higher than anyone would ever need. 
/// The real use of this is to reduce the amount of possible collisions when using large numbers of strings.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class Dictionary64<TKey, TValue>
{
    private struct Entry
    {
        public long hashCode;
        public long next;
        public TKey key;
        public TValue value;
    }

    private long[] buckets;
    private Entry[] entries;
    private long count;
    private long freeList;
    private long freeCount;
    private int version;
    HashUtils.HashFunc<TKey> hashFunc;

    public Dictionary64(HashUtils.HashFunc<TKey> hashFunc, int capacity = 0)
    {
        if (capacity < 0) throw new Exception("Capacity cannot be negative.");
        if (capacity > 0) Initialize(capacity);

        this.hashFunc = hashFunc;
    }

    private void Initialize(int capacity)
    {
        int size = GetPrime(capacity);
        buckets = new long[size];
        for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
        entries = new Entry[size];
        freeList = -1;
    }

    public bool ContainsKey(TKey key) => FindEntry(key) >= 0;

    public bool ContainsValue(TValue value) 
    {
        if (value == null) {
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0 && entries[i].value == null) return true;
            }
        }
        else {
            EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0 && c.Equals(entries[i].value, value)) return true;
            }
        }
        return false;
    }

    public TValue this[TKey key] 
    {
        get {
            long i = FindEntry(key);
            if (i >= 0) return entries[i].value;
            throw new Exception("Key was not present in the dictionary");
        }
        set {
            Insert(key, value, false);
        }
    }

    public void Add(TKey key, TValue value) 
    {
        Insert(key, value, true);
    }

    private long FindEntry(TKey key) 
    {
        if( key == null) throw new Exception("Key cannot be null !");

        if (buckets != null) 
        {
        long hashCode = hashFunc(key);
            for (long i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key.Equals(key)) return i;
            }
        }
        return -1;
    }

    private void Insert(TKey key, TValue value, bool add)
    {
        if (key == null) throw new Exception("Key cannot be null");
        if (buckets == null) Initialize(0);
        long hashCode = hashFunc(key);
        long targetBucket = (long)hashCode % buckets.LongLength;

        for (long i = buckets[targetBucket]; i >= 0; i = entries[i].next) 
        {
            if (entries[i].hashCode == hashCode && entries[i].key.Equals(key)) 
            {
                if (add) throw new Exception("Key is already present in the dictionary.");
                entries[i].value = value;
                version++;
                return;
            }
        }

        long index;
        if(freeCount > 0)
        {
            index = freeList;
            freeList = entries[index].next;
            freeCount--;
        }
        else
        {
            if (count == entries.Length)
            {
                Resize();
                targetBucket = hashCode % buckets.LongLength;
            }
            index = count;
            count++;
        }

        entries[index].hashCode = hashCode;
        entries[index].next = buckets[targetBucket];
        entries[index].key = key;
        entries[index].value = value;
        buckets[targetBucket] = index;
        version++;
    }

    private void Resize() => Resize(ExpandPrime((int)count), false);

    private void Resize(long newSize, bool forceNewHashCodes) 
    {
        long[] newBuckets = new long[newSize];
        for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
        Entry[] newEntries = new Entry[newSize];
        Array.Copy(entries, 0, newEntries, 0, count);
        if(forceNewHashCodes) 
        {
            for (int i = 0; i < count; i++) 
            {
                if(newEntries[i].hashCode != -1) 
                {
                    newEntries[i].hashCode = hashFunc(newEntries[i].key);
                }
            }
        }
        for (int i = 0; i < count; i++) 
        {
            if (newEntries[i].hashCode >= 0) 
            {
                long bucket = newEntries[i].hashCode % newSize;
                newEntries[i].next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }
        }

        buckets = newBuckets;
        entries = newEntries;
    }

    internal static int ExpandPrime(int oldSize)
    {
        int newSize = 2 * oldSize;

        // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newSize > MaxPrimeArrayLength)
            return MaxPrimeArrayLength;

        return GetPrime(newSize);
    }

    internal const int MaxPrimeArrayLength = 0x7FEFFFFD;

    public bool Remove(TKey key) 
    {
        if(key == null) throw new Exception("Key cannot be null !");

        if (buckets != null) 
        {
            long hashCode = hashFunc(key);
            long bucket = hashCode % buckets.Length;
            long last = -1;
            for (long i = buckets[bucket]; i >= 0; last = i, i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key.Equals(key)) 
                {
                    if (last < 0) {
                        buckets[bucket] = entries[i].next;
                    }
                    else {
                        entries[last].next = entries[i].next;
                    }
                    entries[i].hashCode = -1;
                    entries[i].next = freeList;
                    entries[i].key = default(TKey);
                    entries[i].value = default(TValue);
                    freeList = i;
                    freeCount++;
                    version++;
                    return true;
                }
            }
        }
        return false;
    }

    internal static readonly int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

    internal static bool IsPrime(int candidate) 
    {
        if ((candidate & 1) != 0) {
            int limit = (int)Math.Sqrt(candidate);
            for (int divisor = 3; divisor <= limit; divisor += 2) 
            {
                if ((candidate % divisor) == 0) 
                {
                    return false;
                }
            }
            return true;
        }
        return (candidate == 2);
    }

    internal static int GetPrime(int min) 
    {
        for (int i = 0; i < primes.Length; i++) {
            int prime = primes[i];
            if (prime >= min) {
                return prime;
            }
        }

        // Outside of our predefined table. Compute the hard way. 
        for (int i = (min | 1); i < Int32.MaxValue; i += 2) {
            if (IsPrime(i)) {
                return i;
            }
        }
        return min;
    }

}