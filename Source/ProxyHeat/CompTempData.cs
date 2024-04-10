using Verse;

namespace ProxyHeat
{
    public class CompTempData : IExposable
    {
        public bool enabled;
        public float radius;
        public float? tempOutcome;
        public float? minTemperature;
        public float? maxTemperature;

        public void ApplyData(CompProperties_TemperatureSource compProps)
        {
            compProps.disabled = enabled is false;
            compProps.minTemperature = minTemperature;
            compProps.maxTemperature = maxTemperature;
            compProps.radius = radius;
            compProps.tempOutcome = tempOutcome;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref tempOutcome, "tempOutcome");
            Scribe_Values.Look(ref minTemperature, "minTemperature");
            Scribe_Values.Look(ref maxTemperature, "maxTemperature");
        }
    }
}
