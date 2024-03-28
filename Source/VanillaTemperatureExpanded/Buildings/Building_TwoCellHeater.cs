using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded.Buildings;
public class Building_TwoCellHeater : Building_HeaterWithOffset
{
    public override void TickRare()
    {
        if (compPowerTrader.PowerOn)
        {
            IntVec3 intVec = base.Position + IntVec3.North.RotatedBy(base.Rotation);
            IntVec3 c = base.Position + IntVec3.North.RotatedBy(base.Rotation) + IntVec3.East.RotatedBy(base.Rotation);
            bool flag = PushHeat(intVec, maxTemperature: 1000);
            bool flag2 = PushHeat(c, maxTemperature: 1000);
            CompProperties_Power props = compPowerTrader.Props;
            if (flag || flag2)
            {
                compPowerTrader.PowerOutput = 0f - props.PowerConsumption;
            }
            else
            {
                compPowerTrader.PowerOutput = (0f - props.PowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
            }
            compTempControl.operatingAtHighPower = flag || flag2;
        }
    }
}
