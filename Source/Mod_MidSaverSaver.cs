using Verse;
using UnityEngine;
using HarmonyLib;
using static MidSaverSaver.ModSettings_MidSaverSaver;
 
namespace MidSaverSaver
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
			options.CheckboxLabeled("MidSaverSaver.Settings.DisableCompression".Translate(), ref disableCompression, "MidSaverSaver.Settings.DisableCompression.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.FixCorruptWorldObjects".Translate(), ref fixCorruptWorldObjects, "MidSaverSaver.Settings.FixCorruptWorldObjects.Desc".Translate());
			options.CheckboxLabeled("MidSaverSaver.Settings.FixCorruptSectors".Translate(), ref fixCorruptSectors, "MidSaverSaver.Settings.FixCorruptSectors.Desc".Translate());
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
			base.ExposeData();
		}

		public static bool disableCompression, fixCorruptWorldObjects, fixCorruptSectors;
	}
}