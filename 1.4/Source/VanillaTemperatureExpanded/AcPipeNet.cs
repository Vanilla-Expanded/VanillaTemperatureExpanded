using System.Collections.Generic;
using PipeSystem;
using Verse;

namespace VanillaTemperatureExpanded;

public class AcPipeNet : PipeNet
{
    // Singletons
    public Dictionary<ThingDef, List<CompResourceSingleton>> singletonDict = new();

    public void AddToSingletonDict(CompResourceSingleton comp)
    {
        if (!singletonDict.ContainsKey(comp.parent.def))
        {
            singletonDict[comp.parent.def] = new List<CompResourceSingleton>();
        }

        singletonDict[comp.parent.def].Add(comp);
    }

    public void RemoveFromSingletonDict(CompResourceSingleton comp)
    {
        singletonDict[comp.parent.def].Remove(comp);
    }

    public override void RegisterComp(CompResource comp)
    {
        base.RegisterComp(comp);
        if (comp is CompResourceSingleton singleton)
        {
            AddToSingletonDict(singleton);
            UpdateSingletonOverlays(singleton);
        }
    }

    public override void UnregisterComp(CompResource comp)
    {
        base.UnregisterComp(comp);
        if (comp is CompResourceSingleton singleton)
        {
            RemoveFromSingletonDict(singleton);
            UpdateSingletonOverlays(singleton);
        }
    }

    private void UpdateSingletonOverlays(CompResourceSingleton singleton)
    {
        foreach (var s in singletonDict[singleton.parent.def])
        {
            s.UpdateOverlayHandle();
        }
    }
}