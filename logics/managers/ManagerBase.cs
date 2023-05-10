using Godot;

public abstract partial class ManagerBase : Resource
{
    [Export] protected sbyte priority;
    [Export] protected bool active = true;
    public sbyte Priority => priority;
    public bool IsActive { get => active; set => active = value; }

    public abstract bool Initialize();

    /// <summary>
    /// Called after the manager has been initialized. If you want to access other managers to initialize variables, 
    /// you should do it in AllManagersReady() as the order at which Ready() is called can be random.
    /// </summary>
    public virtual void Ready() { }
    /// <summary>
    /// Called after all Ready() from all managers have been called. You are allowed to access variables from other managers in here, because it is
    /// assumed that values are properly initialized in Ready().
    /// </summary>
    public virtual void AllManagersReady() { }
    /// <summary>Called every frame, see Node._Process(double).</summary>
    public virtual void Process(double delta) { }
    /// <summary>Called every physics frame, see Node._PhysicsProcess(double).</summary>
    public virtual void PhysicsProcess(double delta) { }
}