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
            SettingsApplier.ApplySettings();
        }
        public override string SettingsCategory()
        {
            return "Proxy Heat";
        }
    }

    [StaticConstructorOnStartup]
    static class SettingsApplier
    {
        static SettingsApplier()
        {
            ApplySettings();
        }

        public static void ApplySettings()
        {

        }
    }
}
