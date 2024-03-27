using HarmonyLib;
using Verse;

namespace VanillaTemperatureExpanded
{
    public class VanillaTemperatureExpanded : Mod
    {
        public VanillaTemperatureExpanded(ModContentPack content) : base(content)
        {
            new Harmony("VanillaTemperatureExpandedMod").PatchAll();
        }
    }


}