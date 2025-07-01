using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded.Buildings;
public class Building_HeaterWithOffset : Building_TempControl
{
    private IntVec3 outputOffsetNorth;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        outputOffsetNorth = def.HasModExtension<TemperatureOutputPositionModExtension>() ? def.GetModExtension<TemperatureOutputPositionModExtension>().offsetNorth : IntVec3.North;
    }

    public override void TickRare()
    {
        if (compPowerTrader.PowerOn)
        {
            IntVec3 intVec = base.Position + outputOffsetNorth.RotatedBy(base.Rotation);
            var flag = PushHeat(intVec);
            CompProperties_Power props = compPowerTrader.Props;
            if (flag)
            {
                compPowerTrader.PowerOutput = 0f - props.PowerConsumption;
            }
            else
            {
                compPowerTrader.PowerOutput = (0f - props.PowerConsumption) * compTempControl.Props.lowPowerConsumptionFactor;
            }
            compTempControl.operatingAtHighPower = flag;
        }
    }

    protected bool PushHeat(IntVec3 cell, float? maxTemperature = null)
    {
        float temp = cell.GetTemperature(Map);
        if (maxTemperature is null)
        {
            maxTemperature = 120f;
        }
        float num = ((temp < 20f) ? 1f : ((!(temp > maxTemperature)) ? Mathf.InverseLerp(maxTemperature.Value, 20f, temp) : 0f));
        float energyLimit = compTempControl.Props.energyPerSecond * num * 4.1666665f;
        float num2 = GenTemperature.ControlTemperatureTempChange(cell, base.Map, energyLimit, compTempControl.TargetTemperature);
        bool flag = !Mathf.Approximately(num2, 0f);
        if (flag)
        {
            cell.GetRoom(Map).Temperature += num2;
        }
        return flag;
    }
}
