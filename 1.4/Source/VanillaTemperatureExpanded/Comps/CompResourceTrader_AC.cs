using System;
using System.Linq;
using PipeSystem;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded;

public class CompProperties_ResourceTrader_AC : CompProperties_ResourceTrader
{
    public CompProperties_ResourceTrader_AC()
    {
        compClass = typeof(CompResourceTrader_AC);
    }
}

[StaticConstructorOnStartup]
public class CompResourceTrader_AC : CompResourceTrader
{

    private PipeNetOverlayDrawer pipeNetOverlayDrawer;
    private static Material tooManyMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_TooManyACControlUnits", ShaderDatabase.MetaOverlay);

    private static Material missingCompressorsMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_MissingCompressors", ShaderDatabase.MetaOverlay);
    private static Material missingControlMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_MissingACControlUnit", ShaderDatabase.MetaOverlay);

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void PostDeSpawn(Map map)
    {
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, false);
        base.PostDeSpawn(map);
    }

    public AcPipeNet AcPipeNet => PipeNet as AcPipeNet;

    public void UpdateOverlayHandle()
    {
        if (!parent.Spawned)
        {
            return;
        }

        // var tooManyControls = AcPipeNet.ControllerList.Where(controller => controller.powerComp.PowerOn).Count(controller => parent != controller) > 0;
        var tooManyControls = AcPipeNet.ControllerList.Count(c => c.resourceComp.ResourceOn) > 1;
        var notEnough = !AcPipeNet.ControllerList.Any(c => c.resourceComp.ResourceOn);

        //toggle off all overlays first
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, false);

        if (!ResourceOn)
        {
            if (tooManyControls)
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, true);
            } else if (notEnough)
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, true);
            }
            else if(AcPipeNet.WouldOverload(Consumption))
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, true);
            }
            
        }
    }
}