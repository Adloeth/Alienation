public abstract partial class Manager<T> : ManagerBase where T : Manager<T>
{
    private static T instance;

    public static T Instance => instance;

    public override bool Initialize() 
    {
        if(instance != null)
            return false;

        instance = (T)this;
        return true;
    }
}