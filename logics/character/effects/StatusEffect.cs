using Godot;
using System;

public class StatusEffectAsset : IMaster
{
    private string id;

    public string ID => id;
}

public class StatusEffect
{
    private ulong globalID;
    private int duration;
    private byte amplitude;

    public StatusEffectAsset Asset => ObjectManager.GetEffect(globalID);
}