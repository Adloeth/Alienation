using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class MainManager : Node
{
    private static MainManager instance;
    [Export] private ManagerBase[] managers;

    public override void _Ready()
    {
        if(instance != null)
            throw new Exception("Cannot have multiple MainManagers !");

        instance = this;

        if(managers == null || managers.Length == 0)
        {
            GD.Print("There are no managers, the MainManager was removed.");
            ProcessMode = ProcessModeEnum.Disabled;
            QueueFree();
            return;
        }

        //Order the list by priority and reverse it so when we loop in reverse, managers with the lowest priority will be called first.
        List<ManagerBase> reversedManagers = managers.OrderBy(x => x.Priority).Reverse().ToList();

        //Loop in reverse to remove elements that cannot be initialized from the list.
        for(int i = reversedManagers.Count - 1; i >= 0; i--)
            if(!reversedManagers[i].Initialize())
            {
                GD.PrintErr(string.Concat("Manager '", reversedManagers[i].GetType(), "' was already initialized."));
                reversedManagers.RemoveAt(i);
            }

        //Reverse the list again so managers with the lowest priority have the lowest index.
        reversedManagers.Reverse();
        managers = reversedManagers.ToArray();

        Timer timer = new Timer();
        for(int i = 0; i < managers.Length; i++)
        {
            timer.Reset();
            managers[i].Ready();
            double res = timer.Evaluate();
            GD.Print(string.Concat(managers[i].GetType(), " is ready. (took " + res.FormatDecimal(3) + " ms)"));
        }

        //All managers are ready
        for(int i = 0; i < managers.Length; i++)
            managers[i].AllManagersReady();
    }

    public override void _Process(double delta)
    {
        for(int i = 0; i < managers.Length; i++)
            if(managers[i].IsActive)
                managers[i].Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        for(int i = 0; i < managers.Length; i++)
            if(managers[i].IsActive)
                managers[i].PhysicsProcess(delta);
    }
}