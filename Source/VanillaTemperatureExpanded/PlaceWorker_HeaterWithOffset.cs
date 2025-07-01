using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaTemperatureExpanded;

public class PlaceWorker_HeaterWithOffset : PlaceWorker
{
    private IntVec3 GetOutputCell(BuildableDef def)
    {
        return def.HasModExtension<TemperatureOutputPositionModExtension>() ? def.GetModExtension<TemperatureOutputPositionModExtension>().offsetNorth : IntVec3.North;
    }

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var currentMap = Find.CurrentMap;
        var pos = center + GetOutputCell(def).RotatedBy(rot);
        GenDraw.DrawFieldEdges([pos], GenTemperature.ColorSpotHot);
        var room = pos.GetRoom(currentMap);
        if (room is { UsesOutdoorTemperature: false })
        {
            GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomHot);
        }
    }

    public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        var intVec = center + GetOutputCell(def).RotatedBy(rot);
        var result = CheckCell(map, intVec);
        return result;
    }

    private static AcceptanceReport CheckCell(Map map, IntVec3 c)
    {
        if (c.Impassable(map))
        {
            return "VTE.MustPlaceHeaterWithFreeSpaces".Translate();
        }
        var firstThing = c.GetFirstThing<Frame>(map);
        if ((firstThing != null && firstThing.def.entityDefToBuild != null && firstThing.def.entityDefToBuild.passability == Traversability.Impassable))
        {
            return "VTE.MustPlaceHeaterWithFreeSpaces".Translate();
        }
        var firstThing3 = c.GetFirstThing<Blueprint>(map);
        if ((firstThing3 != null && firstThing3.def.entityDefToBuild != null && firstThing3.def.entityDefToBuild.passability == Traversability.Impassable))
        {
            return "VTE.MustPlaceHeaterWithFreeSpaces".Translate();
        }
        return true;
    }
}
