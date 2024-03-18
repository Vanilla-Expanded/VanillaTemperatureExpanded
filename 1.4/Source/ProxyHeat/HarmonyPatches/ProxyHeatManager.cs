using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ProxyHeat
{
	public class ProxyHeatManager : MapComponent
	{
        public bool dirty = false;

        public Dictionary<IntVec3, List<CompTemperatureSource>> temperatureSources = new Dictionary<IntVec3, List<CompTemperatureSource>>();
        public List<CompTemperatureSource> compTemperatures = new List<CompTemperatureSource>();
        public List<CompTemperatureSource> compTemperaturesToTick = new List<CompTemperatureSource>();
        private List<CompTemperatureSource> dirtyComps = new List<CompTemperatureSource>();
        public ProxyHeatManager(Map map) : base(map)
		{
            HarmonyPatches.proxyHeatManagers[map] = this;
		}
        public void MarkDirty(CompTemperatureSource comp)
        {
            dirtyComps.Add(comp);
            dirty = true;
        }
        public void RemoveComp(CompTemperatureSource comp)
        {
            if (compTemperatures.Contains(comp))
            {
                compTemperatures.Remove(comp);
            }
            foreach (var data in temperatureSources.Values)
            {
                if (data.Contains(comp))
                {
                    data.Remove(comp);
                }
            }
        }
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (this.dirty)
            {
                foreach (var comp in dirtyComps)
                {
                    if (comp != null)
                    {
                        if (comp.parent.Spawned)
                        {
                            comp.RecalculateAffectedCells();
                            if (!comp.AffectedCells.Any())
                            {
                                RemoveComp(comp);
                            }
                        }
                        else
                        {
                            RemoveComp(comp);
                        }
                    }
                }
                dirtyComps.Clear();
                this.dirty = false;
            }

            foreach (var comp in compTemperaturesToTick)
            {
                comp.TempTick();
            }
        }

        public float GetTemperatureOutcomeFor(IntVec3 cell, float result)
        {
            if (temperatureSources.TryGetValue(cell, out List<CompTemperatureSource> tempSources))
            {
                if (!cell.UsesOutdoorTemperature(map) && !ProxyHeatMod.settings.enableProxyHeatEffectIndoors)
                {
                    foreach (var comp in tempSources)
                    {
                        this.MarkDirty(comp);
                    }
                    return result;
                }
                var tempResult = result;
                foreach (var tempSourceCandidate in tempSources)
                {
                    var props = tempSourceCandidate.Props;
                    var tempOutcome = tempSourceCandidate.TemperatureOutcome;
                    if (tempOutcome != 0)
                    {
                        if (props.maxTemperature.HasValue && result >= props.maxTemperature.Value && tempOutcome > 0)
                        {
                            continue;
                        }
                        else if (props.minTemperature.HasValue && props.minTemperature.Value >= result && tempOutcome < 0)
                        {
                            continue;
                        }
                        tempResult += tempOutcome;
                        if (props.maxTemperature.HasValue && result < props.maxTemperature.Value && tempResult > props.maxTemperature.Value && tempResult > result)
                        {
                            tempResult = props.maxTemperature.Value;
                        }
                        else if (props.minTemperature.HasValue && props.minTemperature.Value > tempResult && result > tempResult)
                        {
                            tempResult = props.minTemperature.Value;
                        }
                    }
                }
                result = tempResult;
            }
            return result;
        }
    }
}
