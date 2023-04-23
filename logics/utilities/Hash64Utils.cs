using Godot;
using System;
using Standart.Hash.xxHash;

public interface IHash64
{
    public byte[] GetBytes();
}

public static class HashUtils
{
    public static long Hash(string str) { byte[] data = System.Text.Encoding.UTF8.GetBytes(str); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }
    public static long HashASCII(string str) { byte[] data = str.ToAsciiBuffer(); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }

    public static long Hash(ulong val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }
    public static long Hash( long val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }

    public static long Hash(uint val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }
    public static long Hash( int val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }

    public static long Hash(ushort val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }
    public static long Hash( short val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }

    public static long Hash( byte val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }
    public static long Hash(sbyte val) { byte[] data = BitConverter.GetBytes(val); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }

    public static long Hash(IHash64 val) { byte[] data = val.GetBytes(); return (long)(xxHash64.ComputeHash(data, data.Length) & 0x7FFFFFFFFFFFFFFF); }

    public delegate long HashFunc<T>(T obj);
}
