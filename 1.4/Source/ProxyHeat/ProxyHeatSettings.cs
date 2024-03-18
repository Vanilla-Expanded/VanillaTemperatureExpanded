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
    class ProxyHeatSettings : ModSettings
    {
        public bool enableProxyHeatEffectIndoors = false;
        public bool allowPlantGrowthInsideProxyHeatEffectRadius = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableProxyHeatEffectIndoors, "enableProxyHeatEffectIndoors", false);
            Scribe_Values.Look(ref allowPlantGrowthInsideProxyHeatEffectRadius, "allowPlantGrowthInsideProxyHeatEffectRadius", false);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var scrollHeight = GetScrollHeight();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, rect.width - 60f, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            listingStandard.CheckboxLabeled("PH.EnableProxyHeatEffectIndoors".Translate(), ref enableProxyHeatEffectIndoors);
            listingStandard.CheckboxLabeled("PH.AllowPlantGrowthInsideProxyHeatEffectRadius".Translate(), ref allowPlantGrowthInsideProxyHeatEffectRadius);
            listingStandard.End();
            Widgets.EndScrollView();
            SettingsApplier.ApplySettings();
            base.Write();
        }

        private float GetScrollHeight()
        {
            float num = 0;

            return num + 1000;
        }

        private static Vector2 scrollPosition = Vector2.zero;
    }
}
