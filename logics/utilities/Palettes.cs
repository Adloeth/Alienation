using Godot;
using System;

public interface IMaster
{
    public string ID { get; }
}

/// <summary>
/// Converts an object containing a master identifier (could be an item, or any entity) to a unique 64-bit global identifier and vice versa, 
/// thus reducing memory usage when a lot of references must be made.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MasterToGlobal<T> where T : IMaster
{
    private T[] items;
    private Dictionary64<string, ulong> masterToGlobal;
    private long count;

    public MasterToGlobal(HashUtils.HashFunc<string> hashFunc, int capacity = 0)
    {
        masterToGlobal = new Dictionary64<string, ulong>(hashFunc, capacity);
        items = new T[capacity > 0 ? capacity : 11];
    }

    private void Resize(long size)
    {
        if(items.LongLength == size)
            return;

        T[] tmp = items;
        items = new T[size];
        if(tmp.LongLength < size)
            Array.Copy(tmp, items, items.LongLength);
        else
        {
            Array.Copy(tmp, items, size);
            count = size;
        }
    }

    public string GetMaster(ulong globalID) => items[globalID].ID;
    public ulong GetGlobal(string masterID) => masterToGlobal[masterID];
    public T GetItem(ulong globalID) => items[globalID];
    public T GetItem(string masterID) => items[masterToGlobal[masterID]];

    public T this[ulong globalID] => items[globalID];
    public ulong this[string masterID] => masterToGlobal[masterID];

    public void AddRange(params T[] newItems)
    {
        if(count + newItems.LongLength > items.LongLength)
            Resize(count + newItems.LongLength);
        
        newItems.CopyTo(items, count);
        count += newItems.LongLength;
    }
}

/// <summary>
/// Converts an the unique 64-bit global identifier to a smaller 16-bit local identifier and vice versa,
/// thus reducing memory usage when a lot of references must be made.
/// </summary>
public class GlobalToLocal
{
    private ulong[] items;
    private Dictionary64<ulong, ushort> globalToLocal;
    private long count;

    public GlobalToLocal(HashUtils.HashFunc<ulong> hashFunc, int capacity = 0)
    {
        globalToLocal = new Dictionary64<ulong, ushort>(hashFunc, capacity);
        items = new ulong[capacity > 0 ? capacity : 11];
    }

    private void Resize(long size)
    {
        if(items.LongLength == size)
            return;

        ulong[] tmp = items;
        items = new ulong[size];
        if(tmp.LongLength < size)
            Array.Copy(tmp, items, items.LongLength);
        else
        {
            Array.Copy(tmp, items, size);
            count = size;
        }
    }

    public ushort GetLocal(ulong globalID) => globalToLocal[globalID];
    public ulong GetGlobal(ushort localID) => items[localID];

    public ushort this[ulong globalID] => globalToLocal[globalID];
    public ulong this[ushort localID] => items[localID];

    public void AddRange(params ulong[] newItems)
    {
        if(count + newItems.LongLength > items.LongLength)
            Resize(count + newItems.LongLength);
        
        newItems.CopyTo(items, count);
        count += newItems.LongLength;
    }
}