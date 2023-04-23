using Godot;
using System;

public class ObjectManager
{
    private static ObjectManager instance;

    MasterToGlobal<ItemAsset> items;
    MasterToGlobal<StatusEffectAsset> effects;

    /// <summary>
    /// After the init phase, items cannot be added.
    /// </summary>
    private bool pastInitPhase;

    public ObjectManager(int itemCapacity = 0, int effectsCapacity = 0)
    {
        if(instance != null)
            throw new Exception("ObjectManager already exist !");

        instance = this;
        items = new MasterToGlobal<ItemAsset>(HashUtils.HashASCII, itemCapacity);
        effects = new MasterToGlobal<StatusEffectAsset>(HashUtils.HashASCII, effectsCapacity);
    }

    public static ObjectManager Get => instance;

    public static ItemAsset GetItem(ulong globalID) => Get.items[globalID];
    public static void AddItems(params ItemAsset[] items) 
    { 
        if(Get.pastInitPhase)
            throw new Exception("Cannot add items after the init phase !");

        Get.items.AddRange(items);
    }

    public static StatusEffectAsset GetEffect(ulong globalID) => Get.effects[globalID];
    public static void AddItems(params StatusEffectAsset[] items) 
    { 
        if(Get.pastInitPhase)
            throw new Exception("Cannot add items after the init phase !");

        Get.effects.AddRange(items);
    }

    public static void EndInit()
    {
        Get.pastInitPhase = true;
    }
}