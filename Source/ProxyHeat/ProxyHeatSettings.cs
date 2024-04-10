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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    class ProxyHeatSettings : ModSettings
    {
        public bool enableProxyHeatEffectIndoors = false;
        public bool allowPlantGrowthInsideProxyHeatEffectRadius = false;
        public Dictionary<string, CompTempData> compTempData = new Dictionary<string, CompTempData>();
        public Dictionary<string, CompTempData> compTempDataReset = new Dictionary<string, CompTempData>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableProxyHeatEffectIndoors, "enableProxyHeatEffectIndoors", false);
            Scribe_Values.Look(ref allowPlantGrowthInsideProxyHeatEffectRadius, "allowPlantGrowthInsideProxyHeatEffectRadius", false);
            Scribe_Collections.Look(ref compTempData, "compTempData", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref compTempDataReset, "compTempDataReset", LookMode.Value, LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (compTempData is null)
                {
                    compTempData = new Dictionary<string, CompTempData>();
                }
                if (compTempDataReset is null)
                {
                    compTempDataReset = new Dictionary<string, CompTempData>();
                }
            }
        }

        private Vector2 scrollPos;
        private float sectionScrollHeight = 99999999;
        private string searchKey;

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            var initY = ls.curY;
            ls.CheckboxLabeled("PH.EnableProxyHeatEffectIndoors".Translate(), ref enableProxyHeatEffectIndoors);
            ls.CheckboxLabeled("PH.AllowPlantGrowthInsideProxyHeatEffectRadius".Translate(), ref allowPlantGrowthInsideProxyHeatEffectRadius);
            ls.GapLine();
            var labelRect = ls.Label("PH.ProxyHeatBuildings".Translate());
            var thingDefs = new List<(ThingDef def, CompTempData data)>();
            foreach (var data in compTempData)
            {
                var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(data.Key);
                if (thingDef != null)
                {
                    thingDefs.Add((thingDef, data.Value));
                }
            }
            thingDefs = thingDefs.OrderBy(x => x.Item1.index).ToList();
            var resetRect = new Rect(inRect.xMax - 120, labelRect.y, 120, 24);
            if (Widgets.ButtonText(resetRect, "Reset".Translate()))
            {
                DoReset();
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            var searchLabel = new Rect(resetRect.x - 275, resetRect.y, 60, 24);
            Widgets.Label(searchLabel, "PH.Search".Translate());
            var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
            searchKey = Widgets.TextField(searchRect, searchKey);
            Text.Anchor = TextAnchor.UpperLeft;
            if (searchKey.NullOrEmpty() is false)
            {
                thingDefs = thingDefs.Where(x => x.def.label.ToLower().Contains(searchKey.ToLower())).ToList();
            }

            var outRect = new Rect(inRect.x, ls.curY + 5, inRect.width, 495);
            var viewRect = new Rect(outRect.x, outRect.y, outRect.width - 16, sectionScrollHeight);
            Widgets.DrawMenuSection(outRect);
            Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
            sectionScrollHeight = 0;
            var pos = new Vector2(viewRect.x, viewRect.y);
            for (var i = 0; i < thingDefs.Count; i++)
            {
                var entryY = pos.y;
                var def = thingDefs[i].def;
                var data = thingDefs[i].data;
                var enabled = data.enabled;
                var iconRect = new Rect(pos.x, pos.y, 124, 124).ExpandedBy(-30);
                iconRect.x -= 15f;
                Widgets.ThingIcon(iconRect, def);
                var size = Text.CalcSize(def.LabelCap);
                labelRect = new Rect(iconRect.xMax + 15, pos.y, size.x, 24);
                Widgets.Label(labelRect, def.LabelCap);
                float endX = viewRect.xMax;
                AddCheckbox(new Vector2(endX, labelRect.y), ref enabled, true);
                pos.y += 24;
                pos.x = labelRect.x;
                data.enabled = enabled;
                AddSlider(ref pos, endX, "PH.ProxyHeatRadius".Translate() 
                    + ": " + data.radius.ToStringDecimalIfSmall(), ref data.radius, 1.5f, 10f, 0.5f);
                AddSlider(ref pos, endX, "PH.MinTemperature".Translate() + (data.minTemperature != null
                    ? ": " + data.minTemperature.Value.ToStringTemperatureOffset() : "PN.NotSet".Translate().ToString()), 
                    ref data.minTemperature, -100, 200f, 1f);
                AddSlider(ref pos, endX, "PH.MaxTemperature".Translate() + (data.maxTemperature != null
                    ? ": " + data.maxTemperature.Value.ToStringTemperatureOffset() : "PN.NotSet".Translate().ToString()), 
                    ref data.maxTemperature, -100, 200f, 1f);
                if (data.tempOutcome != null)
                {
                    var tempOutcome = data.tempOutcome.Value;
                    AddSlider(ref pos, endX, "PH.TemperatureOutput".Translate() 
                        + ": " + data.tempOutcome.Value.ToStringTemperatureOffset(), ref tempOutcome, -273, 500f, 1f);
                    data.tempOutcome = tempOutcome;
                }
                else
                {
                    var text = "PH.BuildingHasTemperatureControl".Translate();
                    var textSize = Text.CalcSize(text).x;
                    var tempControlLabelRect = new Rect(pos.x, pos.y, textSize, 24);
                    Widgets.Label(tempControlLabelRect, text);
                    pos.y += 24;
                }
                if (i == thingDefs.Count - 1)
                {
                    sectionScrollHeight += pos.y - entryY;
                    break;
                }
                pos.x = viewRect.x;
                float y = pos.y + 12 / 2f;
                Color color = GUI.color;
                GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
                Widgets.DrawLineHorizontal(pos.x, y, viewRect.width);
                GUI.color = color;
                pos.y += 12;
                sectionScrollHeight += pos.y - entryY;
            }
            Widgets.EndScrollView();
            ls.End();
        }

        private void DoReset()
        {
            compTempData = new Dictionary<string, CompTempData>();
            foreach (var data in compTempDataReset)
            {
                compTempData[data.Key] = new CompTempData
                {
                    enabled = data.Value.enabled,
                    radius = data.Value.radius,
                    minTemperature = data.Value.minTemperature,
                    maxTemperature = data.Value.maxTemperature,
                    tempOutcome = data.Value.tempOutcome,
                };
            }
        }

        private const float SliderWidth = 455;
        private static void AddSlider(ref Vector2 pos, float endX, string label, ref float field, 
            float minValue, float maxValue, float roundTo)
        {
            var textWidth = Text.CalcSize(label).x;
            var labelRect = new Rect(pos.x, pos.y, textWidth, 24);
            Widgets.Label(labelRect, label);
            var sliderRect = new Rect(endX - SliderWidth, labelRect.y + 5, SliderWidth, 24);
            field = RoundTo(Widgets.HorizontalSlider(sliderRect, field, minValue, maxValue), roundTo);
            pos.y += 24;
        }

        private static void AddSlider(ref Vector2 pos, float endX, string label, ref float? field,
    float minValue, float maxValue, float roundTo)
        {
            var textWidth = Text.CalcSize(label).x;
            var labelRect = new Rect(pos.x, pos.y, textWidth, 24);
            Widgets.Label(labelRect, label);
            if (field is null)
            {
                bool enabled = false;
                AddCheckbox(new Vector2(endX, labelRect.y), ref enabled, true);
                if (enabled)
                {
                    field = 0;
                }
            }
            else
            {
                bool enabled = true;
                var checkboxWidth = AddCheckbox(new Vector2(endX, labelRect.y), ref enabled, true);
                var sliderRect = new Rect(endX - (SliderWidth), labelRect.y + 5, (SliderWidth - 15) - checkboxWidth, 24);
                field = RoundTo(Widgets.HorizontalSlider(sliderRect, field.Value, minValue, maxValue), roundTo);
                if (enabled is false)
                {
                    field = null;
                }
            }
            pos.y += 24;
        }

        private static float RoundTo(float value, float roundTo)
        {
            return (float)(roundTo * Math.Round(value / roundTo));
        }

        private static float AddCheckbox(Vector2 pos, ref bool enabled, bool shiftedToEnd)
        {
            Text.Anchor = TextAnchor.MiddleRight;
            var enabledText = "Enabled".Translate();
            var enabledSize = Text.CalcSize(enabledText).x + 24 + 15;
            var checkbox = new Rect(shiftedToEnd ? pos.x - enabledSize : pos.x, pos.y, enabledSize, 24);
            Widgets.CheckboxLabeled(checkbox, enabledText, ref enabled, placeCheckboxNearText: true);
            Text.Anchor = TextAnchor.UpperLeft;
            return enabledSize;
        }
    }
}
