using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaTemperatureExpanded.Buildings;

public class Building_AcControlUnit : Building
{
    public CompFacility facilityComp;
    public CompPowerTrader powerComp;
    public CompResourceSingleton resourceComp;


    public float TargetNetworkTemperature;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref TargetNetworkTemperature, "targetNetworkTemperature", 0f, false);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        facilityComp = GetComp<CompFacility>();
        resourceComp = GetComp<CompResourceSingleton>();
        powerComp = GetComp<CompPowerTrader>();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        float offset2 = this.RoundedToCurrentTempModeOffset(-10f);
        yield return new Command_Action
        {
            action = delegate { this.InterfaceChangeTargetNetworkTemperature(offset2); },
            defaultLabel = offset2.ToStringTemperatureOffset("F0"),
            defaultDesc = "CommandLowerTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc5,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_LowerNetworkTemperatureGreatly", true)
        };
        float offset3 = this.RoundedToCurrentTempModeOffset(-1f);
        yield return new Command_Action
        {
            action = delegate { this.InterfaceChangeTargetNetworkTemperature(offset3); },
            defaultLabel = offset3.ToStringTemperatureOffset("F0"),
            defaultDesc = "CommandLowerTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc4,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_LowerNetworkTemperature", true)
        };
        yield return new Command_Action
        {
            action = delegate
            {
                this.TargetNetworkTemperature = AmbientTemperature;
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
                this.ThrowCurrentTemperatureText();
            },
            defaultLabel = "CommandResetTemp".Translate(),
            defaultDesc = "CommandResetTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc1,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_ResetNetworkTemperature", true)
        };
        float offset4 = this.RoundedToCurrentTempModeOffset(1f);
        yield return new Command_Action
        {
            action = delegate { this.InterfaceChangeTargetNetworkTemperature(offset4); },
            defaultLabel = "+" + offset4.ToStringTemperatureOffset("F0"),
            defaultDesc = "CommandRaiseTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc2,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_IncreaseNetworkTemperature", true)
        };
        float offset = this.RoundedToCurrentTempModeOffset(10f);
        yield return new Command_Action
        {
            action = delegate { this.InterfaceChangeTargetNetworkTemperature(offset); },
            defaultLabel = "+" + offset.ToStringTemperatureOffset("F0"),
            defaultDesc = "CommandRaiseTempDesc".Translate(),
            hotKey = KeyBindingDefOf.Misc3,
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_IncreaseNetworkTemperatureGreatly", true)
        };
    }

    private float RoundedToCurrentTempModeOffset(float celsiusTemp)
    {
        return GenTemperature.ConvertTemperatureOffset(
            (float)Mathf.RoundToInt(GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode)),
            Prefs.TemperatureMode, TemperatureDisplayMode.Celsius);
    }

    private void InterfaceChangeTargetNetworkTemperature(float offset)
    {
        SoundDefOf.DragSlider.PlayOneShotOnCamera(null);

        this.TargetNetworkTemperature += offset;
        this.TargetNetworkTemperature =
            Mathf.Clamp(this.TargetNetworkTemperature, -273.15f, 1000f);
        this.ThrowCurrentTemperatureText();
        
        //TODO: change target temp of all connected AC units
    }

    private void ThrowCurrentTemperatureText()
    {
        MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), this.Map,
            this.TargetNetworkTemperature.ToStringTemperature("F0"), Color.white, -1f);
    }
}