﻿using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded.Buildings;

public class Building_CeilingFan : Building
{
    public float angle;

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Graphic.Draw(DrawPos, Rotation, this, angle);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref angle, "angle");
    }
}
