using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ProxyHeat
{
    class ProxyHeatMod : Mod
    {
        public static ProxyHeatSettings settings;
        public ProxyHeatMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<ProxyHeatSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            ApplySettings();
        }

        public static void ApplySettings()
        {
            foreach (var data in ProxyHeatMod.settings.compTempData)
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(data.Key);
                if (def != null)
                {
                    var compProps = def.GetCompProperties<CompProperties_TemperatureSource>();
                    data.Value.ApplyData(compProps);
                }
            }
        }

        public override string SettingsCategory()
        {
            return "Proxy Heat";
        }
    }
}
