using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using static MidsaverSaver.ModSettings_MidSaverSaver;
 
namespace MidsaverSaver
{
    public class BodyDef_Mapper : GameComponent
	{
		List<string> bodyMap; //string is the defName, containing a dictionary of body part defNames and their original index number
		public BodyDef_Mapper(Game game) : base() 
		{
		}
		
		public override void FinalizeInit()
        {
			//Kill self if not using patch
			if (!remapBodyDefs) 
			{
				Current.Game.components?.Remove(this);
				return;
			}

			//Are we reading, or recording?
			if (bodyMap?.Count > 0) ValidateBodyDefs();
			else RecordBodyDefs();
            
        }

        public override void ExposeData()
        {
			if (!remapBodyDefs) return;
			Scribe_Collections.Look(ref bodyMap, "bodyMap", LookMode.Value);
        }

		void ValidateBodyDefs()
		{
			int count = 0;
			bool runRemap = false;
			var arrayMaps = new Dictionary<BodyDef, List<(int, int)>>(); //First number is what it is now, second number is the original index
			foreach (var bodyDef in DefDatabase<BodyDef>.AllDefs)
			{
				var originalData = new List<string>(GetBodyDef(bodyDef.defName));

				//Does the data exist, or was the data actually shrunk?
				if (originalData.NullOrEmpty()) continue;

				var arrayMap = new List<(int, int)>(); 
				
				var list = bodyDef.AllParts;
				for (int i = 0; i < list.Count; i++)
				{
					//Get the body part and generate its string
					var def = list[i];
					var compare = GenerateString(bodyDef, def);

					//Find what that string original was in the old array and create a cross reference array
					var originalIndex = originalData.FindIndex(x => x == compare);
					if (originalIndex != i && i < originalData.Count)
					{
						//Log.Message("[Mid-saver Saver] Remapping " + def.def.defName + " from " + i.ToString() + " to " + originalIndex.ToString() + " : " + compare);

						//What used to be at this index number? Try to find a match
						var tmp = originalData[i];
						for (int j = 0; j < list.Count; j++)
						{
							if (tmp == GenerateString(bodyDef, list[j])) originalIndex = j;
						}
					}
					arrayMap.Add((i, originalIndex));

					//Set flag if work needs to be done
					if (i != originalIndex) runRemap = true;
				}

				arrayMaps.Add(bodyDef, arrayMap);
			}
			if (runRemap) count += RemapData(arrayMaps);
			if (count > 0)
			{
				var prompt = new Dialog_MessageBox(text: "MidSaverSaver.BodyMapWritten".Translate(count.ToString()), title: "MidSaverSaver.BodyMap.Header".Translate());
				if (!Find.WindowStack.IsOpen(prompt)) Find.WindowStack.Add(prompt);
			}
			
			//We're done here, delete the component so it doesn't get saved
			Current.Game.components.Remove(this);

			//Turn off this fix
			remapBodyDefs = false;
			LoadedModManager.GetMod<Mod_MidSaverSaver>().WriteSettings();

			return;
		}

		int RemapData(Dictionary<BodyDef, List<(int, int)>> arrayMaps)
		{
			int count = 0;
			foreach (var pawn in Find.Maps.SelectMany(x => x.mapPawns.AllPawns).Concat(Find.WorldPawns.AllPawnsAliveOrDead))
			{
				if (pawn == null || pawn.health == null || pawn.health.hediffSet == null || pawn.health.hediffSet.hediffs == null) continue;

				foreach (var hediff in pawn.health.hediffSet.hediffs.ToList())
				{
					if (hediff.part == null) continue;
					
					int currentIndex = hediff.part.Index;
					if (arrayMaps.TryGetValue(hediff.part.body, out List<(int, int)> arrayMap))
					{
						var originalIndex = arrayMap.Find(x => x.Item1 == currentIndex).Item2;
						//Log.Message("[Mid-save Saver] for " + hediff.part.body.defName + " pawn " + (pawn.Name?.ToString() ?? "NULL") + " we are changing " + hediff.part.def.defName + 
								//" at index " + currentIndex.ToString() + " to index " + originalIndex.ToString());
						if (currentIndex != originalIndex && originalIndex > -1 && originalIndex < hediff.part.body.AllParts.Count)
						{
							count++;
							var replacementPart = hediff.part.body.AllParts[originalIndex];
							if (replacementPart != null) hediff.part = replacementPart;
						}
					}
				}
			}
			return count;
		}

		IEnumerable<string> GetBodyDef(string defName)
		{
			var length = bodyMap.Count;
			for (int i = 0; i < length; i++)
			{
				var tmp = bodyMap[i];
				if (tmp.Split('/')[0] == defName) yield return tmp;
			}
		}

		void RecordBodyDefs()
		{
			bodyMap = new List<string>();
			foreach (var bodyDef in DefDatabase<BodyDef>.AllDefs)
			{	
				var list = bodyDef.AllParts;
				for (int i = 0; i < list.Count; i++)
				{
					var def = list[i];
					if (def == null) continue;

					bodyMap.Add(GenerateString(bodyDef, def));
				}
			}

			if (bodyMap?.Count > 0) 
			{
				var prompt = new Dialog_MessageBox(text: "MidSaverSaver.BodyMapSaved".Translate(bodyMap.Count.ToString()), title: "MidSaverSaver.BodyMap.Header".Translate() );
				if (!Find.WindowStack.IsOpen(prompt)) Find.WindowStack.Add(prompt);
			}
		}

		string GenerateString(BodyDef bodyDef, BodyPartRecord def)
		{
			string parent = def.parent?.def?.defName ?? "NULL";
			return bodyDef.defName + "/" + parent + "/" + (def.customLabel != null ? "#"+def.customLabel : (def.def?.defName ?? "NULL"));
		}
	}
}