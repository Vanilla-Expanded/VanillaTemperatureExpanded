using System.Text;
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
        var tooManyControls = AcPipeNet.ControllerList.Count(c => c.resourceComp.CanBeOn()) > 1;
        var notEnough = !AcPipeNet.ControllerList.Any(c => c.resourceComp.CanBeOn());

        //toggle off all overlays first
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, false);

        if (!ResourceOn)
        {
            if (tooManyControls)
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, true);
            }
            else if (notEnough)
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, true);
            }
            else
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, true);
            }
        }
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (!ResourceOn)
        {
            var tooManyControls = AcPipeNet.ControllerList.Count(c => c.resourceComp.CanBeOn()) > 1;
            var notEnough = !AcPipeNet.ControllerList.Any(c => c.resourceComp.CanBeOn());

            if (tooManyControls)
            {
                stringBuilder.AppendLine("VTE.TooManyControllers".Translate());
            }
            else if (notEnough)
            {
                stringBuilder.AppendLine("VTE.MissingController".Translate());
            }
            else
            {
                stringBuilder.AppendLine("VTE.MissingCompressors".Translate());
            }
        }
        else
        {
            stringBuilder.AppendLine("VTE.Efficiency".Translate() + ": " + AcPipeNet.Efficiency.ToStringPercent());
            stringBuilder.AppendLine("VTE.TotalProduction".Translate() + ": " + AcPipeNet.Production);
            stringBuilder.Append("VTE.TotalConsumption".Translate() + ": " + AcPipeNet.Consumption);
        }

        return stringBuilder.ToString().Trim();
    }
}