using System.Configuration;

namespace GX26Bot
{
	public class BotConfiguration
	{
		public static string GX26_SERVICES { get; } = ConfigurationManager.AppSettings["GX26ServicesLocation"];
		public static string ALCHEMY_API_KEY { get; } = ConfigurationManager.AppSettings["AlchemyApiKey"];
		public static string WATSON_API_KEY { get; } = ConfigurationManager.AppSettings["WatsonApiKey"];
		public static string CONVERSATION_USERNAME { get; } = ConfigurationManager.AppSettings["ConversationApiUser"];
		public static string CONVERSATION_PASSWORD { get; } = ConfigurationManager.AppSettings["ConversationApiPassword"];
		public static string CONVERSATION_WORKSPACE { get; } = ConfigurationManager.AppSettings["ConversationWorkspace"];
		public static string TEXTANALYTICS_KEY { get; } = ConfigurationManager.AppSettings["TextAnalytics"];

	}
}