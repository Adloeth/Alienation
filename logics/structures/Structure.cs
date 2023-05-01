/// <summary>FromTo type</summary>
global using FTType = System.Int16;

using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Structure
{
    private List<Room> rooms;

    public Structure(int capacity)
    {
        rooms = new List<Room>(capacity);
    }

    public Structure(params Room[] rooms)
    {
        this.rooms = new List<Room>(rooms);
    }

    public ReadOnlyCollection<Room> Rooms => new ReadOnlyCollection<Room>(rooms);

    /// <summary>
    /// Checks whether a bounds intersect with one of the room's bounds. The room bounds encapsulate the whole room, but that doesn't mean there is an intesection with the room.
    /// This method can be used to rule out rooms to check intersections from.
    /// </summary>
    private void IsNearRoom(Aabbi bounds, ICollection<Room> result)
    {
        for(int i = 0; i < rooms.Count; i++)
            if(rooms[i].BoundingBox.Intersect(bounds))
            {
                GD.Print("Intersected with " + rooms[i].BoundingBox);
                result.Add(rooms[i]);
            }
    }

    public bool IntersectRoom(Aabbi bounds)
    {
        List<Room> result = new List<Room>(rooms.Count);
        IsNearRoom(bounds, result);
        
        if(result.Count == 0)
            return false;

        for(int i = 0; i < result.Count; i++)
        {
            ReadOnlyCollection<RoomArea> areas = result[i].Areas;
            for(int j = 0; j < areas.Count; j++)
                if(areas[j].BoundingBox.Intersect(bounds))
                    return true;
        }

        return false;
    }

    private bool IntersectRooms(Aabbi bounds, ICollection<Room> result)
    {
        List<Room> nearResult = new List<Room>(rooms.Count);
        IsNearRoom(bounds, nearResult);
        GD.Print("IntersectRoom: " + nearResult.Count);
        
        if(nearResult.Count == 0)
            return false;

        for(int i = 0; i < nearResult.Count; i++)
        {
            ReadOnlyCollection<RoomArea> areas = nearResult[i].Areas;
            for(int j = 0; j < areas.Count; j++)
                if(areas[j].BoundingBox.Intersect(bounds))
                {
                    result.Add(nearResult[i]);
                    break;
                }
        }

        return result.Count > 0;
    }

    public Room[] IntersectRooms(FromTo fromTo, FTType level)
    {
        List<Room> result = new List<Room>(rooms.Count);
        IntersectRooms(fromTo.ToBounds(level), result);
        return result.ToArray();
    }

    public Room[] IntersectRooms(FromTo3D fromTo)
    {
        List<Room> result = new List<Room>(rooms.Count);
        IntersectRooms(fromTo.ToBounds(), result);
        return result.ToArray();
    }

    public Issue PlaceRoom(Vector2I start, Vector2I end, FTType level) => PlaceRoom(new FromTo(start, end), level);

    public Issue PlaceRoom(FromTo fromTo, FTType level)
    {
        Room[] intersectedRooms = IntersectRooms(fromTo, level);
        if(intersectedRooms.Length > 0)
            return new InvalidPositionRoomIntersectIssue(intersectedRooms);

        rooms.Add(new Room(fromTo, level, 2));
        return null;
    }

    public Issue ExpandRoom(Vector3I start, Vector3I end) => ExpandRoom(new FromTo3D(start, end));

    public Issue ExpandRoom(FromTo3D fromTo)
    {
        Room[] intersectedRooms = IntersectRooms(fromTo);
        if(intersectedRooms.Length == 0)
            return new InvalidExpandNoRoomIssue();

        if(intersectedRooms.Length > 1)
            return new InvalidExpandRoomIntersectIssue(intersectedRooms);

        Room selectedRoom = intersectedRooms[0];
        selectedRoom.Expand(fromTo);
        GD.Print("Selected room is " + rooms.IndexOf(selectedRoom));

        return null;
    }
}

public class InvalidExpandNoRoomIssue : Issue
{
    public InvalidExpandNoRoomIssue()
    {

    }

    protected override void HandleExec()
    {
        GD.PrintErr(string.Concat("Cannot expand room because it none were selected"));
    }
}

public class InvalidExpandRoomIntersectIssue : Issue
{
    private Room[] intersectedRooms;

    public InvalidExpandRoomIntersectIssue(Room[] intersectedRooms)
    {
        this.intersectedRooms = intersectedRooms;
    }

    public Room[] IntersectedRooms => intersectedRooms;

    protected override void HandleExec()
    {
        string rooms = "";
        for(int i = 1; i < intersectedRooms.Length; i++)
            rooms = string.Concat(rooms, i == 1 ? "{ " : ", ", i);
        rooms = string.Concat(rooms, " }");

        GD.PrintErr(string.Concat("Cannot expand room because it intersected with these other rooms : \n", rooms));
    }
}

public class InvalidPositionRoomIntersectIssue : Issue
{
    private Room[] intersectedRooms;

    public InvalidPositionRoomIntersectIssue(Room[] intersectedRooms)
    {
        this.intersectedRooms = intersectedRooms;
    }

    public Room[] IntersectedRooms => intersectedRooms;

    protected override void HandleExec()
    {
        string rooms = "";
        for(int i = 0; i < intersectedRooms.Length; i++)
            rooms = string.Concat(rooms, i == 0 ? "{ " : ", ", i);
        rooms = string.Concat(rooms, " }");

        GD.PrintErr(string.Concat("Invalid position for room because it intersected with these rooms : \n", rooms));
    }
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

    private List<RoomArea> areas;

    public Room(int capacity = 1)
    {
        areas = new List<RoomArea>(capacity);
    }

    public Room(FromTo fromTo, FTType level, int capacity = 1) : this(capacity)
    {
        areas.Add(new RoomArea(fromTo, level));
    }

    public Room(params RoomArea[] areas)
    {
        this.areas = new List<RoomArea>(areas);
    }

    public ReadOnlyCollection<RoomArea> Areas => new ReadOnlyCollection<RoomArea>(areas);
    public byte Type { get => type; set => type = value; }

    public Aabbi BoundingBox
    {
        get
        {
            if(areas.Count == 0)
                throw new Exception("Room with no areas should not exist.");

            Aabbi bounds = new Aabbi();
            for(int i = 0; i < areas.Count; i++)
            {
                if(i == 0)
                {
                    bounds = areas[0].BoundingBox;
                    continue;
                }
                
                bounds.Expand(areas[i].BoundingBox);
            }
            return bounds;
        }
    }

    public void Expand(FromTo3D fromTo)
    {
        
    }

    public IEnumerable<RoomRenderInfo>[] Render()
    {
        IEnumerable<RoomRenderInfo>[] result = new IEnumerable<RoomRenderInfo>[areas.Count];
        for(int i = 0; i < areas.Count; i++)
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
    private Door[] doors;

    public RoomArea neighbourRight;
    public RoomArea neighbourLeft;
    public RoomArea neighbourTop;
    public RoomArea neighbourBottom;
    public RoomArea neighbourFront;
    public RoomArea neighbourBack;

    public Aabbi BoundingBox => fromTo.ToBounds(level);

    public RoomArea this[int i]
    {
        get
        {
            switch(i)
            {
                case 0: return neighbourRight;
                case 1: return neighbourLeft;
                case 2: return neighbourTop;
                case 3: return neighbourBottom;
                case 4: return neighbourFront;
                case 5: return neighbourBack;
                default: throw new IndexOutOfRangeException();
            }
        }
        set
        {
            switch(i)
            {
                case 0: neighbourRight  = value; break;
                case 1: neighbourLeft   = value; break;
                case 2: neighbourTop    = value; break;
                case 3: neighbourBottom = value; break;
                case 4: neighbourFront  = value; break;
                case 5: neighbourBack   = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }

    public RoomArea(Vector2I start, Vector2I end, FTType level) : this(new FromTo(start, end), level) { }

    public RoomArea(FromTo fromTo, FTType level)
    {
        this.doors = null;
        this.fromTo = fromTo;
        this.level = level;
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

    public OpenedSide CalculateOpenSides()
    {
        OpenedSide result = OpenedSide.None;

        if(neighbourLeft != null)
            result |= OpenedSide.Left;
        
        if(neighbourRight != null)
            result |= OpenedSide.Right;

        if(neighbourTop != null)
            result |= OpenedSide.Top;

        if(neighbourBottom != null)
            result |= OpenedSide.Bottom;

        if(neighbourFront != null)
            result |= OpenedSide.Front;

        if(neighbourBack != null)
            result |= OpenedSide.Back;

        return result;
    }

    private static Vector3 GetWallPosX(Vector2I position, Vector2I size, FTType level, int i, bool pos)
        => new Vector3(position.X + i, 0, position.Y + (pos ? size.Y - 1 : 0)) * totalWallWidth + Vector3.Up * level * totalWallHeight;

    private static Vector3 GetWallPosY(Vector2I position, Vector2I size, FTType level, int j, bool pos)
        => new Vector3(position.X + (pos ? size.X - 1 : 0), 0, position.Y + j) * totalWallWidth + Vector3.Up * level * totalWallHeight;

    private static bool HasDoorAt(Door[] doors, Orientation orientation, int shift)
    {
        if(doors != null)
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
        OpenedSide openedSides = CalculateOpenSides();
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

    public Aabbi ToBounds(FTType level) => new Aabbi(new Vector3I(fromX, level, fromY), new Vector3I(toX - fromX, level, toY - fromY));
}

public struct FromTo3D
{
    public FTType fromX, fromY, fromZ, toX, toY, toZ;

    public FromTo3D(Vector3I start, Vector3I end)
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

        if(start.Z < end.Z)
        {
            fromZ = (FTType)start.Z;
              toZ = (FTType)  end.Z;
        }
        else
        {
            fromZ = (FTType)  end.Z;
              toZ = (FTType)start.Z;
        }
    }

    public Vector3I Position => new Vector3I(fromX, fromY, fromZ);
    public Vector3I Size => new Vector3I(toX - fromX, toY - fromY, toZ - fromZ) + Vector3I.One;

    public Aabbi ToBounds() => new Aabbi(new Vector3I(fromX, fromY, fromZ), new Vector3I(toX - fromX, toY - fromY, toZ - fromZ));
}