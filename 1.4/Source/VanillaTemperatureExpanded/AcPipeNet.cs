using System;
using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using Verse;

namespace VanillaTemperatureExpanded;

public class AcPipeNet : PipeNet
{
    // Singletons
    public Dictionary<ThingDef, List<CompResourceSingleton>> singletonDict = new();

    public float Efficiency = 1f;

    private static readonly float MinEff = 0f;
    private static readonly float MaxEff = 1.5f;
    private static readonly float BaseEff = 1f;

    public void AddToSingletonDict(CompResourceSingleton comp)
    {
        if (!singletonDict.ContainsKey(comp.parent.def))
        {
            singletonDict[comp.parent.def] = new List<CompResourceSingleton>();
        }

        if (!singletonDict[comp.parent.def].Contains(comp))
        {
            singletonDict[comp.parent.def].Add(comp);
        }
    }

    public void RemoveFromSingletonDict(CompResourceSingleton comp)
    {
        singletonDict[comp.parent.def].Remove(comp);
    }

    public override void RegisterComp(CompResource comp)
    {
        base.RegisterComp(comp);
        if (comp is CompResourceSingleton singleton)
        {
            AddToSingletonDict(singleton);
            UpdateSingletonOverlays(singleton);
        }

        receiversDirty = true;
        producersDirty = true;
    }

    public override void UnregisterComp(CompResource comp)
    {
        base.UnregisterComp(comp);
        if (comp is CompResourceSingleton singleton)
        {
            RemoveFromSingletonDict(singleton);
            UpdateSingletonOverlays(singleton);
        }

        receiversDirty = true;
        producersDirty = true;
    }

    public override void PipeSystemTick()
    {
        var effChanged = false;
        if (receiversDirty || nextTickRDirty)
        {
            nextTickRDirty = ReceiversDirty();
            effChanged = true;
        }

        if (producersDirty)
        {
            ProducersDirty();
            effChanged = true;
        }

        foreach (var compResourceTrader in producersOff.Where(compResourceTrader => compResourceTrader.CanBeOn()))
        {
            compResourceTrader.ResourceOn = true;
            effChanged = true;
        }

        foreach (var compResourceTrader in receiversOff.Where(compResourceTrader => compResourceTrader.CanBeOn()))
        {
            compResourceTrader.ResourceOn = true;
            effChanged = true;
        }

        if (effChanged)
        {
            CalculateEfficiency();
            //NB: Currently inspection string does not change shown power usage. TODO: add current power usage to inspection strings
            if (Efficiency == 0)
            {
                foreach (var rec in receivers)
                {
                    rec.ResourceOn = false;
                    rec.powerComp.PowerOutput = -rec.powerComp.Props.PowerConsumption;
                }
            }
            else
            {
                foreach (var rec in receivers)
                {
                    rec.powerComp.PowerOutput = -rec.powerComp.Props.PowerConsumption * (2 - Efficiency);
                }
            }

            foreach (var singleton in singletonDict.SelectMany(kv => kv.Value))
            {
                UpdateSingletonOverlays(singleton);
            }
        }
    }

    private void CalculateEfficiency()
    {
        var efficiencyFactor = Production - Consumption;
        if (Production == 0)
        {
            Efficiency = 0;
        }
        else
        {
            Efficiency = efficiencyFactor < 0
                ? Math.Max(MinEff, BaseEff - 0.05f * Math.Abs(efficiencyFactor))
                : Math.Min(MaxEff, BaseEff + 0.01f * Math.Abs(efficiencyFactor));
        }
    }


    private void UpdateSingletonOverlays(CompResourceSingleton singleton)
    {
        foreach (var s in singletonDict[singleton.parent.def])
        {
            s.UpdateOverlayHandle();
        }
    }
}