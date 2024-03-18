﻿using System;
using System.Collections.Generic;
using System.Linq;
using PipeSystem;
using VanillaTemperatureExpanded.Buildings;
using VanillaTemperatureExpanded.Comps;

namespace VanillaTemperatureExpanded;

public class AcPipeNet : PipeNet
{
    // Singletons
    public List<Building_AcControlUnit> ControllerList = new();

    public float Efficiency = 1f;

    private static readonly float MinEff = 0f;
    private static readonly float MaxEff = 1.5f;
    private static readonly float BaseEff = 1f;

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

        foreach (var compResourceTrader in producersOff.Where(compResourceTrader => compResourceTrader.CanBeOn()))
        {
            compResourceTrader.ResourceOn = true;
            effToChange = true;
        }

        foreach (var compResourceTrader in receiversOff.Where(compResourceTrader => compResourceTrader.CanBeOn()))
        {
            compResourceTrader.ResourceOn = true;
            effToChange = true;
        }

        if (effToChange)
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

            foreach (var singleton in ControllerList)
            {
                singleton.GetComp<CompResourceSingleton>().UpdateOverlayHandle();
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
}