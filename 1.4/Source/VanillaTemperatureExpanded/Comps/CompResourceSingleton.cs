using System.Text;
using PipeSystem;
using UnityEngine;
using VanillaTemperatureExpanded.Buildings;
using Verse;

namespace VanillaTemperatureExpanded.Comps;

public class CompProperties_ResourceSingleton : CompProperties_ResourceTrader
{
    public CompProperties_ResourceSingleton()
    {
        compClass = typeof(CompResourceSingleton);
    }
}

[StaticConstructorOnStartup]
public class CompResourceSingleton : CompResourceTrader
{
    private PipeNetOverlayDrawer pipeNetOverlayDrawer;
    //TODO: Nice to have: Move this to Building_AcControlUnit to make this generic (for reuse purposes) 

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
        pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, false);
        base.PostDeSpawn(map);
    }

    public AcPipeNet AcPipeNet => PipeNet as AcPipeNet;

    public void UpdateOverlayHandle()
    {
        if (!parent.Spawned)
        {
            return;
        }

        var tooManyControls =
            AcPipeNet.ControllerList.Count(controller => parent != controller && controller.resourceComp.CanBeOn()) > 0;

        //toggle off all overlays first
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, false);

        if (CanBeOn())
        {
            if (tooManyControls)
                pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, true);
            else
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, missingCompressorsMat, AcPipeNet is { Efficiency: 0f });
            }
        }
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (!ResourceOn)
        {
            if (AcPipeNet.ControllerList.Count(controller =>
                    parent != controller && controller.resourceComp.CanBeOn()) > 0)
            {
                stringBuilder.AppendLine("VTE.TooManyControllers".Translate());
            }
            else if (AcPipeNet.Efficiency == 0f)
            {
                stringBuilder.AppendLine("VTE.MissingCompressors".Translate());
            }
        }
        else
        {
            stringBuilder.AppendLine("VTE.NetworkTargetTemp".Translate() + ": " +
                                     ((Building_AcControlUnit)parent).TargetNetworkTemperature
                                     .ToStringTemperature("F0"));
            stringBuilder.AppendLine("VTE.Efficiency".Translate() + ": " + AcPipeNet.Efficiency * 100 + "%");
            stringBuilder.AppendLine("VTE.Production".Translate() + ": " + Consumption * -1);
            stringBuilder.AppendLine("VTE.TotalProduction".Translate() + ": " + AcPipeNet.Production);
            stringBuilder.AppendLine("VTE.TotalConsumption".Translate() + ": " + AcPipeNet.Consumption);
        }

        return stringBuilder.ToString().Trim();
    }
}