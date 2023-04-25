/// <summary>FromTo type</summary>
global using FTType = System.Int16;

using Godot;
using System;
using System.Collections.Generic;


public class Structure
{
    private Room[] rooms;

    public Structure(params Room[] rooms)
    {
        this.rooms = rooms;
    }

    public Room[] Rooms => rooms;
}

public class Door
{
    private byte type;

    private Orientation orientation;
    private FTType position;

    public Door(byte type, Orientation orientation, FTType position)
    {
        this.type = type;
        this.orientation = orientation;
        this.position = position;
    }

    public byte Type => type;
    public Orientation Orientation => orientation;
    public FTType Position => position;
}

public class Room
{
    private byte type;

    private RoomArea[] areas;

    public Room(params RoomArea[] areas)
    {
        this.areas = areas;
    }

    public IEnumerable<RoomRenderInfo>[] Render()
    {
        IEnumerable<RoomRenderInfo>[] result = new IEnumerable<RoomRenderInfo>[areas.Length];
        for(int i = 0; i < areas.Length; i++)
            result[i] = areas[i].Render();
        return result;
    }
}

[Flags] public enum OpenedSide : byte
{
    None    = 0, 
    Front   = 1, 
    Right   = 2, 
    Back    = 4, 
    Left    = 8,
    Bottom  = 16,
    Top     = 32,

    All = 63
}

public enum RoomPart : byte
{
    Wall,
    WallShort,
    WallLow,
    WallShortLow,

    DoorFrame,

    Floor,
    Ceiling,

    Railing,
    
    Stairs,

    None = 255
}

public struct RoomRenderInfo
{
    public Transform3D transform;
    public RoomPart type;

    public RoomRenderInfo(Transform3D transform, RoomPart type)
    {
        this.transform = transform;
        this.type = type;
    }
}

public class RoomArea
{
    public FromTo fromTo;
    public FTType level;
    public OpenedSide openedSides;
    private Door[] doors;

    public RoomArea(Vector2I start, Vector2I end, Door[] doors, FTType level, OpenedSide openedSides) : this(new FromTo(start, end), doors, level, openedSides) { }

    public RoomArea(FromTo fromTo, Door[] doors, FTType level, OpenedSide openedSides)
    {
        this.doors = doors == null ? new Door[0] : doors;
        this.fromTo = fromTo;
        this.level = level;
        this.openedSides = openedSides;
    }

    private static readonly Basis forwardBasis = new Basis(Vector3.Up, 0);
    private static readonly Basis backwardBasis = new Basis(Vector3.Up, Mathf.Pi);
    private static readonly Basis rightBasis = new Basis(Vector3.Up, Mathf.Pi / 2.0f);
    private static readonly Basis leftBasis = new Basis(Vector3.Up, Mathf.Pi * 3.0f / 2.0f);

    private const float wallHeight = 2.5f;
    private const float wallWidth = 2f;
    private const float wallShortSize = 0.5f;
    private const float totalWallWidth = wallWidth + wallShortSize;
    private const float totalWallHeight = wallHeight + wallShortSize;

    private static Vector3 GetWallPosX(Vector2I position, Vector2I size, FTType level, int i, bool pos)
        => new Vector3(position.X + i, 0, position.Y + (pos ? size.Y - 1 : 0)) * totalWallWidth + Vector3.Up * level * totalWallHeight;

    private static Vector3 GetWallPosY(Vector2I position, Vector2I size, FTType level, int j, bool pos)
        => new Vector3(position.X + (pos ? size.X - 1 : 0), 0, position.Y + j) * totalWallWidth + Vector3.Up * level * totalWallHeight;

    private static bool HasDoorAt(Door[] doors, Orientation orientation, int shift)
    {
        for(int i = 0; i < doors.Length; i++)
            if(doors[i].Orientation == orientation && doors[i].Position == shift)
                return true;
        return false;
    }

    private static void AddWallX(ICollection<RoomRenderInfo> result, Door[] doors, Basis basis, Vector2I position, Vector2I size, FTType level, int i, bool pos, RoomPart wall = RoomPart.Wall, RoomPart shortWall = RoomPart.WallShort, RoomPart doorFrame = RoomPart.DoorFrame)
    {
        Vector3 vec = GetWallPosX(position, size, level, i, pos);
        result.Add(new RoomRenderInfo(new Transform3D(basis, vec), HasDoorAt(doors, pos ? Orientation.Up : Orientation.Down, i) ? doorFrame : wall));
        if(i > 0)
            result.Add(new RoomRenderInfo(new Transform3D(basis, vec - new Vector3(1.25f, 0, 0)), shortWall));
    }

    private static void AddWallY(ICollection<RoomRenderInfo> result, Door[] doors, Basis basis, Vector2I position, Vector2I size, FTType level, int j, bool pos, RoomPart wall = RoomPart.Wall, RoomPart shortWall = RoomPart.WallShort, RoomPart doorFrame = RoomPart.DoorFrame)
    {
        Vector3 vec = GetWallPosY(position, size, level, j, pos);
        result.Add(new RoomRenderInfo(new Transform3D(basis, vec), HasDoorAt(doors, pos ? Orientation.Right : Orientation.Left, j) ? doorFrame : wall));
        if(j > 0)
            result.Add(new RoomRenderInfo(new Transform3D(basis, vec - new Vector3(0, 0, 1.25f)), shortWall));
    }

    public IEnumerable<RoomRenderInfo> Render()
    {
        if(openedSides == OpenedSide.All)
            return null;

        Vector2I position = fromTo.Position, size = fromTo.Size;
        List<RoomRenderInfo> result = new List<RoomRenderInfo>((size.X + size.Y) * 4);

        //Generate floor
        if(!openedSides.HasFlag(OpenedSide.Bottom))
            result.Add(new RoomRenderInfo(
                new Transform3D(Basis.FromScale(new Vector3(size.X, 0, size.Y) * totalWallWidth + Vector3.Up),
                new Vector3(position.X + size.X * 0.5f, 0, position.Y + (size.Y - 2) * 0.5f) * totalWallWidth + Vector3.Up * level * totalWallHeight - new Vector3(1, 0, -1) * totalWallWidth * 0.5f),
                RoomPart.Floor
            ));

        //Generate ceiling
        if(!openedSides.HasFlag(OpenedSide.Top))
            result.Add(new RoomRenderInfo(
                new Transform3D(Basis.FromScale(new Vector3(size.X, 0, size.Y) * totalWallWidth + Vector3.Up),
                new Vector3(position.X + size.X * 0.5f, 0, position.Y + (size.Y - 2) * 0.5f) * totalWallWidth + Vector3.Up * (level + 1) * totalWallHeight - Vector3.Up * wallShortSize - new Vector3(1, 0, -1) * totalWallWidth * 0.5f),
                RoomPart.Ceiling
            ));

        //Generate walls along the X axis
        for(int i = 0; i < size.X; i++)
        {
            //Generate the wall on the back
            if(!openedSides.HasFlag(OpenedSide.Back))
                AddWallX(result, doors, backwardBasis, position, size, level, i, false);
            else if(openedSides.HasFlag(OpenedSide.Bottom))
            {
                AddWallX(result, doors, backwardBasis, position, size, level, i, false, RoomPart.Railing, RoomPart.WallShort, RoomPart.None);
                AddWallX(result, doors, forwardBasis, position - Vector2I.Down, size, level, i, false, RoomPart.None, RoomPart.WallShort, RoomPart.None);
            }
            
            //Generate the wall on the front
            if(!openedSides.HasFlag(OpenedSide.Front))
                AddWallX(result, doors, forwardBasis, position, size, level, i, true);
            //If opened on the front, short walls need to be added at the end of the opening to merge with other RoomAreas
            else
            {
                if(openedSides.HasFlag(OpenedSide.Bottom))
                {
                    AddWallX(result, doors, forwardBasis, position, size, level, i, false, RoomPart.Railing, RoomPart.WallShort, RoomPart.None);
                    AddWallX(result, doors, backwardBasis, position - Vector2I.Up, size, level, i, false, RoomPart.None, RoomPart.WallShort, RoomPart.None);
                }

                result.Add(new RoomRenderInfo(new Transform3D(leftBasis, GetWallPosY(position, size, level, size.Y, false) - new Vector3(0, 0, 1.25f)), RoomPart.WallShort));
                result.Add(new RoomRenderInfo(new Transform3D(rightBasis, GetWallPosY(position, size, level, size.Y, true) - new Vector3(0, 0, 1.25f)), RoomPart.WallShort));
            }

            //If opened at the top, low walls need to be added at the top merge with other RoomAreas
            if(openedSides.HasFlag(OpenedSide.Top))
            {
                Vector3 posNeg = GetWallPosX(position, size, level, i, false);
                Vector3 posPos = GetWallPosX(position, size, level, i, true);
                //Walls
                result.Add(new RoomRenderInfo(new Transform3D(backwardBasis, posNeg + new Vector3(0, wallHeight, 0)), RoomPart.WallLow));
                result.Add(new RoomRenderInfo(new Transform3D(forwardBasis, posPos + new Vector3(0, wallHeight, 0)), RoomPart.WallLow));
                if(i > 0)
                {
                    //Short walls
                    result.Add(new RoomRenderInfo(new Transform3D(backwardBasis, posNeg - new Vector3(1.25f, -wallHeight, 0)), RoomPart.WallShortLow));
                    result.Add(new RoomRenderInfo(new Transform3D(forwardBasis, posPos - new Vector3(1.25f, -wallHeight,0)), RoomPart.WallShortLow));
                }
            }
        }

        //Generate walls along the Y axis
        for(int j = 0; j < size.Y; j++)
        {
            //Generate the wall on the left
            if(!openedSides.HasFlag(OpenedSide.Left))
                AddWallY(result, doors, leftBasis, position, size, level, j, false);
            else if(openedSides.HasFlag(OpenedSide.Bottom))
            {
                AddWallY(result, doors, leftBasis, position, size, level, j, false, RoomPart.Railing, RoomPart.WallShort, RoomPart.None);
                AddWallY(result, doors, backwardBasis, position - Vector2I.Left, size, level, j, false, RoomPart.None, RoomPart.WallShort, RoomPart.None);
            }

            //Generate the wall on the right
            if(!openedSides.HasFlag(OpenedSide.Right))
                AddWallY(result, doors, rightBasis, position, size, level, j, true);
            //If opened on the right, short walls need to be added at the end of the opening to merge with other RoomAreas
            else
            {
                if(openedSides.HasFlag(OpenedSide.Bottom))
                {
                    AddWallY(result, doors, rightBasis, position, size, level, j, false, RoomPart.Railing, RoomPart.WallShort, RoomPart.None);
                    AddWallY(result, doors, backwardBasis, position - Vector2I.Right, size, level, j, false, RoomPart.None, RoomPart.WallShort, RoomPart.None);
                }

                result.Add(new RoomRenderInfo(new Transform3D(backwardBasis, GetWallPosX(position, size, level, size.X, false) - new Vector3(1.25f, 0, 0)), RoomPart.WallShort));
                result.Add(new RoomRenderInfo(new Transform3D(forwardBasis, GetWallPosX(position, size, level, size.X, true) - new Vector3(1.25f, 0, 0)), RoomPart.WallShort));
            }
                        
            //If opened at the top, low walls need to be added at the top merge with other RoomAreas
            if(openedSides.HasFlag(OpenedSide.Top))
            {
                Vector3 posNeg = GetWallPosY(position, size, level, j, false);
                Vector3 posPos = GetWallPosY(position, size, level, j, true);
                //Walls
                result.Add(new RoomRenderInfo(new Transform3D(rightBasis, posPos + new Vector3(0, wallHeight, 0)), RoomPart.WallLow));
                result.Add(new RoomRenderInfo(new Transform3D(leftBasis, posNeg + new Vector3(0, wallHeight, 0)), RoomPart.WallLow));
                //Short Walls
                if(j > 0)
                {
                    result.Add(new RoomRenderInfo(new Transform3D(rightBasis, posPos - new Vector3(0, -wallHeight, 1.25f)), RoomPart.WallShortLow));
                    result.Add(new RoomRenderInfo(new Transform3D(leftBasis, posNeg - new Vector3(0, -wallHeight, 1.25f)), RoomPart.WallShortLow));
                }
            }
        }

        return result;
    }
}

public struct FromTo
{
    public FTType fromX, fromY, toX, toY;

    public FromTo(Vector2I start, Vector2I end)
    {
        if(start.X < end.X)
        {
            fromX = (FTType)start.X;
              toX = (FTType)  end.X;
        }
        else
        {
            fromX = (FTType)  end.X;
              toX = (FTType)start.X;
        }

        if(start.Y < end.Y)
        {
            fromY = (FTType)start.Y;
              toY = (FTType)  end.Y;
        }
        else
        {
            fromY = (FTType)  end.Y;
              toY = (FTType)start.Y;
        }
    }

    public Vector2I Position => new Vector2I(fromX, fromY);
    public Vector2I Size => new Vector2I(toX - fromX, toY - fromY) + Vector2I.One;
}