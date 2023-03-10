using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using System.Collections.Generic;
using static MidsaverSaver.ModSettings_MidSaverSaver;
using static MidsaverSaver.MidSaverSaverUtility;
 
namespace MidsaverSaver
{
    //Check for missing sections
    [HarmonyPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
    [HarmonyPriority(Priority.First)]
    public static class CheckWorldObjects
    {
        static void Postfix()
        {
            if (fixCorruptWorldObjects)
            {
                try { CheckWorldObjects(); fixCorruptWorldObjects = false;}
                catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(CheckWorldObjects)); }
            }
        }
    }

    //Check for missing sections
    [HarmonyPatch(typeof(MapDrawer), nameof(MapDrawer.SectionAt))]
    [HarmonyPriority(Priority.First)]
    public static class Patch_SectionAt
    {
        static void Prefix(MapDrawer __instance)
        {
            if (fixCorruptSectors) CheckSectors(__instance);
        }
    }

    [HarmonyPatch(typeof(GenStep_ScatterLumpsMineable), nameof(GenStep_ScatterLumpsMineable.ChooseThingDef))]
    public static class Patch_GenStep_ScatterLumpsMineable
    {
        public static bool overRideScatterActive;
        public static Dictionary<Map, List<ThingDef>> mineableDefsQueue;
        public static List<ThingDef> mineableDefs;
        static bool Prefix(ref ThingDef __result)
        {
            if (!overRideScatterActive) return true;

            __result = mineableDefs.RandomElementByWeightWithFallback(delegate(ThingDef d)
			{
				if (d.building == null) return 0f;
				if (d.building.mineableThing != null && d.building.mineableThing.BaseMarketValue > float.MaxValue) return 0f;
				return d.building.mineableScatterCommonality;
			}, null);
            return false;
        }
    }

    [HarmonyPatch(typeof(ThingFilter), nameof(ThingFilter.ExposeData))]
    static class Patch_ExposeThingFilter
    {
        public static int count = 0;
        static void Postfix(ThingFilter __instance)
        {
            if (!fixMisc) return;
            try
            {
                if (__instance.disallowedSpecialFilters != null)
                {
                    count += __instance.disallowedSpecialFilters.RemoveAll(x => x is null);
                }
                if (__instance.allowedDefs != null)
                {
                    count += __instance.allowedDefs.RemoveWhere(x => x is null);
                }
            }
            catch (System.Exception ex) { Log.Error("[Mid-saver Saver] failed to run " + nameof(Patch_ExposeThingFilter) + "\n" + ex); }
        }
    }

    [HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.ExposeData))]
    static class Patch_GameConditionManager_ExposeData
    {
        public static int count = 0;
        static void Postfix(GameConditionManager __instance)
        {
            if (!fixMisc) return;
            int count = 0;
            //Check broken game conditions
            var activeConditions = __instance.activeConditions;
            for (int i = activeConditions.Count; i-- > 0;)
            {
                var activeCondition = activeConditions[i];

                if (activeCondition == null || activeCondition.def == null) 
                {
                    count++;
                    activeConditions.Remove(activeCondition);
                }
            }
        }
    }
    
    /*
    [HarmonyPatch(typeof(GenLabel), nameof(GenLabel.BestKindLabel), new System.Type[] { typeof(Pawn), typeof(bool), typeof(bool), typeof(bool), typeof(int) } )]
    static class Patch_BestKindLabel
    {
        public static int count = 0;
        public static int destroyed = 0;
        static void Prefix(Pawn pawn)
        {
            if (!fixMisc) return;
            if (pawn.kindDef == null)
            {
                count++;
                if (pawn.RaceProps.Humanlike) pawn.kindDef = PawnKindDefOf.Villager;
                else
                {
                    var fallback = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(x => x.race == pawn.def);
                    if (fallback == null)
                    {
                        destroyed++;
                        pawn.Destroy();
                    }
                    else pawn.kindDef = fallback;
                }
            }
        }
    }
    
    
    //[HarmonyPatch(typeof(Thought_IdeoLeaderResentment), nameof(Thought_IdeoLeaderResentment.LabelCap), MethodType.Getter)]
    static class Patch_Thought_IdeoLeaderResentment
    {
        public static int count = 0;
        static bool Prefix(Thought_IdeoLeaderResentment __instance, string __result)
        {
            if (!fixCorruptIdeos) return true;
            
            if (__instance.Leader == null || __instance.Leader.Ideo == null) 
            {
                __result = "NULL";
                return false;
            }
            return true;
        }
    }
    */
}