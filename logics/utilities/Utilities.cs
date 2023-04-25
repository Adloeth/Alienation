using Godot;
using System;

public enum Orientation : byte
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public static class Utils
{
    public static float Normalize(this float value, float min, float max) => (value - min) / (max - min);

    public static Vector2I RotateToSize(Vector2I vector, Orientation orientation)
    {
        switch(orientation)
        {
            case Orientation.Up:    return new Vector2I(vector.Y, vector.X);
            case Orientation.Right: return new Vector2I(vector.X, vector.Y);
            case Orientation.Down:  return new Vector2I(vector.Y, vector.X);
            case Orientation.Left:  return new Vector2I(vector.X, vector.Y);
            default: throw new Exception(string.Concat("Illegal orientation (", (byte)orientation, ")"));
        }
    }

    public static Vector2I RotateToOffset(Vector2I vector, Orientation orientation)
    {
        switch(orientation)
        {
            case Orientation.Up:    return new Vector2I();                    
            case Orientation.Right: return new Vector2I();                    
            case Orientation.Down:  return new Vector2I(-vector.Y, -vector.X);
            case Orientation.Left:  return new Vector2I(-vector.X, -vector.Y);
            default: throw new Exception(string.Concat("Illegal orientation (", (byte)orientation, ")"));
        }
    }

    public static float Mod(float x, float y) => (y / 2) - (y / Mathf.Pi) * Mathf.Atan(1.0f / Mathf.Tan((x / y) * Mathf.Pi));

    public static void Resize(this MultiMesh multimesh, int count)
    {
        int smallest = multimesh.InstanceCount < count ? multimesh.InstanceCount : count;
        Transform3D[] transforms = new Transform3D[smallest];

        Color[] colors = null;
        Color[] customs = null;

        if(multimesh.UseColors)
            colors = new Color[smallest];
        if(multimesh.UseCustomData)
            customs = new Color[smallest];

        for (int i = 0; i < smallest; i++)
        {
            transforms[i] = multimesh.GetInstanceTransform(i);
            if(multimesh.UseColors)
                colors[i] = multimesh.GetInstanceColor(i);
            if(multimesh.UseCustomData)
                customs[i] = multimesh.GetInstanceCustomData(i);
        }

        multimesh.InstanceCount = count;
        for (int i = 0; i < smallest; i++)
        {
            multimesh.SetInstanceTransform(i, transforms[i]);
            if(multimesh.UseColors)
                multimesh.SetInstanceColor(i, colors[i]);
            if(multimesh.UseCustomData)
                multimesh.SetInstanceCustomData(i, customs[i]);
        }
    }
}
