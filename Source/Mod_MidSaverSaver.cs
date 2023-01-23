using Verse;
using UnityEngine;
using HarmonyLib;
using static MidsaverSaver.ModSettings_MidSaverSaver;
 
namespace MidsaverSaver
{
    public class Mod_MidSaverSaver : Mod
	{
		public Mod_MidSaverSaver(ModContentPack content) : base(content)
		{
			new Harmony(this.Content.PackageIdPlayerFacing).PatchAll();
			base.GetSettings<ModSettings_MidSaverSaver>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard options = new Listing_Standard();
			options.Begin(inRect);
			options.Label("MidSaverSaver.Settings.Label.Adding".Translate());
			options.GapLine();
			options.CheckboxLabeled("MidSaverSaver.Settings.DisableCompression".Translate(), ref disableCompression, "MidSaverSaver.Settings.DisableCompression.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.GenerateMissingMineables".Translate(), ref generateMissingMineables, "MidSaverSaver.Settings.GenerateMissingMineables.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.RemapBodyDefs".Translate(), ref remapBodyDefs, "MidSaverSaver.Settings.RemapBodyDefs.Desc".Translate());
			options.Gap(); //============================
			options.Label("MidSaverSaver.Settings.Label.Removing".Translate());
			options.GapLine();
			options.CheckboxLabeled("MidSaverSaver.Settings.FixCorruptWorldObjects".Translate(), ref fixCorruptWorldObjects, "MidSaverSaver.Settings.FixCorruptWorldObjects.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.FixCorruptSectors".Translate(), ref fixCorruptSectors, "MidSaverSaver.Settings.FixCorruptSectors.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.FixCorruptWeather".Translate(), ref fixCorruptWeather, "MidSaverSaver.Settings.FixCorruptWeather.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.FixCorruptIdeos".Translate(), ref fixCorruptIdeos, "MidSaverSaver.Settings.FixCorruptIdeos.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.FixMissingStuff".Translate(), ref fixMissingStuff, "MidSaverSaver.Settings.FixMissingStuff.Desc".Translate());
			if (Prefs.DevMode)
			{
				options.Gap(); //============================
				options.Label("MidSaverSaver.Settings.Label.Misc".Translate());
				options.GapLine();
				options.CheckboxLabeled("MidSaverSaver.Settings.DisableErrorSpamControl".Translate(), ref disableErrorSpamControl, "MidSaverSaver.Settings.DisableErrorSpamControl.Desc".Translate());
			}
			options.End();
			base.DoSettingsWindowContents(inRect);
		}
		public override string SettingsCategory()
		{
			return "Mid-saver Saver";
		}
		public override void WriteSettings()
		{
			base.WriteSettings();
		}
	}
	public class ModSettings_MidSaverSaver : ModSettings
	{
		public override void ExposeData()
		{
			Scribe_Values.Look<bool>(ref disableCompression, "disableCompression");
			Scribe_Values.Look<bool>(ref fixCorruptWorldObjects, "fixCorruptWorldObjects");
			Scribe_Values.Look<bool>(ref fixCorruptSectors, "fixCorruptSectors");
			Scribe_Values.Look<bool>(ref fixCorruptWeather, "fixCorruptWeather");
			Scribe_Values.Look<bool>(ref fixCorruptIdeos, "fixCorruptIdeos");
			Scribe_Values.Look<bool>(ref fixMissingStuff, "fixMissingStuff");
			Scribe_Values.Look<bool>(ref remapBodyDefs, "remapBodyDefs");
			base.ExposeData();
		}

		public static bool disableErrorSpamControl, 
			disableCompression, 
			fixCorruptWorldObjects, 
			fixCorruptSectors, 
			fixCorruptWeather,
			fixCorruptIdeos, 
			fixMissingStuff, 
			generateMissingMineables, 
			remapBodyDefs;
	}
}