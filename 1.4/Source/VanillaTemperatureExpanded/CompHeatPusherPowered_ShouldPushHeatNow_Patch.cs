using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaTemperatureExpanded.Buildings;

[HarmonyPatch(typeof(CompHeatPusherPowered), "ShouldPushHeatNow", MethodType.Getter)]
public static class CompHeatPusherPowered_ShouldPushHeatNow_Patch
{
    public static void Postfix(CompHeatPusherPowered __instance, bool __result)
    {
        if (__instance.parent.def == VTE_DefOf.VTE_StandingCooler)
        {
            var compPowerTrader = __instance.parent.GetComp<CompPowerTrader>();
            CompProperties_Power props = compPowerTrader.Props;
            if (__result)
            {
                compPowerTrader.PowerOutput = 0f - props.PowerConsumption;
            }
            else
            {
                compPowerTrader.PowerOutput = 0f - props.idlePowerDraw;
            }

        }
    }
}
