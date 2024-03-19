using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded.Comps;

public class CompAcTempControl : CompTempControl
{
    public bool IndependentTemp;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        var pipeNet = (AcPipeNet)parent.GetComp<CompResourceTrader>().PipeNet;
        if (!IndependentTemp && pipeNet?.ControllerList.FirstOrDefault() != null)
        {
            targetTemperature = pipeNet.ControllerList.First().TargetNetworkTemperature;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref IndependentTemp, "independentTemp");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        var pipeNet = (AcPipeNet)parent.GetComp<CompResourceTrader>().PipeNet;
        if (pipeNet != null && !pipeNet.ControllerList.NullOrEmpty())
        {
            yield return new Command_Action
            {
                action = delegate
                {
                    IndependentTemp = !IndependentTemp;

                    if (!IndependentTemp)
                    {
                        targetTemperature = pipeNet.ControllerList.First().TargetNetworkTemperature;
                    }
                },
                defaultLabel = IndependentTemp ? "Relink" : "Unlink",
                defaultDesc = "todo".Translate(),
                hotKey = KeyBindingDefOf.Misc3,
                icon = IndependentTemp
                    ? ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_LinkWithACControlUnit")
                    : ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_UnlockWithACControlUnit"),
            };
        }

        if (IndependentTemp || pipeNet == null)
        {
            foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
        }
    }
}