using PipeSystem;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded;

public class CompProperties_ResourceSingleton : CompProperties_Resource
{
    public CompProperties_ResourceSingleton()
    {
        compClass = typeof(CompResourceSingleton);
    }
}

[StaticConstructorOnStartup]
public class CompResourceSingleton : CompResource
{
    private PipeNetOverlayDrawer pipeNetOverlayDrawer;

    private static Material tooManyMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_TooManyACControlUnits", ShaderDatabase.MetaOverlay);

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void PostDeSpawn(Map map)
    {
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        base.PostDeSpawn(map);
    }


    public void UpdateOverlayHandle()
    {
        if (!parent.Spawned)
        {
            return;
        }

        var showTooMany = PipeNet is AcPipeNet acPipeNet &&
                          acPipeNet.singletonDict[parent.def].Count > 1;
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, showTooMany);
    }
}