using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using VanillaTemperatureExpanded.Comps;
using Verse;
using Verse.Sound;

namespace VanillaTemperatureExpanded.Buildings;

[StaticConstructorOnStartup]
public class Building_AcControlUnit : Building
{
    public CompFacility facilityComp;
    public CompPowerTrader powerComp;
    public CompResourceSingleton resourceComp;


    public float TargetNetworkTemperature = 21f;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref TargetNetworkTemperature, "targetNetworkTemperature");
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

        if(resourceComp.ResourceOn)
        {
            float offset2 = RoundedToCurrentTempModeOffset(-10f);
            yield return new Command_Action
            {
                action = delegate { InterfaceChangeTargetNetworkTemperature(offset2); },
                defaultLabel = offset2.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandLowerTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc5,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_LowerNetworkTemperatureGreatly")
            };
            float offset3 = RoundedToCurrentTempModeOffset(-1f);
            yield return new Command_Action
            {
                action = delegate { InterfaceChangeTargetNetworkTemperature(offset3); },
                defaultLabel = offset3.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandLowerTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc4,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_LowerNetworkTemperature")
            };
            yield return new Command_Action
            {
                action = delegate
                {
                    TargetNetworkTemperature = AmbientTemperature;
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    ThrowCurrentTemperatureText();
                },
                defaultLabel = "CommandResetTemp".Translate(),
                defaultDesc = "CommandResetTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_ResetNetworkTemperature")
            };
            float offset4 = RoundedToCurrentTempModeOffset(1f);
            yield return new Command_Action
            {
                action = delegate { InterfaceChangeTargetNetworkTemperature(offset4); },
                defaultLabel = "+" + offset4.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandRaiseTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc2,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_IncreaseNetworkTemperature")
            };
            float offset = RoundedToCurrentTempModeOffset(10f);
            yield return new Command_Action
            {
                action = delegate { InterfaceChangeTargetNetworkTemperature(offset); },
                defaultLabel = "+" + offset.ToStringTemperatureOffset("F0"),
                defaultDesc = "CommandRaiseTempDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc3,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Gizmo_IncreaseNetworkTemperatureGreatly")
            };
        }
    }

    private float RoundedToCurrentTempModeOffset(float celsiusTemp)
    {
        return GenTemperature.ConvertTemperatureOffset(
            Mathf.RoundToInt(GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode)),
            Prefs.TemperatureMode, TemperatureDisplayMode.Celsius);
    }

    private void InterfaceChangeTargetNetworkTemperature(float offset)
    {
        SoundDefOf.DragSlider.PlayOneShotOnCamera();

        TargetNetworkTemperature += offset;
        TargetNetworkTemperature =
            Mathf.Clamp(TargetNetworkTemperature, -273.15f, 1000f);

        foreach (var compResourceTrader in resourceComp.AcPipeNet.receivers)
        {
            var tempControl = compResourceTrader.parent.GetComp<CompAcTempControl>();
            if (tempControl is { IndependentTemp: false })
            {
                tempControl.targetTemperature = TargetNetworkTemperature;
            }
        }

        ThrowCurrentTemperatureText();
    }

    private void ThrowCurrentTemperatureText()
    {
        MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), Map,
            TargetNetworkTemperature.ToStringTemperature("F0"), Color.white);

        foreach (var compResourceTrader in resourceComp.AcPipeNet.receivers)
        {
            var tempControl = compResourceTrader.parent.GetComp<CompAcTempControl>();
            if (tempControl is { IndependentTemp: false })
            {
                MoteMaker.ThrowText(tempControl.parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), Map,
                    TargetNetworkTemperature.ToStringTemperature("F0"), Color.white);
            }
        }
    }
    
    //Draw Methods Related Methods and Fields
    private static readonly Vector2 BarSize = new(0.47f, 0.315f);
    private static readonly Vector3 BarOffset = Vector3.up * 0.1f + Vector3.right * 0.62f + Vector3.back * 0.625f;

    private static readonly Material HighEffFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(153f/255f, 143f/255f, 85f/255f));
    private static readonly Material MedEffFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(153f/255f, 113f/255f, 86f/255f));
    private static readonly Material LowEffFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(153f/255f, 86f/255f, 86f/255f));

    private static readonly Material BatteryBarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));
    
    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        GenDraw.FillableBarRequest fillableBarRequest = default(GenDraw.FillableBarRequest);
        fillableBarRequest.center = drawLoc + BarOffset;
        fillableBarRequest.size = BarSize;
        fillableBarRequest.fillPercent = FillPct();
        fillableBarRequest.filledMat = BarMat();
        fillableBarRequest.unfilledMat = BatteryBarUnfilledMat;
        fillableBarRequest.margin = 0.06f;
        Rot4 rotation = Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        fillableBarRequest.rotation = rotation;
        GenDraw.DrawFillableBar(fillableBarRequest);
    }
    
    
    private float FillPct()
    {
        return resourceComp.AcPipeNet.Efficiency switch
        {
            0f => 0f,
            <= 1f => Math.Max(0.1f,resourceComp.AcPipeNet.Efficiency * 0.5f),
            > 1f => resourceComp.AcPipeNet.Efficiency - 0.5f,
            _ => 0.5f
        };
    }
    
    private Material BarMat()
    {
        return resourceComp.AcPipeNet.Efficiency switch
        {
            < 0.75f => LowEffFilledMat,
            > 1f => HighEffFilledMat,
            _ => MedEffFilledMat
        };
    }
}