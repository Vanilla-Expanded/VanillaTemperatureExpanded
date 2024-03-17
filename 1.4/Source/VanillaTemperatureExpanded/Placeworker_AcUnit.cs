using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded;

public class Placeworker_AcUnit : PlaceWorker_Cooler
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var currentMap = Find.CurrentMap;
        var intVec = center + IntVec3.South.RotatedBy(rot);
        GenDraw.DrawFieldEdges(new List<IntVec3> { intVec }, GenTemperature.ColorSpotCold);
        var room = intVec.GetRoom(currentMap);
        if (room is { UsesOutdoorTemperature: false })
        {
            GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomCold);
        }
    }
}