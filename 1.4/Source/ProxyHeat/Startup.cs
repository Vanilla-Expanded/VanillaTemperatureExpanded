using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
                            props.tempOutcome = compHeatPusher.heatPerSecond * 5f;
                            if (compHeatPusher.heatPushMaxTemperature != default && compHeatPusher.heatPushMaxTemperature != 99999f)
                            {
                                props.maxTemperature = compHeatPusher.heatPushMaxTemperature;
                            }
                        }
                        else
                        {
                            props.tempOutcome = compHeatPusher.heatPerSecond * 5f;
                            if (compHeatPusher.heatPushMinTemperature != default && compHeatPusher.heatPushMinTemperature != -99999f)
                            {
                                props.minTemperature = compHeatPusher.heatPushMinTemperature;
                            }
                        }
                        props.tempOutcome = Mathf.Clamp(props.tempOutcome.Value, -50f, 50f);
                        SetData(thingDef, props);
                        thingDef.comps.Add(props);
                    }
                    else if (thingDef.thingClass != null && typeof(Building_TempControl).IsAssignableFrom(thingDef.thingClass))
                    {
                        props = new CompProperties_TemperatureSource();
                        SetData(thingDef, props);
                        thingDef.comps.Add(props);
                    }
                }
            }
		}

        private static void SetData(ThingDef thingDef, CompProperties_TemperatureSource props)
        {
            props.dependsOnFuel = thingDef.GetCompProperties<CompProperties_Refuelable>() != null;
            props.dependsOnPower = thingDef.GetCompProperties<CompProperties_Power>()?.basePowerConsumption > 0;
            props.radius = ((thingDef.Size.x + thingDef.Size.z) / 2f) + 0.5f;
            props.smeltSnowRadius = props.radius;
        }
    }
}
