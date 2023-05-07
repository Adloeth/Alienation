using Godot;
using System;

public partial class ObjectManager : Manager<ObjectManager>
{
    [Export] private int itemCapacity;
    [Export] private int effectsCapacity;

    private MasterToGlobal<ItemAsset> items;
    private MasterToGlobal<StatusEffectAsset> effects;

    /// <summary>
    /// After the init phase, items cannot be added.
    /// </summary>
    private bool pastInitPhase;

    public override void Ready()
    {
        items = new MasterToGlobal<ItemAsset>(HashUtils.HashASCII, itemCapacity);
        effects = new MasterToGlobal<StatusEffectAsset>(HashUtils.HashASCII, effectsCapacity);
    }

    public static ItemAsset GetItem(ulong globalID) => Instance.items[globalID];
    public static void AddItems(params ItemAsset[] items) 
    { 
        if(Instance.pastInitPhase)
            throw new Exception("Cannot add items after the init phase !");

        Instance.items.AddRange(items);
    }

    public static StatusEffectAsset GetEffect(ulong globalID) => Instance.effects[globalID];
    public static void AddItems(params StatusEffectAsset[] items) 
    { 
        if(Instance.pastInitPhase)
            throw new Exception("Cannot add items after the init phase !");

        Instance.effects.AddRange(items);
    }

    public static void EndInit()
    {
        Instance.pastInitPhase = true;
    }
}