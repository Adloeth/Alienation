using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Container
{
    private SlotGroup[] groups;
}

public class SlotGroup
{
    private byte properties;
    private byte width;
    private byte height;
    private Item[] items;

    /// <summary>Items placed in quick access slot group can assign any key to them in order to equip them faster. </summary>
    /// <value></value>
    public bool QuickAccess
    {
        get => (properties & 0b00001111) > 0;

        set
        {
            if(value) properties |= 0b00001111;
            else      properties &= 0b11110000;
        }
    }

    /// <summary>Items placed in hidden slot groups will not appear when searched unless the player hit "search", which will take some time.</summary>
    /// <value></value>
    public bool Hidden
    {
        get => (properties & 0b11110000) > 0;

        set
        {
            if(value) properties |= 0b11110000;
            else      properties &= 0b00001111;
        }
    }

    private bool IsInItem(Item item, Vector2I currSize, byte x, byte y) 
    {
        Vector2I size = Utils.RotateToSize(item.CurrentSize, item.Orientation);
        return x >= item.X && x < item.X + size.X && y >= item.Y && y < item.Y + size.Y &&
               x + currSize.X >= item.X && x + currSize.X < item.X + size.X && y + currSize.Y >= item.Y && y + currSize.Y < item.Y + size.Y;
    }

    /// <summary>
    /// Optimized version of <see cref="IsInItem(Item,byte,byte)"/> where the ObjectManager is not called.
    /// </summary>
    /// <param name="pairs"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsInItem(byte itemX, byte itemY, Vector2I itemSize, Vector2I currSize, byte x, byte y) 
        => x >= itemX && x < itemX + itemSize.X && y >= itemY && y < itemY + itemSize.Y &&
           x + currSize.X >= itemX && x + currSize.X < itemX + itemSize.X && y + currSize.Y >= itemY && y + currSize.Y < itemY + itemSize.Y;

    public bool ItemInSlot(Vector2I size, byte x, byte y)
    {
        foreach(Item item in items)
            if(IsInItem(item, size, x, y))
                return true;
        return false;
    }

    /// <summary>
    /// Optimized version of <see cref="ItemInSlot(byte,byte)"/> where the ObjectManager is not called.
    /// </summary>
    /// <param name="pairs"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool ItemInSlot(IEnumerable<Tuple<Item, ItemAsset>> pairs, Vector2I size, byte x, byte y)
    {
        foreach(Tuple<Item, ItemAsset> pair in pairs)
            if(IsInItem(pair.Item1.X, pair.Item1.Y, Utils.RotateToSize(pair.Item2.Size, pair.Item1.Orientation), size, x, y))
                return true;
        return false;
    }

    public bool IsItemOut(Item item)
    {
        Vector2I size = item.CurrentSize;
        return item.X > width || item.Y > height || item.X + size.X > width || item.Y + size.Y > height;
    }

    public bool IsItemOut(Vector2I size, byte x, byte y) => x > width || y > height || x + size.X > width || y + size.Y > height;

    public bool ItemPositionValid(IEnumerable<Tuple<Item, ItemAsset>> pairs, Vector2I size, byte x, byte y) => !IsItemOut(size, x, y) && ItemInSlot(pairs, size, x, y);

    public void Sort()
    {
        //Optimize access to the ObjectManager by collecting all ItemAssets once
        Tuple<Item, ItemAsset>[] itemsToSort = new Tuple<Item, ItemAsset>[items.Length];
        for(int i = 0; i < itemsToSort.Length; i++)
            itemsToSort[i] = new Tuple<Item, ItemAsset>(items[i], items[i].Asset);

        IEnumerable<Tuple<Item, ItemAsset>> sortedItems = itemsToSort.OrderBy(x => x.Item2.Area);
        items = new Item[items.Length];

        int index = 0;
        foreach(Tuple<Item, ItemAsset> pair in sortedItems)
        {
            Vector2I upSize = Utils.RotateToSize(pair.Item2.Size, Orientation.Up);
            Vector2I rightSize = Utils.RotateToSize(pair.Item2.Size, Orientation.Right);

            bool placed = false;
            for(byte i = 0; i < width; i++)
            {
                for(byte j = 0; j < height; j++)
                {
                    if(ItemPositionValid(sortedItems, upSize, i, j))
                    {
                        pair.Item1.Orientation = Orientation.Up;
                        pair.Item1.X = i;
                        pair.Item1.Y = j;
                        items[index++] = pair.Item1;
                        placed = true;
                    }
                    else if(ItemPositionValid(sortedItems, rightSize, i, j))
                    {
                        pair.Item1.Orientation = Orientation.Right;
                        pair.Item1.X = i;
                        pair.Item1.Y = j;
                        items[index++] = pair.Item1;
                        placed = true;
                    }

                    if(placed) break;
                }

                if(placed) break;
            }

            if(placed) continue;

            GD.Print("Could not place item '" + pair.Item1 + "' while sorting !");
        }
    }
}