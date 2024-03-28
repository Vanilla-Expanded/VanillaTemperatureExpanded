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
        var intVec = center + IntVec3.North.RotatedBy(rot);
        GenDraw.DrawFieldEdges(new List<IntVec3> { intVec }, GenTemperature.ColorSpotCold);
        var room = intVec.GetRoom(currentMap);
        if (room is { UsesOutdoorTemperature: false })
        {
            GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomCold);
        }
    }
    
    public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        
        IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
        
        if (intVec2.Impassable(map))
        {
            return "MustPlaceCoolerWithFreeSpaces".Translate();
        }
        Frame firstThing2 = intVec2.GetFirstThing<Frame>(map);
        if ( firstThing2 != null && firstThing2.def.entityDefToBuild != null && firstThing2.def.entityDefToBuild.passability == Traversability.Impassable)
        {
            return "MustPlaceCoolerWithFreeSpaces".Translate();
        }
        Blueprint firstThing4 = intVec2.GetFirstThing<Blueprint>(map);
        if (firstThing4 != null && firstThing4.def.entityDefToBuild != null && firstThing4.def.entityDefToBuild.passability == Traversability.Impassable)
        {
            return "MustPlaceCoolerWithFreeSpaces".Translate();
        }
        return true;
    }
}