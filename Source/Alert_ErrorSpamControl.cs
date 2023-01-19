using Verse;
using RimWorld;
using static MidsaverSaver.ModSettings_MidSaverSaver;
 
namespace MidsaverSaver
{
    public class Alert_ErrorSpamControl : Alert_Critical
	{
		public Alert_ErrorSpamControl()
		{
		}
		public override string GetLabel()
		{
			return "MidSaverSaver.Alert.ErrorSpamControl.Label".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "MidSaverSaver.Alert.ErrorSpamControl.Desc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (disableErrorSpamControl)
			{
				return new AlertReport  {active = true };
			}
			return new AlertReport();
		}
	}
}