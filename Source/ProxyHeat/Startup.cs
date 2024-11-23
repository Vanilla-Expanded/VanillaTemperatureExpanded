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
            AutogenerateProxyHeatComps();
        }

        private static void AutogenerateProxyHeatComps()
        {
            if (ProxyHeatMod.settings.compTempData is null)
            {
                ProxyHeatMod.settings.compTempData = new Dictionary<string, CompTempData>();
            }
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                var props = thingDef.GetCompProperties<CompProperties_TemperatureSource>();

                if (props is null)
                {
                    var compHeatPusher = thingDef.GetCompProperties<CompProperties_HeatPusher>();
                    if (compHeatPusher != null)
                    {
                        if (thingDef.category != ThingCategory.Ethereal 
                            && thingDef.category != ThingCategory.PsychicEmitter
                            && typeof(Hive).IsAssignableFrom(thingDef.thingClass) is false)
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
                                props.smeltSnowPower = 0.001f;
                            }
                            props.tempOutcome = Mathf.Clamp(props.tempOutcome.Value, -50f, 50f);
                            SetData(thingDef, props);
                            thingDef.comps.Add(props);
                        }
                    }
                    else if (thingDef.thingClass != null
                        && typeof(Building_TempControl).IsAssignableFrom(thingDef.thingClass)
                        && typeof(Building_Vent).IsAssignableFrom(thingDef.thingClass) is false)
                    {
                        props = new CompProperties_TemperatureSource();
                        SetData(thingDef, props);
                        thingDef.comps.Add(props);
                    }
                }

                if (props != null)
                {
                    if (ProxyHeatMod.settings.compTempDataReset.ContainsKey(thingDef.defName) is false)
                    {
                        ProxyHeatMod.settings.compTempDataReset[thingDef.defName] = GetTempData(props);
                    }
                    if (ProxyHeatMod.settings.compTempData.TryGetValue(thingDef.defName, out var tempData))
                    {
                        tempData.ApplyData(props);
                    }
                    else
                    {
                        ProxyHeatMod.settings.compTempData[thingDef.defName] = GetTempData(props);
                    }
                }
            }
        }

        private static CompTempData GetTempData(CompProperties_TemperatureSource props)
        {
            return new CompTempData
            {
                enabled = true,
                radius = props.radius,
                maxTemperature = props.maxTemperature,
                minTemperature = props.minTemperature,
                tempOutcome = props.tempOutcome,
            };
        }

        private static void SetData(ThingDef thingDef, CompProperties_TemperatureSource props)
        {
            var extension = thingDef.GetModExtension<ProxyHeatExtension>();
            if (extension != null)
            {
                props.radius = extension.radius;
                props.smeltSnowRadius = props.radius;
            }
            else
            {
                props.radius = ((thingDef.Size.x + thingDef.Size.z) / 2f) + 0.5f;
                props.smeltSnowRadius = props.radius;
            }
        }
    }

    public class ProxyHeatExtension : DefModExtension
    {
        public float radius;
    }
}
