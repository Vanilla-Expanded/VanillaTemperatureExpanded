using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ProxyHeat
{
	[StaticConstructorOnStartup]
	internal static class HarmonyPatches
	{
		public static Dictionary<Map, ProxyHeatManager> proxyHeatManagers = new Dictionary<Map, ProxyHeatManager>();
		public static List<string> temperatureBuildingMethodsForRegisterMethod = new List<string>
		{
            "RimWorld.Building_Heater:TickRare",
            "RimWorld.Building_Cooler:TickRare",
            "VanillaTemperatureExpanded.Buildings.Building_AcUnit:TickRare",
            "VanillaTemperatureExpanded.Buildings.Building_HeaterWithOffset:PushHeat",
            "VanillaTemperatureExpanded.Buildings.Building_TwoCellCooler:PushCoolAir",
        };

        public static List<string> temperatureBuildingMethodsForPostfixMethod = new List<string>
        {
            "RimWorld.Building_Heater:TickRare",
            "RimWorld.Building_Cooler:TickRare",
            "VanillaTemperatureExpanded.Buildings.Building_AcUnit:TickRare",
            "VanillaTemperatureExpanded.Buildings.Building_HeaterWithOffset:TickRare",
            "VanillaTemperatureExpanded.Buildings.Building_TwoCellHeater:TickRare",
            "VanillaTemperatureExpanded.Buildings.Building_TwoCellCooler:TickRare",
        };

        static HarmonyPatches()
		{
			Harmony harmony = new Harmony("LongerCFloor.ProxyHeat");
			harmony.PatchAll();
            var transpiler = AccessTools.Method(typeof(HarmonyPatches), nameof(TickRare_Transpiler));

            foreach (var methodName in temperatureBuildingMethodsForRegisterMethod)
			{
				var method = AccessTools.Method(methodName);
				if (method != null)
				{
                    harmony.Patch(method, transpiler: new HarmonyMethod(transpiler));
                }
			}
			var postfix = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.TickRare_Postfix));
            foreach (var methodName in temperatureBuildingMethodsForPostfixMethod)
            {
                var method = AccessTools.Method(methodName);
                if (method != null)
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(postfix));
                }
            }

            if (ModLister.HasActiveModWithName("Brrr and Phew (Continued)"))
			{
				var prefix = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.TryGiveJobPrefix));
				postfix = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.TryGiveJobPostfix));
				foreach (var type in GenTypes.AllSubclasses(typeof(ThinkNode_JobGiver)))
				{
					if (type.Namespace == "Brrr")
					{
						var method = AccessTools.Method(type, "TryGiveJob");
						harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
					}
				}

				var genNewRRJobMethod = AccessTools.Method("Brrr.BrrrGlobals:GenNewRRJob");
				harmony.Patch(genNewRRJobMethod, new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.GenNewRRJobPrefix)));

				var meth_JobGiver_Brrr_TryGiveJob = AccessTools.Method("Brrr.JobGiver_Brrr:TryGiveJob");
				harmony.Patch(meth_JobGiver_Brrr_TryGiveJob, null, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.JobGiver_BrrrTranspiler)));

				var meth_JobGiver_Phew_TryGiveJob = AccessTools.Method("Brrr.JobGiver_Phew:TryGiveJob");
				harmony.Patch(meth_JobGiver_Phew_TryGiveJob, null, null, new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.JobGiver_PhewTranspiler)));
			}
		}

		public static (Building source, float heatpush)? heatpushData;
		public static void TickRare_Postfix(Building_TempControl __instance)
		{
			if (heatpushData.HasValue && heatpushData.Value.source == __instance)
			{
                var compPowerTrader = __instance.compPowerTrader;
                CompProperties_Power props = compPowerTrader.Props;
                var flag = !Mathf.Approximately(heatpushData.Value.heatpush, 0f);
                if (flag)
                {
                    compPowerTrader.PowerOutput = 0f - props.PowerConsumption;
                }
                else
                {
                    compPowerTrader.PowerOutput = (0f - props.PowerConsumption) * __instance.compTempControl.Props.lowPowerConsumptionFactor;
                }
                __instance.compTempControl.operatingAtHighPower = flag;
            }
			heatpushData = null;
        }

		private static MethodInfo get_Position = AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Position));
        private static MethodInfo controlTemperatureTempChange = AccessTools.Method(typeof(GenTemperature), nameof(GenTemperature.ControlTemperatureTempChange));
        private static MethodInfo registerRoomTemperatureChange = AccessTools.Method(typeof(HarmonyPatches), nameof(RegisterRoomTemperatureChange));
 
        public static IEnumerable<CodeInstruction> TickRare_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
           var codes = instructions.ToList();
			for (var i = 0; i < codes.Count; i++)
			{
				var code = codes[i];
				yield return code;
				if (i > 1 && codes[i - 1].Calls(controlTemperatureTempChange))
				{
                    if (code.opcode == OpCodes.Stloc_S)
                    {
                        foreach (var codeInstruction in AddRegisterMethod(registerRoomTemperatureChange, codes, i,
                            Gen.YieldSingle(new CodeInstruction(OpCodes.Ldloc_S, (code.operand as LocalBuilder).LocalIndex))))
                        {
                            yield return codeInstruction;
                        }
                    }
                    else if (code.opcode == OpCodes.Stloc_3)
                    {
                        foreach (var codeInstruction in AddRegisterMethod(registerRoomTemperatureChange, codes, i,
                            Gen.YieldSingle(new CodeInstruction(OpCodes.Ldloc_3))))
                        {
                            yield return codeInstruction;
                        }
                    }
                }
            }
		}

        private static IEnumerable<CodeInstruction> AddRegisterMethod(MethodInfo registerRoomTemperatureChange, 
			List<CodeInstruction> codes, int i, IEnumerable<CodeInstruction> cellCodeInstructions)
        {
            if (codes[i - 8].Calls(get_Position))
			{
                yield return codes[i - 9];
                yield return codes[i - 8];
            }
            else if (codes[i - 8].IsLdloc() || codes[i - 8].opcode == OpCodes.Ldarg_1)
			{
				yield return codes[i - 8];
			}
            foreach (var code in cellCodeInstructions)
			{
				yield return code;
			}
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return codes[i - 5];
            yield return new CodeInstruction(OpCodes.Call, registerRoomTemperatureChange);
        }

        private static void RegisterRoomTemperatureChange(IntVec3 cell, float result, 
			Building_TempControl building, float energyLimit)
		{
			if (result == 0)
			{
                Room room = cell.GetRoom(building.Map);
                if (room != null && room.UsesOutdoorTemperature)
                {
                    var comp = building.GetComp<CompTemperatureSource>();
                    float b = energyLimit / (float)comp.AffectedCells.Count;
                    var cellTemperature = cell.GetTemperature(building.Map);
                    float a = building.compTempControl.targetTemperature - cellTemperature;
                    float heatPush;
                    float num;
                    if (energyLimit > 0f)
                    {
                        num = Mathf.Min(a, b);
                        heatPush = Mathf.Max(num, 0f);
                    }
                    else
                    {
                        num = Mathf.Max(a, b);
                        heatPush = Mathf.Min(num, 0f);
                    }
                    RegisterHeatPush(building, comp, cellTemperature, heatPush);
                }
            }
        }

        private static void RegisterHeatPush(Building_TempControl building, CompTemperatureSource comp, 
			float cellTemperature, float heatPush)
        {
			var roomTemperature = cellTemperature;
			GlobalControls_TemperatureString_Patch.ModifyTemperatureIfNeeded(ref roomTemperature, building.Position, building.Map);
            if (heatPush > 0 && roomTemperature > building.compTempControl.TargetTemperature)
			{
                heatPush = 0;
            }
            else if (heatPush < 0 && roomTemperature < building.compTempControl.TargetTemperature)
			{
                heatPush = 0;
            }

            var ticksPassed = Find.TickManager.TicksGame - comp.lastRoomTemperatureChangeTicks;
            if (ticksPassed > GenDate.TicksPerHour)
            {
                comp.lastRoomTemperatureChange = 0;
                comp.lastRoomTemperatureChangeTicks = Find.TickManager.TicksGame;
				heatpushData = null;
                return;
            }
            else if (ticksPassed > GenTicks.TickRareInterval)
            {
                comp.lastRoomTemperatureChange = heatPush;
            }
            else
            {
                comp.lastRoomTemperatureChange += heatPush;
            }
            if (comp.lastRoomTemperatureChangeTicks != Find.TickManager.TicksGame || heatpushData is null)
            {
                heatpushData = (building, heatPush);
            }
            else
            {
				heatpushData = (building, heatPush + heatpushData.Value.heatpush);
            }

            comp.lastRoomTemperatureChangeTicks = Find.TickManager.TicksGame;
            float diff = TempDiffFromOutdoorsAdjusted(building.Map, cellTemperature + comp.lastRoomTemperatureChange);
            var intervalChange = diff * 0.0007f * (float)ticksPassed;
            comp.lastRoomTemperatureChange += intervalChange;
            //Log.Message("cellTemperature: " + cellTemperature + " - heatPush: " + heatPush
			//	+ " - cellCount: " + comp.AffectedCells.Count + " - intervalChange: " + intervalChange
            //	+ " - ticksPassed: " + ticksPassed + " - diff: " + diff 
			//	+ " - comp.lastRoomTemperatureChange: " + comp.lastRoomTemperatureChange + " - " + comp);
        }

        private static float TempDiffFromOutdoorsAdjusted(Map map, float cellTemperature)
        {
            float num = map.mapTemperature.OutdoorTemp - cellTemperature;
            if (Mathf.Abs(num) < 100f)
            {
                return num;
            }
            return Mathf.Sign(num) * 100f + 5f * (num - Mathf.Sign(num) * 100f);
        }


        public static IEnumerable<CodeInstruction> JobGiver_BrrrTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var jobDefInfo = AccessTools.Field(AccessTools.TypeByName("Brrr.JobGiver_Brrr+BrrrJobDef"), "Brrr_BrrrRecovery");
			bool found = false;
			var list = instructions.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				var instruction = list[i];
				yield return instruction;
				if (jobDefInfo != null && !found && i > 1 && list[i - 1].opcode == OpCodes.Brfalse_S && list[i].LoadsField(jobDefInfo) && i < list.Count - 1 && list[i + 1].opcode == OpCodes.Ldloc_S)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "BrrrJob"));
					i += 2;
					found = true;
				}
			}
			if (jobDefInfo is null)
			{
				Log.Error("Proxy Heat failed to transpile Brr and Phew");
			}
		}
		public static IEnumerable<CodeInstruction> JobGiver_PhewTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var jobDefInfo = AccessTools.Field(AccessTools.TypeByName("Brrr.JobGiver_Phew+BrrrJobDef"), "Brrr_PhewRecovery");
			bool found = false;
			var list = instructions.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				var instruction = list[i];
				yield return instruction;

				if (jobDefInfo != null && !found && i > 1 && list[i - 1].opcode == OpCodes.Brfalse_S && 
					list[i].LoadsField(jobDefInfo) && i < list.Count - 1 && list[i + 1].opcode == OpCodes.Ldloc_S)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "BrrrJob"));
					i += 4;
					found = true;
				}

			}
			if (jobDefInfo is null)
            {
				Log.Error("Proxy Heat failed to transpile Brr and Phew");
            }
		}
		public static Job BrrrJob(JobDef def, Pawn pawn)
		{
			var tempRange = pawn.SafeTemperatureRange();
			if (!tempRange.Includes(pawnToLookUp.AmbientTemperature))
			{
				var result = Patch_TryGiveJob.SeekSafeTemperature(def, pawn, tempRange);
				return result;
			}
			return null;
		}

		private static Pawn pawnToLookUp;
		public static void TryGiveJobPrefix(Pawn pawn)
		{
			pawnToLookUp = pawn;
		}
		public static void TryGiveJobPostfix(Pawn pawn)
		{
			pawnToLookUp = null;
		}

		public static bool GenNewRRJobPrefix(ref Job __result, JobDef def, Region reg)
		{
			if (pawnToLookUp != null)
			{
				var map = reg.Map;
				var tempRange = pawnToLookUp.ComfortableTemperatureRange();
				if (reg.Room.UsesOutdoorTemperature && proxyHeatManagers.TryGetValue(map, out ProxyHeatManager proxyHeatManager))
				{
					var candidates = new List<IntVec3>();
					foreach (var tempSource in proxyHeatManager.temperatureSources)
					{
						if (reg.Room.ContainsCell(tempSource.Key))
						{
							var result = proxyHeatManager.GetTemperatureOutcomeFor(tempSource.Key, GenTemperature.GetTemperatureForCell(tempSource.Key, map));
							if (tempRange.Includes(result))
							{
								candidates.Add(tempSource.Key);
							}
						}
					}
					candidates = candidates.OrderBy(x => pawnToLookUp.Position.DistanceTo(x)).ToList();

					foreach (var cell in candidates)
					{
						if (cell.GetFirstPawn(map) is null && pawnToLookUp.Map.pawnDestinationReservationManager.FirstReserverOf(cell, pawnToLookUp.Faction) is null
							&& pawnToLookUp.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly))
						{
							__result = JobMaker.MakeJob(def, cell);
							pawnToLookUp.Reserve(cell, __result);
							return false;
						}
					}

					foreach (var cell in candidates)
					{
						if (pawnToLookUp.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly))
						{
							__result = JobMaker.MakeJob(def, cell);
							pawnToLookUp.Reserve(cell, __result);
							return false;
						}
					}
				}
			}
			return true;
		}


		[HarmonyPatch(typeof(Building), nameof(Building.SpawnSetup))]
		public static class Patch_SpawnSetup
		{
			private static void Postfix(Building __instance)
			{
				if (proxyHeatManagers.TryGetValue(__instance.Map, out ProxyHeatManager proxyHeatManager))
				{
					foreach (var comp in proxyHeatManager.compTemperatures)
					{
						if (comp.InRangeAndActive(__instance.Position))
						{
							proxyHeatManager.MarkDirty(comp);
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Building), nameof(Building.DeSpawn))]
		public static class Patch_DeSpawn
		{
			private static void Prefix(Building __instance)
			{
				if (__instance.Map != null && proxyHeatManagers.TryGetValue(__instance.Map, out ProxyHeatManager proxyHeatManager))
				{
					foreach (var comp in proxyHeatManager.compTemperatures)
					{
						if (comp.InRangeAndActive(__instance.Position))
						{
							proxyHeatManager.MarkDirty(comp);
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(GlobalControls), "TemperatureString")]
		public static class GlobalControls_TemperatureString_Patch
		{
			[HarmonyPriority(int.MinValue)]
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
			{
				var codes = codeInstructions.ToList();
				for (var i = 0; i < codes.Count; i++)
				{
					var code = codes[i];
					yield return code;
					if (code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder lb && lb.LocalIndex == 4)
					{
						yield return new CodeInstruction(OpCodes.Ldloca_S, 4);
						yield return new CodeInstruction(OpCodes.Ldloc_1);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Find), nameof(Find.CurrentMap)));
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GlobalControls_TemperatureString_Patch), nameof(ModifyTemperatureIfNeeded)));
					}
				}
			}

			public static void ModifyTemperatureIfNeeded(ref float result, IntVec3 cell, Map map)
			{
				if (proxyHeatManagers.TryGetValue(map, out ProxyHeatManager proxyHeatManager))
				{
					result = proxyHeatManager.GetTemperatureOutcomeFor(cell, result);
				}
			}
		}

		[HarmonyPatch(typeof(Thing), nameof(Thing.AmbientTemperature), MethodType.Getter)]
		public static class Patch_AmbientTemperature
		{
			private static void Postfix(Thing __instance, ref float __result)
			{
				var map = __instance.Map;
				if (map != null && proxyHeatManagers.TryGetValue(map, out ProxyHeatManager proxyHeatManager))
				{
					__result = proxyHeatManager.GetTemperatureOutcomeFor(__instance.Position, __result);
				}
			}
		}

		[HarmonyPatch(typeof(Plant), nameof(Plant.GrowthRateFactor_Temperature), MethodType.Getter)]
		public static class Patch_GrowthRateFactor_Temperature
		{
			public static bool checkForPlantGrowth;
			private static void Prefix(Plant __instance)
			{
				if (ProxyHeatMod.settings.allowPlantGrowthInsideProxyHeatEffectRadius)
				{
					checkForPlantGrowth = true;
				}
			}
			private static void Postfix(Plant __instance, float __result)
			{
				checkForPlantGrowth = false;
			}
		}


		[HarmonyPatch(typeof(PlantUtility), nameof(PlantUtility.GrowthSeasonNow))]
		public static class Patch_GrowthSeasonNow
		{
			private static bool Prefix(ref bool __result, IntVec3 c, Map map, bool forSowing = false)
			{
				if (ProxyHeatMod.settings.allowPlantGrowthInsideProxyHeatEffectRadius)
                {
					if (proxyHeatManagers.TryGetValue(map, out ProxyHeatManager proxyHeatManager))
					{
						var tempResult = proxyHeatManager.GetTemperatureOutcomeFor(c, 0f);
						if (tempResult != 0)
                        {
							float temperature = c.GetTemperature(map) + tempResult;
							if (temperature > 0f)
							{
								__result = temperature < 58f;
							}
							else
                            {
								__result = false;
                            }
							return false;
						}
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(GenTemperature), "TryGetTemperatureForCell")]
		public static class Patch_TryGetTemperatureForCell
		{
			private static void Postfix(bool __result, IntVec3 c, Map map, ref float tempResult)
			{
				if (__result && Patch_GrowthRateFactor_Temperature.checkForPlantGrowth)
				{
					if (proxyHeatManagers.TryGetValue(map, out ProxyHeatManager proxyHeatManager))
					{
						var plant = c.GetPlant(map);
						if (plant != null)
                        {
							tempResult = proxyHeatManager.GetTemperatureOutcomeFor(c, tempResult);
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(JobGiver_SeekSafeTemperature), "TryGiveJob")]
		public static class Patch_TryGiveJob
		{
			public static bool Prefix(Pawn pawn, ref Job __result)
            {
				if (!pawn.health.hediffSet.HasTemperatureInjury(TemperatureInjuryStage.Serious))
				{
					return false;
				}
				FloatRange tempRange = pawn.ComfortableTemperatureRange();
				if (!tempRange.Includes(pawn.AmbientTemperature))
				{
					var job = SeekSafeTemperature(JobDefOf.GotoSafeTemperature, pawn, tempRange);
					if (job != null)
                    {
						__result = job;
						return false;
                    }
				}
				return true;
			}
		
			public static Job SeekSafeTemperature(JobDef def, Pawn pawn, FloatRange tempRange)
			{
				var map = pawn.Map;
				if (pawn.Position.UsesOutdoorTemperature(map) && proxyHeatManagers.TryGetValue(map, out ProxyHeatManager proxyHeatManager))
				{
					var candidates = new List<IntVec3>();
					foreach (var tempSource in proxyHeatManager.temperatureSources)
                    {
						var result = proxyHeatManager.GetTemperatureOutcomeFor(tempSource.Key, GenTemperature.GetTemperatureForCell(tempSource.Key, map));
						if (tempRange.Includes(result))
						{
							candidates.Add(tempSource.Key);
						}
					}
					candidates = candidates.OrderBy(x => pawn.Position.DistanceTo(x)).ToList();
					foreach (var cell in candidates)
                    {
						if (cell.GetFirstPawn(map) is null && pawn.Map.pawnDestinationReservationManager.FirstReserverOf(cell, pawn.Faction) is null 
							&& pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly))
						{
							var job = JobMaker.MakeJob(def, cell);
							pawn.Reserve(cell, job);
							return job;
						}
					}

					foreach (var cell in candidates)
					{
						if (pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Deadly))
						{
							var job = JobMaker.MakeJob(def, cell);
							pawn.Reserve(cell, job);
							return job;
						}
					}
				}
				return null;
			}
		}
	}
}
