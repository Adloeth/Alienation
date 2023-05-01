using System;
using System.Collections.Generic;

public class IdProvider
{
    private static IdProvider instance;
    private ulong nextID;
    private Queue<ulong> availableIDs;

    public IdProvider()
    {
        if(instance != null)
            instance = this;
        else
            throw new Exception();

        availableIDs = new Queue<ulong>(20);
    }

    public static ulong NewID
    {
        get
        {
            ulong newID = 0;

            if(instance.availableIDs.Count > 0)
            {
                do
                {
                    newID = instance.availableIDs.Dequeue();
                }
                while(newID < instance.nextID);
            }
            else
            {
                newID = instance.nextID;
                instance.nextID++;
            }

            return newID;
        }
    }

    public static void Free(ulong id)
    {
        if(id == instance.nextID - 1)
            instance.nextID = id;
        else if(id < instance.nextID)
            instance.availableIDs.Enqueue(id);     
    }
}