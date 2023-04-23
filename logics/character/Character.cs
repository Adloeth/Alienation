using Godot;
using System;
using System.Collections.Generic;

public struct QuickAccess
{
    private Key key;
    private Item item;
}

public class Character
{
    public static readonly List<WearableSlot> wearableSlots = new List<WearableSlot>
    {
        WearableSlot.Helmet,
        WearableSlot.Visor,
        WearableSlot.Torso,
        WearableSlot.Back,
        WearableSlot.Arms,
        WearableSlot.Arms,
        WearableSlot.Gants,
        WearableSlot.Gants,
        WearableSlot.Legs,
        WearableSlot.Legs,
        WearableSlot.Boots,
        WearableSlot.Boots,  
    };

    private Item[] wearedItems;
    private QuickAccess[] accessableItems;

    public bool Equip(Item item, int slot)
    {
        if(!(item.Asset is IWearableAsset wearable))
            return false;

        //Accessories not supported yet
        if(wearable.Slot.HasFlag(WearableSlot.IsAccessory))
            return false;

        //Wrong slot
        if(!wearableSlots[slot].HasFlag(wearable.Slot))
            return false;

        if(wearedItems[slot] != null)
        {
            //Remove effects from character stats
            //Drop item
        }

        wearedItems[slot] = item;
        //Add effects to character stats
        //wearable.Effects;
        return true;
    }
}
