using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using static MidSaverSaver.ModSettings_MidSaverSaver;
 
namespace MidSaverSaver
{ 
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    public static class Patch_LoadGame
    {
        static void Postfix()
        {
            if (!disableCompression) return;
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                thingDef.saveCompressible = false;
            }
        }
    }

    [HarmonyPatch(typeof(ExpandableWorldObjectsUtility), nameof(ExpandableWorldObjectsUtility.SortByExpandingIconPriority))]
    public static class Patch_SortByExpandingIconPriority
    {
        static bool Prefix(ref List<WorldObject> worldObjects)
        {
            if (!fixCorruptWorldObjects) return true;
            if (worldObjects == null)
            {
                Log.Message("[Mid-saver Saver] worldObjects list is null, skipping...");
                return false;
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
            return true;
        }
    }

    [HarmonyPatch(typeof(MapDrawer), nameof(MapDrawer.SectionAt))]
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

    [HarmonyPatch(typeof(Map), nameof(Map.MapUpdate))]
    public static class Patch_MapUpdate
    {
        static void Postfix(Map __instance)
        {
            if (!fixCorruptSectors) return;
            if (__instance.areaManager == null)
            {
                Log.Message("[Mid-saver Saver] detected corrupted area manager. Regenerating...");
                __instance.areaManager = new AreaManager(__instance);
                __instance.areaManager.AddStartingAreas();
            }
        }
    }
}