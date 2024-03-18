using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ProxyHeat
{
    [StaticConstructorOnStartup]
	public static class Startup
	{
		static Startup()
		{
			foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				var props = thingDef.GetCompProperties<CompProperties_TemperatureSource>();
				if (props is null)
				{
                    var compHeatPusher = thingDef.GetCompProperties<CompProperties_HeatPusher>();
                    if (compHeatPusher != null)
                    {
                        props = new CompProperties_TemperatureSource();
                        if (compHeatPusher.heatPerSecond > 0)
                        {
                            if (compHeatPusher.heatPushMaxTemperature != default && compHeatPusher.heatPushMaxTemperature != 99999f)
                            {
                                props.tempOutcome = compHeatPusher.heatPushMaxTemperature;
                                props.maxTemperature = compHeatPusher.heatPushMaxTemperature;
                            }
                            else
                            {
                                props.tempOutcome = compHeatPusher.heatPerSecond * 5f;
                            }
                        }
                        else
                        {
                            if (compHeatPusher.heatPushMinTemperature != default && compHeatPusher.heatPushMinTemperature != -99999f)
                            {
                                props.tempOutcome = compHeatPusher.heatPushMinTemperature;
                                props.minTemperature = compHeatPusher.heatPushMinTemperature;
                            }
                            else
                            {
                                props.tempOutcome = compHeatPusher.heatPerSecond * 5f;
                            }
                        }
                        props.dependsOnFuel = thingDef.GetCompProperties<CompProperties_Refuelable>() != null;
                        props.dependsOnPower = thingDef.GetCompProperties<CompProperties_Power>()?.basePowerConsumption > 0;
                        props.radius = ((thingDef.Size.x + thingDef.Size.z) / 2f) + 0.5f;
                        props.smeltSnowRadius = props.radius;
                        thingDef.comps.Add(props);
                    }
                    else
                    {
                        var compTarget = thingDef.GetCompProperties<CompProperties_TempControl>();
                        if (compTarget != null) 
                        {
                            Log.Message("[VTE] Found no data for " + thingDef + " to autogenerate proxy heat");
                        }
                    }
                }
            }
		}
    }
}
