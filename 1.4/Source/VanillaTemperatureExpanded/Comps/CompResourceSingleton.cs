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

    private static Material missingCompressorsMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_MissingCompressors", ShaderDatabase.MetaOverlay);

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
    
    public AcPipeNet AcPipeNet => PipeNet as AcPipeNet;

    public void UpdateOverlayHandle()
    {
        if (!parent.Spawned)
        {
            return;
        }

        var tooManyControls = AcPipeNet != null &&
                              AcPipeNet.singletonDict[parent.def].Count > 1;

        //toggle off all overlays first
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, false);

        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, tooManyControls);
        if (!tooManyControls)
        {
            pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, AcPipeNet is { Efficiency: 0f });
        }
    }
}