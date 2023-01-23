using HarmonyLib;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
 
namespace MidsaverSaver
{
    public static class MidSaverSaverUtility
    {
        static Dialog_MessageBox reloadNow;
        public static void PromptToReload()
        {
            if (reloadNow == null) reloadNow = new Dialog_MessageBox(text: "MidSaverSaver.ReloadNow".Translate(), title: "MidSaverSaver.ReloadNow.Header".Translate() );
			if (!Find.WindowStack.IsOpen(reloadNow)) Find.WindowStack.Add(reloadNow);
        }
		public static void DisableCompression()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                thingDef.saveCompressible = false;
            }
        }
        public static void CheckWorldObjects()
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
                else if (item is Settlement settlement)
                {
                    bool isBad = false;
                    try
                    {
                        var materialTest = settlement.Material;
                        isBad = materialTest == null;
                    }
                    catch (System.Exception)
                    {
                        isBad = true;
                    }

                    if (isBad && item.factionInt == null)
                    {
                        count++;
                        item.Destroy();
                        worldObjects.Remove(item);
                    }
                }
            }
            if (count > 0)
            {
                Log.Message("[Mid-saver Saver] removed " + count.ToString() + " corrupt World Objects.");
                PromptToReload();
            }
            return;
        }
        public static void CheckIdeos()
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
                PromptToReload();
            }
        }
        public static void CheckAreas()
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
                PromptToReload();
            }
        } 
        public static void CheckWeather()
        {
            int count = 0;
            foreach (var map in Find.Maps)
            {
                if (map.weatherManager.curWeather == null || map.weatherManager.lastWeather == null)
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
                PromptToReload();
            }
        }
		public static void CheckSectors(MapDrawer __instance)
		{
            if (__instance.sections == null)
            {
                Log.Message("[Mid-saver Saver] detected missing map sections. Regenerating...");
                __instance.RegenerateEverythingNow();
                PromptToReload();
            }
		}
		public static void CheckNullStuff()
		{
			Dictionary<ThingDef, int> stuffLedger = new Dictionary<ThingDef, int>();
			int count = 0;

			foreach (var maps in Find.Maps)
			{
				var list = maps.listerThings.AllThings;
				var length = list.Count;
				for (int i = 0; i < length; i++)
				{
					var thing = list[i];
					if ((thing.def?.MadeFromStuff ?? false) && thing.stuffInt == null)
					{
						//Determine what the most common material is for other things of the same def
						var stuff = MostCommonStuff(thing.def);
						if (stuff != null)
						{
							thing.stuffInt = stuff;
						}
						//No other items in the world with valid stuff existed, so fallback to commonality on the first stuff category
						else
						{
							ThingDef replacementStuff;
							GenStuff.TryRandomStuffByCommonalityFor(thing.def, out replacementStuff);
							thing.stuffInt = replacementStuff;
						}
						count++;
						thing.graphicInt = null; //Reinit the graphic to set the color
					}
				}

				if (count > 0)
				{
					Log.Message("[Mid-saver Saver] detected " + count.ToString() + " things made a stuff material that no longer exists. Picking a new material...");
                    PromptToReload();
				}
			}

			ThingDef MostCommonStuff(ThingDef def)
			{
				//Use cache is available
				if (stuffLedger.ContainsKey(def)) return stuffLedger.MaxBy(x => x.Value).Key;

				foreach (var map in Find.Maps)
				{
					foreach (var sameThing in map.listerThings.ThingsOfDef(def))
					{
						if (sameThing.stuffInt == null) continue;

						if (stuffLedger.ContainsKey(sameThing.stuffInt)) stuffLedger[sameThing.stuffInt]++;
						else stuffLedger.Add(sameThing.stuffInt, 1);
					}
				}
				if (stuffLedger.Count > 0) return stuffLedger.MaxBy(x => x.Value).Key;
				return null;
			}
		}
        public static void CheckMissingMineables()
        {
            ThingDef[] blackList = new ThingDef[]
            { 
                ThingDefOf.CollapsedRocks,
                ThingDefOf.RaisedRocks,
                DefDatabase<ThingDef>.GetNamed("Marble", false),
                DefDatabase<ThingDef>.GetNamed("Limestone", false),
                DefDatabase<ThingDef>.GetNamed("Granite", false),
                DefDatabase<ThingDef>.GetNamed("Slate", false),
                DefDatabase<ThingDef>.GetNamed("Sandstone", false)
            };
            //Create list of mineables
            List<ThingDef> moddedMineables = new List<ThingDef>();
            var list = DefDatabase<ThingDef>.AllDefsListForReading;
            var length = list.Count;
            for (int i = 0; i < length; i++)
            {
                var def = list[i];
                if (def.mineable && !def.IsSmoothed && !blackList.Contains(def)) moddedMineables.Add(def);
            }
            
            //Abort if there's nothing to add
            if (moddedMineables.Count == 0) return;

            //Scan colony maps
            Patch_GenStep_ScatterLumpsMineable.overRideScatterActive = true;
            foreach (var map in Find.Maps)
            {
                if (!map.IsPlayerHome) continue;

                //Generate map name
                string mapName = map.Biome.label + " " + "MidSaverSaver.Map".Translate(); //Bi
                if (map.info.parent is Settlement settlement && settlement.namedByPlayer && !settlement.nameInt.NullOrEmpty()) mapName = settlement.nameInt; //Named map
                
                for (int i = moddedMineables.Count; i-- > 0;)
                {
                    var mineableDef = moddedMineables[i];
                    bool isLast = i + 1 == moddedMineables.Count;

                    if (map.listerThings.ThingsOfDef(mineableDef).Count != 0 ) continue;
                    var prompt = new Dialog_MessageBox(
                        text:          "MidSaverSaver.GenerateMineable".Translate(mineableDef.label, mapName), 
                        buttonAText:   "MidSaverSaver.GenerateMineable.Confirm".Translate(), 
                        buttonAAction: delegate {TryRegenNow(mineableDef, map, isLast); }, 
                        buttonBText:   "MidSaverSaver.GenerateMineable.Skip".Translate(), 
                        buttonBAction: delegate {TryRegenNow(isLast: isLast); }, 
                        title:         "MidSaverSaver.GenerateMineable.Header".Translate());
                    
                    if (!Find.WindowStack.IsOpen(prompt)) Find.WindowStack.Add(prompt);
                }
            }
            
            static void TryRegenNow(ThingDef mineableDef = null, Map map = null, bool isLast = false)
            {
                var dict = Patch_GenStep_ScatterLumpsMineable.mineableDefsQueue;
                if (mineableDef != null)
                {
                    //Initialize dictionary?
                    if (dict.NullOrEmpty()) dict = new Dictionary<Map, List<ThingDef>>();
                    //Map key already there?
                    if (!dict.ContainsKey(map)) dict.Add(map, new List<ThingDef>(){mineableDef});
                    else dict[map].Add(mineableDef);
                }

                if (isLast)
                {
                    if (dict?.Count > 0)
                    {
                        foreach (var entry in dict)
                        {
                            Patch_GenStep_ScatterLumpsMineable.mineableDefs = entry.Value;
                            float adjuster = 10f;
                            switch (Find.WorldGrid[entry.Key.Tile].hilliness)
                            {
                                case Hilliness.Flat:
                                    adjuster = 2f;
                                    break;
                                case Hilliness.SmallHills:
                                    adjuster = 3f;
                                    break;
                                case Hilliness.LargeHills:
                                    adjuster = 5f;
                                    break;
                                case Hilliness.Mountainous:
                                    adjuster = 6f;
                                    break;
                                case Hilliness.Impassable:
                                    adjuster = 7f;
                                    break;
                            }
                            var scatter = new GenStep_ScatterLumpsMineable()
                            {
                                maxValue = float.MaxValue,
                                countPer10kCellsRange = new FloatRange(adjuster, adjuster)
                            };

                            scatter.Generate(entry.Key, new GenStepParams());
                            FloodFillerFog.DebugRefogMap(entry.Key);
                        }
                    }
                    //Cleanup
                    Patch_GenStep_ScatterLumpsMineable.mineableDefs = null;
                    Patch_GenStep_ScatterLumpsMineable.mineableDefsQueue = null;
                    Patch_GenStep_ScatterLumpsMineable.overRideScatterActive = false;
                }
            }
        }
    }
}