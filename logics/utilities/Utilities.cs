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

public struct Aabbi
{
    public Vector3I position;
    public Vector3I size;

    public Aabbi(Vector3I position, Vector3I size)
    {
        this.position = position;
        this.size = size;
    }

    public Vector3I Min => position;
    public Vector3I Max => position + size;

    public override string ToString() => string.Concat('(', Min, ", ", Max, ')');

    public bool Contains(Vector3I point) => point.X >= position.X && point.Y >= position.Y && point.Z >= position.Z 
        && point.X < position.X + size.X && point.Y < position.Y + size.Y && point.Z < position.Z + size.Z;

    public bool Intersect(Aabbi bounds) => 
        position.X          <= bounds.position.X + bounds.size.X &&
        position.X + size.X >= bounds.position.X                 &&
        position.Y          <= bounds.position.Y + bounds.size.Y &&
        position.Y + size.Y >= bounds.position.Y                 &&
        position.Z          <= bounds.position.Z + bounds.size.Z &&
        position.Z + size.Z >= bounds.position.Z                   ;

    public Aabbi Encapsulate(Vector3I point) 
    {
        int x = position.X, y = position.Y, z = position.Z;
        int sX = size.X, sY = size.Y, sZ = size.Z;

        if(x > point.X) x = point.X;
        if(y > point.Y) y = point.Y;
        if(z > point.Z) z = point.Z;

        if(x + sX < point.X) sX = point.X - x;
        if(y + sY < point.Y) sY = point.Y - y;
        if(z + sZ < point.Z) sZ = point.Z - z;

        return new Aabbi(new Vector3I(x, y, z), new Vector3I(sX, sY, sZ));
    }

    public Aabbi Expand(Aabbi bounds) => Encapsulate(bounds.Min).Encapsulate(bounds.Max);
}