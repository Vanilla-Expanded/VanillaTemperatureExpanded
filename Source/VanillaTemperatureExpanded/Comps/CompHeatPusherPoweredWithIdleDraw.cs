using RimWorld;
using Verse;

namespace VanillaTemperatureExpanded.Comps;

public class CompHeatPusherPoweredWithIdleDraw : CompHeatPusherPowered
{
    public override bool ShouldPushHeatNow
    {
        get
        {
            var result = base.ShouldPushHeatNow;
            CompProperties_Power props = powerComp.Props;
            if (result)
            {
                powerComp.PowerOutput = 0f - props.PowerConsumption;
            }
            else
            {
                powerComp.PowerOutput = 0f - props.idlePowerDraw;
            }
            return result;
        }
    }
}
