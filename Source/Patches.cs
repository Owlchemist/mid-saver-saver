using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using static MidsaverSaver.ModSettings_MidSaverSaver;
using static MidsaverSaver.MidSaverSaverUtility;
 
namespace MidsaverSaver
{
    [HarmonyPatch(typeof(Log), nameof(Log.Notify_MessageReceivedThreadedInternal))]
    [HarmonyPriority(Priority.First)]
    public static class Patch_Notify_MessageReceivedThreadedInternal
    {
        static bool Prefix()
        {
            return !disableErrorSpamControl;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    [HarmonyPriority(Priority.First)]
    public static class LogFixesBeingUsed
    {
        static void Prefix()
        {
            List<string> report = new List<string>();
            if (disableCompression) report.Add(nameof(disableCompression));
            if (fixCorruptIdeos) report.Add(nameof(fixCorruptIdeos));
            if (fixCorruptWorldObjects) report.Add(nameof(fixCorruptWorldObjects));
            if (fixCorruptSectors) report.Add(nameof(fixCorruptSectors));
            if (fixCorruptWeather) report.Add(nameof(fixCorruptWeather));
            if (generateMissingMineables) report.Add(nameof(generateMissingMineables));
            if (report.Count > 0) Log.Message("[Mid-saver Saver] Loading game and attemting the following fixes (note: do not run fixes unless they are needed):\n - " + string.Join("\n - ", report));
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    [HarmonyPriority(Priority.First)]
    public static class RunFixesAfterGameLoads
    {
        static void Postfix()
        {
            try { if (disableCompression) DisableCompression();}
            catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(DisableCompression)); }
            
            try { if (fixCorruptIdeos) CheckIdeos(); fixCorruptIdeos = false;}
            catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(CheckIdeos)); }

            try { if (fixCorruptSectors) CheckAreas();}
            catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(CheckAreas)); }

            try { if (fixCorruptWeather) CheckWeather(); fixCorruptWeather = false;}
            catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(CheckWeather)); }

            try { if (fixMissingStuff) CheckNullStuff(); fixMissingStuff = false;}
            catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(CheckNullStuff)); }

            try { if (generateMissingMineables) CheckMissingMineables();}
            catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(CheckMissingMineables)); }
        }
    }

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
}