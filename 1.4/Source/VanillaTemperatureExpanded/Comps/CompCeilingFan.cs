using System.Text;
using UnityEngine;
using VanillaTemperatureExpanded.Buildings;
using Verse;

namespace VanillaTemperatureExpanded.Comps;

public class CompProperties_CeilingFan : CompProperties_HeatPusher
{
    public float minSpinRate;
    public float maxSpinRate;
    public CompProperties_CeilingFan()
    {
        this.compClass = typeof(CompCeilingFan);
    }
}

public class CompCeilingFan : CompHeatPusherPoweredWithIdleDraw
{
    public new CompProperties_CeilingFan Props => base.props as CompProperties_CeilingFan;
    public float spinRate;

    public override void CompTick()
    {
        if (powerComp.PowerOn)
        {
            var shouldPush = ShouldPushHeatNow;
            if (shouldPush && parent.IsHashIntervalTick(60))
            {
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond);
            }
            if (shouldPush)
            {
                spinRate = Mathf.Min(Props.maxSpinRate, spinRate + GetSpinRateOffset);
            }
            else
            {
                spinRate = Mathf.Max(Props.minSpinRate, spinRate - GetSpinRateOffset);
            }
            (parent as Building_CeilingFan).angle += spinRate;
        }
    }

    private float GetSpinRateOffset => Props.maxSpinRate / (3f * 60f);

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref spinRate, "spinRate");
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("PowerConsumptionMode".Translate() + ": ");
        if (ShouldPushHeatNow)
        {
            stringBuilder.Append("PowerConsumptionHigh".Translate().CapitalizeFirst());
        }
        else
        {
            stringBuilder.Append("PowerConsumptionLow".Translate().CapitalizeFirst());
        }
        return stringBuilder.ToString();
    }
}
