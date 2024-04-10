using Verse;

namespace ProxyHeat
{
    public static class ModCompatibility
    {
        public static bool VFETribalsActive = ModsConfig.IsActive("OskarPotocki.VFE.Tribals");

        public static bool ShouldWork(Thing thing)
        {
            if (VFETribalsActive && IsLargeFire(thing, out var lightOn))
            {
                if (lightOn is false)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsLargeFire(Thing thing, out bool lightOn)
        {
            lightOn = false;
            if (thing is VFETribals.LargeFire largeFire)
            {
                lightOn = largeFire.lightOn;
                return true;
            }
            return false;
        }
    }
}
