using Godot;
using System;
using System.Collections.Generic;

public class StructureInteriorInstance
{
    private Rid scenario;
    private Rid space;
    private Multimesh[] multimeshes;

    public StructureInteriorInstance(Rid scenario, Rid space, Multimesh[] multimeshes)
    {
        this.scenario = scenario;
        this.space = space;
        this.multimeshes = multimeshes;
    }

    ~StructureInteriorInstance()
    {
        //RenderingServer.FreeRid(scenario);
        PhysicsServer3D.FreeRid(space);
    }

    public Rid Scenario => scenario;
    public Rid Space => space;
    public Multimesh[] Multimeshes => multimeshes;

}

public partial class StructureRenderer : Node
{
    [Export] private Camera3D playerCamera;
    [Export] private Mesh wallMesh;
    [Export] private Mesh wallShortMesh;
    [Export] private Mesh wallLowMesh;
    [Export] private Mesh wallShortLowMesh;
    [Export] private Mesh doorFrameMesh;
    [Export] private Mesh floorMesh;
    [Export] private Mesh ceilingMesh;

    Rid interiorViewport;

    Dictionary<Structure, StructureInteriorInstance> interiorInstances;

    Structure structTest;

    public override void _Ready()
    {
        interiorInstances = new Dictionary<Structure, StructureInteriorInstance>();

        structTest = new Structure(
            new Room(
                new RoomArea(Vector2I.Zero, new Vector2I(9, -4), new Door[] { new Door(0, Orientation.Left, 2), new Door(0, Orientation.Down, 5) }, 0, OpenedSide.Front | OpenedSide.Right | OpenedSide.Top),
                new RoomArea(Vector2I.Zero, new Vector2I(9, -4), null, 1, OpenedSide.Top | OpenedSide.Bottom | OpenedSide.Back),
                new RoomArea(new Vector2I(0, -5), new Vector2I(9, -7), null, 1, OpenedSide.Top | OpenedSide.Front),
                new RoomArea(Vector2I.Zero, new Vector2I(9, -7), null, 2, OpenedSide.Bottom),
                new RoomArea(new Vector2I(0, 1), new Vector2I(9, 2), null, 0, OpenedSide.Back),
                new RoomArea(new Vector2I(10, 0), new Vector2I(15, -4), null, 0, OpenedSide.Left)
            ),
            new Room(
                new RoomArea(new Vector2I(3,-5), new Vector2I(7, -7), new Door[] { new Door(0, Orientation.Up, 2) }, 0, OpenedSide.None)
            )
        );

        /*Rid box = PhysicsServer3D.BoxShapeCreate();
        PhysicsServer3D.ShapeSetData(box, Vector3.One);

        Rid body = PhysicsServer3D.BodyCreate();
        PhysicsServer3D.BodySetMode(body, PhysicsServer3D.BodyMode.Static);
        PhysicsServer3D.BodySetState(body, PhysicsServer3D.BodyState.Transform, Transform3D.Identity);
        PhysicsServer3D.BodySetSpace(body, worldHolder.GetWorld3D().Space);
        PhysicsServer3D.BodyAddShape(body, box, Transform3D.Identity);

        GD.Print(PhysicsServer3D.ShapeGetType(PhysicsServer3D.BodyGetShape(body, 0)));
        GD.Print(PhysicsServer3D.ShapeGetData(PhysicsServer3D.BodyGetShape(body, 0)));*/

        Instantiate(structTest, playerCamera.GetWorld3D().Scenario);
        Render(structTest);
    }

    private void AddModel(StructureInteriorInstance instance, int multimeshIndex, Transform3D transform)
    {
        Multimesh meshes = instance.Multimeshes[multimeshIndex];

        if(meshes.InstanceCount <= meshes.VisibleInstanceCount + 1)
            meshes.Resize(meshes.VisibleInstanceCount + 1);

        meshes.VisibleInstanceCount++;
        meshes.SetTransform(meshes.VisibleInstanceCount - 1, transform);
    }

    private Multimesh[] GenerateMultimeshes(Rid scenario) => new Multimesh[7]
    {
        new Multimesh(scenario, wallMesh        , false, false),
        new Multimesh(scenario, wallShortMesh   , false, false),
        new Multimesh(scenario, wallLowMesh     , false, false),
        new Multimesh(scenario, wallShortLowMesh, false, false),
        new Multimesh(scenario, doorFrameMesh   , false, false),
        new Multimesh(scenario, floorMesh       , false, false),
        new Multimesh(scenario, ceilingMesh     , false, false)
    };

    public void Instantiate(Structure structure, Rid? scenario = null)
    {
        if(scenario == null) scenario = RenderingServer.ScenarioCreate();
        Rid space = PhysicsServer3D.SpaceCreate();
        interiorInstances.Add(structure, new StructureInteriorInstance(scenario.Value, space, GenerateMultimeshes(scenario.Value)));
    }

    public void Render(Structure structure)
    {
        StructureInteriorInstance instance = interiorInstances[structure];

        foreach(Room room in structure.Rooms)
            Render(instance, room.Render());
    }

    public void Render(StructureInteriorInstance instance, IEnumerable<RoomRenderInfo>[] infos)
    {
        foreach(IEnumerable<RoomRenderInfo> info in infos)
            Render(instance, info);
    }

    public void Render(StructureInteriorInstance instance, IEnumerable<RoomRenderInfo> infos)
    {
        if(infos == null)
            return;

        foreach(RoomRenderInfo info in infos)
        {
            AddModel(instance, (int)info.type, info.transform);
            //GD.Print("Adding wall at " + info.transform.Origin);
        }
    }

}
