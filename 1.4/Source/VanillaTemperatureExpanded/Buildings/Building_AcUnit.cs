using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded.Buildings;

public class Building_AcUnit : Building_TempControl
{
    private CompResourceTrader resourceComp;
    public AcPipeNet AcPipeNet => resourceComp.PipeNet as AcPipeNet;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        resourceComp = GetComp<CompResourceTrader>();
    }

    public override void TickRare()
    {
        if (compPowerTrader.PowerOn && resourceComp.ResourceOn)
        {
            var coolerVec = Position + IntVec3.North.RotatedBy(Rotation);
            var highPowerMode = false;
            if (!coolerVec.Impassable(Map))
            {
                var vecTemperature = coolerVec.GetTemperature(Map);
                var energyDirection = compTempControl.targetTemperature < vecTemperature ? 1 : -1;
                var energyLimit = compTempControl.Props.energyPerSecond * 4.1666665f * energyDirection;
                //upper temperature limit similar to that of core heaters
                if (vecTemperature > 120)
                {
                    energyLimit = 0;
                }

                var tempChange = GenTemperature.ControlTemperatureTempChange(coolerVec, Map, energyLimit,
                    compTempControl.targetTemperature);

                highPowerMode = !Mathf.Approximately(tempChange, 0f);
                if (highPowerMode)
                {
                    coolerVec.GetRoom(Map).Temperature += tempChange;
                }
            }

            if (highPowerMode)
            {
                compPowerTrader.PowerOutput = -compPowerTrader.Props.PowerConsumption * (2 - AcPipeNet.Efficiency);
            }
            else
            {
                compPowerTrader.PowerOutput = -compPowerTrader.Props.PowerConsumption *
                                              compTempControl.Props.lowPowerConsumptionFactor *
                                              (2 - AcPipeNet.Efficiency);
            }

            compTempControl.operatingAtHighPower = highPowerMode;
        }
    }
}