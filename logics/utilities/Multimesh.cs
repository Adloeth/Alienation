using Godot;
using System;

public class Multimesh
{
    Rid id;
    Rid instanceID;
    bool useColors, useCustomData;

    public Multimesh(Rid scenario, Mesh mesh, bool useColors = false, bool useCustomData = false)
    {
        this.useColors = useColors;
        this.useCustomData = useCustomData;
        instanceID = RenderingServer.InstanceCreate();
        RenderingServer.InstanceSetVisible(instanceID, true);
        RenderingServer.InstanceSetIgnoreCulling(instanceID, true);
        RenderingServer.InstanceSetTransform(instanceID, Transform3D.Identity);
        RenderingServer.InstanceGeometrySetCastShadowsSetting(instanceID, RenderingServer.ShadowCastingSetting.DoubleSided);
        TransferToScenario(scenario);

        id = RenderingServer.MultimeshCreate();
        RenderingServer.MultimeshSetMesh(id, mesh.GetRid());
        RenderingServer.MultimeshAllocateData(id, 0, RenderingServer.MultimeshTransformFormat.Transform3D, useColors, useCustomData);
        RenderingServer.InstanceSetBase(instanceID, id);

        VisibleInstanceCount = 0;
    }

    ~Multimesh()
    {
        RenderingServer.FreeRid(id);
        RenderingServer.FreeRid(instanceID);
    }

    public int InstanceCount => RenderingServer.MultimeshGetInstanceCount(id);
    public int VisibleInstanceCount { get => RenderingServer.MultimeshGetVisibleInstances(id); set => RenderingServer.MultimeshSetVisibleInstances(id, value); }
    public bool UseColors => useColors;
    public bool UseCustomData => useCustomData;

    public void TransferToScenario(Rid scenario) => RenderingServer.InstanceSetScenario(instanceID, scenario);

    public Transform3D GetTransform(int instance) => RenderingServer.MultimeshInstanceGetTransform(id, instance);
    public void SetTransform(int instance, Transform3D transform) => RenderingServer.MultimeshInstanceSetTransform(id, instance, transform);

    public Color GetColor(int instance) => RenderingServer.MultimeshInstanceGetColor(id, instance);
    public void SetColor(int instance, Color color) => RenderingServer.MultimeshInstanceSetColor(id, instance, color);

    public Color GetCustomData(int instance) => RenderingServer.MultimeshInstanceGetCustomData(id, instance);
    public void SetCustomData(int instance, Color color) => RenderingServer.MultimeshInstanceSetCustomData(id, instance, color);

    public void Resize(int count)
    {
        int smallest = InstanceCount < count ? InstanceCount : count;
        Transform3D[] transforms = new Transform3D[smallest];

        Color[] colors = UseColors ? new Color[smallest] : null;
        Color[] customs = UseCustomData ? new Color[smallest] : null;

        for (int i = 0; i < smallest; i++)
        {
            transforms[i] = RenderingServer.MultimeshInstanceGetTransform(id, i);
            if(UseColors)      colors[i] = RenderingServer.MultimeshInstanceGetColor(id, i);
            if(UseCustomData) customs[i] = RenderingServer.MultimeshInstanceGetCustomData(id, i);
        }

        RenderingServer.MultimeshAllocateData(id, count, RenderingServer.MultimeshTransformFormat.Transform3D, useColors, useCustomData);
        
        for (int i = 0; i < smallest; i++)
        {
            RenderingServer.MultimeshInstanceSetTransform(id, i, transforms[i]);
            if(UseColors)     RenderingServer.MultimeshInstanceSetColor(id, i, colors[i]);
            if(UseCustomData) RenderingServer.MultimeshInstanceSetCustomData(id, i, customs[i]);
        }        
    }
}
