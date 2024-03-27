using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded;

public class PlaceWorker_HeaterWithOffset : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        Map currentMap = Find.CurrentMap;
        IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
        GenDraw.DrawFieldEdges(new List<IntVec3> { intVec2 }, GenTemperature.ColorSpotHot);
        Room room = intVec2.GetRoom(currentMap);
        if (room == null)
        {
            return;
        }
        if (!room.UsesOutdoorTemperature)
        {
            GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomHot);
        }
    }

    public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        IntVec3 intVec = center + IntVec3.North.RotatedBy(rot);
        var result = CheckCell(map, intVec);
        return result;
    }

    private static AcceptanceReport CheckCell(Map map, IntVec3 c)
    {
        if (c.Impassable(map))
        {
            return "VTE.MustPlaceHeaterWithFreeSpaces".Translate();
        }
        Frame firstThing = c.GetFirstThing<Frame>(map);
        if ((firstThing != null && firstThing.def.entityDefToBuild != null && firstThing.def.entityDefToBuild.passability == Traversability.Impassable))
        {
            return "VTE.MustPlaceHeaterWithFreeSpaces".Translate();
        }
        Blueprint firstThing3 = c.GetFirstThing<Blueprint>(map);
        if ((firstThing3 != null && firstThing3.def.entityDefToBuild != null && firstThing3.def.entityDefToBuild.passability == Traversability.Impassable))
        {
            return "VTE.MustPlaceHeaterWithFreeSpaces".Translate();
        }
        return true;
    }
}
