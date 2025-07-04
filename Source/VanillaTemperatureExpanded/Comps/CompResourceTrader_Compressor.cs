using System.Text;
using PipeSystem;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded;

[StaticConstructorOnStartup]
public class CompResourceTrader_Compressor : CompResourceTrader
{
    private PipeNetOverlayDrawer pipeNetOverlayDrawer;

    public override bool CanBeOn()
    {
        return base.CanBeOn() && !RoofUtility.IsAnyCellUnderRoof(parent);
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        if (ResourceOn && RoofUtility.IsAnyCellUnderRoof(parent))
        {
            this.ResourceOn = false;
            UpdateOverlayHandle();
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
        base.PostSpawnSetup(respawningAfterLoad);
        UpdateOverlayHandle();
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, roofedBadMat, false);
        base.PostDeSpawn(map,mode);
    }

    // private PipeNetOverlayDrawer pipeNetOverlayDrawer;
    private static Material tooManyMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_TooManyACControlUnits", ShaderDatabase.MetaOverlay);

    private static Material missingControlMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_MissingACControlUnit", ShaderDatabase.MetaOverlay);

    private static Material roofedBadMat =
        MaterialPool.MatFrom("UI/Overlays/Overlay_RoofedBad", ShaderDatabase.MetaOverlay);

    public AcPipeNet AcPipeNet => PipeNet as AcPipeNet;

    public void UpdateOverlayHandle()
    {
        if (!parent.Spawned)
        {
            return;
        }

        var tooManyControls = AcPipeNet.ControllerList.Count(c => c.resourceComp.CanBeOn()) > 1;
        var notEnough = !AcPipeNet.ControllerList.Any(c => c.resourceComp.CanBeOn());
        var anyRoofed = RoofUtility.IsAnyCellUnderRoof(parent);

        //toggle off all overlays first
        pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, false);
        pipeNetOverlayDrawer.TogglePulsing(parent, roofedBadMat, false);

        if (!ResourceOn && CanBeOn())
        {
            if (anyRoofed)
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, roofedBadMat, true);
            }
            else if (tooManyControls)
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, tooManyMat, true);
            }
            else if (notEnough)
            {
                pipeNetOverlayDrawer.TogglePulsing(parent, missingControlMat, true);
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
            var anyRoofed = RoofUtility.IsAnyCellUnderRoof(parent);

            if (anyRoofed)
            {
                stringBuilder.AppendLine("VTE.CompressorCovered".Translate());
            }
            else if (tooManyControls)
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
            stringBuilder.AppendLine("VTE.Production".Translate() + ": " + Consumption * -1);
            stringBuilder.AppendLine("VTE.TotalProduction".Translate() + ": " + AcPipeNet.Production);
            stringBuilder.AppendLine("VTE.TotalConsumption".Translate() + ": " + AcPipeNet.Consumption);
        }

        return stringBuilder.ToString().Trim();
    }
}