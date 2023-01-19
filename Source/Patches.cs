using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using static MidsaverSaver.ModSettings_MidSaverSaver;
 
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
            var report = new List<string>();
            if (disableCompression) report.Add(nameof(disableCompression));
            if (fixCorruptIdeos) report.Add(nameof(fixCorruptIdeos));
            if (fixCorruptWorldObjects) report.Add(nameof(fixCorruptWorldObjects));
            if (fixCorruptSectors) report.Add(nameof(fixCorruptSectors));
            if (fixCorruptWeather) report.Add(nameof(fixCorruptWeather));
            Log.Message("[Mid-saver Saver] Loading game and attemting the following fixes (note: do not run fixes unless they are needed):\n" + string.Join("\n - ", report));
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    [HarmonyPriority(Priority.First)]
    public static class RunFixesAfterGameLoads
    {
        static void Postfix()
        {
            if (disableCompression) DisableCompression();
            if (fixCorruptIdeos) CheckIdeos();
            if (fixCorruptWorldObjects) CheckWorldObjects();
            if (fixCorruptSectors) CheckAreas();
            if (fixCorruptWeather) CheckWeather();
        }

        static void DisableCompression()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                thingDef.saveCompressible = false;
            }
        }
        
        //Check for world objects with corrupt faction instances
        static void CheckWorldObjects()
        {
            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            if (worldObjects == null)
            {
                Log.Message("[Mid-saver Saver] worldObjects list is null, skipping...");
                return;
            }
            int count = 0;
            foreach (var item in worldObjects.ToList())
            {
                if (item.factionInt != null && item.factionInt.def == null) 
                {
                    count++;
                    item.Destroy();
                    worldObjects.Remove(item);
                }
            }
            if (count > 0)
            {
                Log.Message("[Mid-saver Saver] removed " + count.ToString() + " corrupt World Objects.");
            }
            return;
        }

        //Check for precepts that reference null defs
        static void CheckIdeos()
        {
            int count = 0;
            //Check corrupt apparel precepts
            foreach (var ideo in Find.IdeoManager.ideos.ToList())
            {
                foreach (var precept in ideo.precepts.ToList())
                {
                    if (precept is Precept_Apparel precept_Apparel && precept_Apparel.apparelDef == null)
                    {
                        count++;
                        ideo.RemovePrecept(precept);
                    }
                }
            }
            //Second pass to handle roles
            foreach (var ideo in Find.IdeoManager.ideos.ToList())
            {
                foreach (var precept in ideo.precepts.ToList())
                {
                    if (precept is Precept_Role precept_Role)
                    {
                        try
                        {
                            var strings = precept_Role.AllApparelRequirementLabels(Gender.Male);
                        }
                        catch (System.Exception)
                        {
                            count++;
                            ideo.RemovePrecept(precept);
                        }
                    }
                }
            }
            if (count > 0)
            {
                Log.Message("[Mid-saver Saver] removed " + count.ToString() + " corrupt precepts across all factions' ideologies.");
            }
        }

        static void CheckAreas()
        {
            int count = 0;
            foreach (var map in Find.Maps)
            {
                if (map.areaManager == null)
                {
                    count++;
                    map.areaManager = new AreaManager(map);
                    map.areaManager.AddStartingAreas();
                }
            }
            if (count > 0)
            {
                Log.Message("[Mid-saver Saver] found " + count.ToString() + " maps with corrupt area managers. Regenerating... You will need to rebuild your areas manually (home area, roof areas, etc)");
            }
        }
    
        static void CheckWeather()
        {
            int count = 0;
            foreach (var map in Find.Maps)
            {
                if (map.weatherManager.curWeather == null)
                {
                    count++;
                    map.weatherManager.curWeather = WeatherDefOf.Clear;
                    map.weatherManager.lastWeather = WeatherDefOf.Clear;
                    map.weatherManager.curWeatherAge = 0;
                }
            }
            if (count > 0)
            {
                Log.Message("[Mid-saver Saver] detected " + count.ToString() + " maps with corrupt weather managers. Resetting...");
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
            if (!fixCorruptSectors) return;
            if (__instance.sections == null)
            {
                Log.Message("[Mid-saver Saver] detected missing map sections. Regenerating...");
                __instance.RegenerateEverythingNow();
            }
        }
    }
}