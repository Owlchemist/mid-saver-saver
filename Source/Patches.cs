using HarmonyLib;
using Verse;
using System.Collections.Generic;
using static MidsaverSaver.ModSettings_MidSaverSaver;
using static MidsaverSaver.MidSaverSaverUtility;
 
namespace MidsaverSaver
{
    //Handles the log spam control override
    [HarmonyPatch(typeof(Log), nameof(Log.Notify_MessageReceivedThreadedInternal))]
    [HarmonyPriority(Priority.First)]
    public static class Patch_Notify_MessageReceivedThreadedInternal
    {
        static bool Prefix()
        {
            return !disableErrorSpamControl;
        }
    }

    //Just used for logging to help troubleshooters
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
            if (fixMisc) report.Add(nameof(fixMisc));
            if (report.Count > 0) Log.Message("[Mid-saver Saver] Loading game and attempting the following fixes (note: do not run fixes unless they are needed):\n - " + string.Join("\n - ", report));
        }
    }

    //Initializes most of the fixes
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

            try { if (fixMisc) CheckMisc(); fixMisc = false;}
            catch (System.Exception) { Log.Error("[Mid-saver Saver] failed to run " + nameof(CheckMisc)); }
        }
    }
}