using Godot;
using System;

public class ItemAsset : IMaster
{
    private string id;
    private Vector2I size;

    public string ID => id;
    public Vector2I Size => size;
    public int Area => size.X * size.Y;
}

/// <summary>
/// Item is the base class used for all items stored in containers. They get most of their properties (inventory size, mass, etc...) from an ItemAsset. 
/// Special items can be inherited from this class.
/// </summary>
public class Item
{
    private ulong globalID;
    private Orientation orientation;
    private byte x;
    private byte y;

    public ulong ID => globalID;
    public ItemAsset Asset => ObjectManager.GetItem(globalID);

    public Vector2I CurrentSize => Utils.RotateToSize(Asset.Size, orientation);

    public Orientation Orientation { get => orientation; set => orientation = value; }
    public byte X { get => x; set => x = value; }
    public byte Y { get => y; set => y = value; }
    public Vector2I Position { get => new Vector2I(x, y); set { x = (byte)value.X; y = (byte)value.Y; } }
}