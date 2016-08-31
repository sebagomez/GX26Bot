using System.Configuration;

namespace GX26Bot
{
	public class BotConfiguration
	{
		public static string GX26_SERVICES { get; } = ConfigurationManager.AppSettings["GX26ServicesLocation"];

	}
}