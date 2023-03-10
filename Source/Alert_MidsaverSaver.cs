using Verse;
using RimWorld;
using static MidsaverSaver.ModSettings_MidSaverSaver;
 
namespace MidsaverSaver
{
    public class Alert_MidSaverSaver : Alert_Critical
	{
		public Alert_MidSaverSaver()
		{
		}
		public override string GetLabel()
		{
			return "MidSaverSaver.Alert.MidSaverSaver.Label".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "MidSaverSaver.Alert.MidSaverSaver.Desc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (fixCorruptIdeos || fixCorruptSectors || fixCorruptWeather || fixCorruptWorldObjects || fixMissingStuff || fixMisc)
			{
				return new AlertReport  {active = true };
			}
			return new AlertReport();
		}
	}
}