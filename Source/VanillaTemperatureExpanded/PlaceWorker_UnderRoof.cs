using RimWorld;
using System.Linq;
using Verse;

namespace VanillaTemperatureExpanded;

public class PlaceWorker_UnderRoof : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        var occupiedRect = GenAdj.OccupiedRect(loc, rot, checkingDef.Size);
        if (occupiedRect.Any(x => x.Roofed(map) is false))
        {
            return new AcceptanceReport("VTE.MustPlaceUnderRoof".Translate());
        }
        if (occupiedRect.Any(x => x.Filled(map)))
        {
            return false;
        }
        return true;
    }
}
