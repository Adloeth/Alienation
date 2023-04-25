using Godot;
using System;

public class BoxCollider
{
    Rid id;
    Rid body;
    Transform3D transform;

    public BoxCollider(Rid space, Vector3 position, Quaternion rotation, Vector3 halfExtents)
    {
        transform = new Transform3D(new Basis(rotation), position);
        
        id = PhysicsServer3D.BoxShapeCreate();
        PhysicsServer3D.ShapeSetData(id, halfExtents);

        body = PhysicsServer3D.BodyCreate();
        PhysicsServer3D.BodySetMode(body, PhysicsServer3D.BodyMode.Static);
        PhysicsServer3D.BodySetState(body, PhysicsServer3D.BodyState.Transform, transform);
        PhysicsServer3D.BodySetSpace(body, space);
        PhysicsServer3D.BodyAddShape(body, id, Transform3D.Identity);
    }

    ~BoxCollider()
    {
        RenderingServer.FreeRid(id);
        RenderingServer.FreeRid(body);
    }

    public Transform3D Transform { get => transform; set => transform = value; }
    public Vector3 Position { get => transform.Origin; set => transform = new Transform3D(transform.Basis, value); }
    public Quaternion Rotation { get => transform.Basis.GetRotationQuaternion(); set => transform = new Transform3D(new Basis(value), transform.Origin); }
    public Vector3 HalfExtents { get => (Vector3)PhysicsServer3D.ShapeGetData(id); set => PhysicsServer3D.ShapeSetData(id, value); }
    public Vector3 Extents { get => (Vector3)PhysicsServer3D.ShapeGetData(id) * 2; set => PhysicsServer3D.ShapeSetData(id, value / 2); }

    public void TransferToSpace(Rid space) => PhysicsServer3D.BodySetSpace(body, space);
}
