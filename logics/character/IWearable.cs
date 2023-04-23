using Godot;
using System;
using System.Collections.Generic;

[Flags] public enum WearableSlot : ushort
{
    None = 0,
    Helmet = 1,
    Visor = 2,
    Arms = 4,
    Torso = 8,
    Legs = 16,
    Boots = 32,
    Gants = 64,
    Back = 128,
    IsAccessory = 0xFFFF
}

public interface IWearableAsset
{
    public WearableSlot Slot { get; }
    public byte AccessoriesCount { get; }
    public IEnumerable<StatusEffect> Effects { get; }
    public Container Container { get; }
}
