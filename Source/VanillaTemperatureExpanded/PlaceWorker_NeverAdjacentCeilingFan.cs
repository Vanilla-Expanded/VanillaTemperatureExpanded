using RimWorld;
using System.Collections.Generic;
using VanillaTemperatureExpanded.Comps;
using Verse;

namespace VanillaTemperatureExpanded;

public class PlaceWorker_NeverAdjacentCeilingFan : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        foreach (IntVec3 item in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1))
        {
            List<Thing> list = map.thingGrid.ThingsListAt(item);
            for (int i = 0; i < list.Count; i++)
            {
                if (GenConstruct.BuiltDefOf(list[i].def) is ThingDef builtDef && builtDef.HasComp<CompCeilingFan>())
                {
                    return "VTE.CannotPlaceAdjacentCeilingFan".Translate();
                }
            }
        }
        return true;
    }
}
