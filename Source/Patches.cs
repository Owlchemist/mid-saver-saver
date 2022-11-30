using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
 
namespace MidSaverSaver
{
    [StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
        static HarmonyPatches()
        {
            new Harmony("owlchemist.midsaversaver").PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    public static class Patch_LoadGame
    {
        static void Postfix()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                thingDef.saveCompressible = false;
            }
        }
    }
}