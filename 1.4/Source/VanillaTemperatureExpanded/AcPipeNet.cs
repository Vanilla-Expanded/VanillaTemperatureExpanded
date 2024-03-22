using System;
using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using VanillaTemperatureExpanded.Buildings;
using VanillaTemperatureExpanded.Comps;
using Verse;

namespace VanillaTemperatureExpanded;

public class AcPipeNet : PipeNet
{
    // Singletons
    public List<Building_AcControlUnit> ControllerList = new();

    public float Efficiency = 1f;

    public static readonly float MinEff = 0f;
    public static readonly float MaxEff = 1.5f;
    public static readonly float BaseEff = 1f;

    public void AddToSingletonList(CompResourceSingleton comp)
    {
        var controller = comp.parent as Building_AcControlUnit;
        if (!ControllerList.Contains(controller))
            ControllerList.Add(controller);
    }

    public void RemoveFromSingletonList(CompResourceSingleton comp)
    {
        var controller = comp.parent as Building_AcControlUnit;
        if (ControllerList.Contains(controller))
            ControllerList.Remove(controller);
    }

    public override void RegisterComp(CompResource comp)
    {
        base.RegisterComp(comp);
        if (comp is CompResourceSingleton singleton)
        {
            AddToSingletonList(singleton);
            singleton.UpdateOverlayHandle();
        }

        receiversDirty = true;
        producersDirty = true;
    }

    public override void UnregisterComp(CompResource comp)
    {
        base.UnregisterComp(comp);
        if (comp is CompResourceSingleton singleton)
        {
            RemoveFromSingletonList(singleton);
            singleton.UpdateOverlayHandle();
        }

        receiversDirty = true;
        producersDirty = true;
    }

    public override void PipeSystemTick()
    {
        var effToChange = false;
        if (receiversDirty || nextTickRDirty)
        {
            nextTickRDirty = ReceiversDirty();
            effToChange = true;
        }

        if (producersDirty)
        {
            ProducersDirty();
            effToChange = true;
        }

        var wouldOverload = WouldOverload();
        if (ControllerList.Count(c => c.resourceComp.ResourceOn || c.resourceComp.CanBeOn()) == 1)
        {
            //if in normal flow OR in zero state but can start gracefully
            if (receiversOn.Count > 0 || (!wouldOverload && receiversOn.Count == 0))
            {
                foreach (var compResourceTrader in receiversOff.Where(
                             compResourceTrader => compResourceTrader.CanBeOn()))
                {
                    compResourceTrader.ResourceOn = true;
                    effToChange = true;
                    (compResourceTrader as CompResourceTrader_AC)?.UpdateOverlayHandle();
                }
            }

            foreach (var compResourceTrader in producersOff.Where(compResourceTrader => compResourceTrader.CanBeOn()))
            {
                compResourceTrader.ResourceOn = true;
                effToChange = true;
                (compResourceTrader as CompResourceTrader_AC)?.UpdateOverlayHandle();
            }
        }

        if (effToChange)
        {
            CalculateEfficiency();
            if (Efficiency == 0 ||
                ControllerList.Count(c => c.resourceComp.ResourceOn || c.resourceComp.CanBeOn()) != 1)
            {
                foreach (var rec in receivers)
                {
                    rec.ResourceOn = false;
                    rec.powerComp.PowerOutput = -rec.powerComp.Props.PowerConsumption;
                    (rec as CompResourceTrader_AC)?.UpdateOverlayHandle();
                }

                foreach (var p in producers)
                {
                    p.ResourceOn = false;
                    (p as CompResourceTrader_Compressor)?.UpdateOverlayHandle();
                }
            }

            if (!wouldOverload)
            {
                foreach (var singleton in ControllerList)
                {
                    if (ControllerList.Count(c => c.resourceComp.CanBeOn()) == 1 && singleton.resourceComp.CanBeOn())
                    {
                        singleton.resourceComp.ResourceOn = true;
                    }

                    singleton.GetComp<CompResourceSingleton>().UpdateOverlayHandle();
                }
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

    public bool WouldOverload()
    {
        var efficiencyFactor = Production + producersOff.Where(c => c.CanBeOn()).Sum(c => c.Consumption) -
                               (Consumption + receiversOff.Where(c => c.CanBeOn()).Sum(c => c.Consumption));

        var x = efficiencyFactor < 0
            ? Math.Max(MinEff, BaseEff - 0.05f * Math.Abs(efficiencyFactor))
            : Math.Min(MaxEff, BaseEff + 0.01f * Math.Abs(efficiencyFactor));

        return x < MinEff;
    }
}