using HarmonyLib;
using RimWorld;
using System.Linq;
using VanillaTemperatureExpanded.Buildings;
using Verse;

namespace VanillaTemperatureExpanded;

[HarmonyPatch(typeof(RoofGrid), "SetRoof")]
public static class RoofGrid_SetRoof_Patch
{
    public static void Postfix(RoofGrid __instance, IntVec3 c, RoofDef def, Map ___map)
    {
        if (def is null)
        {
            var fans = c.GetThingList(___map).OfType<Building_CeilingFan>().ToList();
            foreach (var f in fans)
            {
                f.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 1000));
                if (f.Destroyed is false)
                {
                    f.Destroy(DestroyMode.KillFinalize);
                }
            }
        }
    }
}
