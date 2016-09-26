using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Cognitive.Watson;
using GX26Bot.GX26;
using GX26Bot.GX26.Data;
using GX26Bot.Helpers;
using GX26Bot.ServiceReference;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace GX26Bot.Cognitive.LUIS
{
	[Serializable]
	public class LUISManager : LuisDialog<object>
	{
		readonly string CONSECUTIVES_FAILS = "FAILS";
		readonly double MIN_ALLOWED_SCORE = 0.25d;

		static string s_model { get; } = ConfigurationManager.AppSettings["LuisModelId"];
		static string s_key { get; } = ConfigurationManager.AppSettings["LuisSubscriptionKey"];

		const string GREETING = "Greeting";
		const string SPEAKER_SESSION = "SpeakerSession";
		const string RESTROOM = "Restroom";
		const string LOCATION = "Location";
		const string HARASSMENT = "Harassment";
		const string DEEP = "Deep";
		const string FORTY_TWO = "42";
		const string PENAROL = "Peñarol";
		const string GRATITUDE = "Gratitude";
		const string NONE = "None";

		static LuisService s_service;
		static LUISManager()
		{
			LuisModelAttribute model = new LuisModelAttribute(ConfigurationManager.AppSettings["LuisModelId"], ConfigurationManager.AppSettings["LuisSubscriptionKey"]);
			s_service = new LuisService(model);
		}

		public LUISManager() : base(s_service) { }

		#region Greeting

		[LuisIntent(GREETING)]
		public async Task Greeting(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);

			string name = (await activity).From.Name.Trim();
			if (!string.IsNullOrEmpty(name))
				name = name.Split(' ')[0];
			
			await SendMessage(context, string.Format(lang.Hello, name), result.Query, GREETING);
			await SendMessage(context, lang.HelloMultiLine, result.Query, GREETING);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Speaker Session

		[LuisIntent(SPEAKER_SESSION)]
		public async Task SpeakerSession(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);

			string speaker = null;
			if (result.Entities != null && result.Entities.Count == 1)
			{
				speaker = result.Query.Substring(result.Entities[0].StartIndex.Value, (result.Entities[0].EndIndex.Value - result.Entities[0].StartIndex.Value) + 1);
				await SendSessionsMessageText(context, speaker, result.Query);
			}
			else
				await SendMessage(context, lang.NoSpeaker, result.Query, SPEAKER_SESSION);
			
			context.Wait(MessageReceived);
		}

		private async Task SendSessionsMessageText(IDialogContext context, string speaker, string original)
		{
			LanguageManager lang = await LanguageManager.GetLanguage(context, null);

			GX26Session sessions = FindSessions.Find(speaker, lang.Language);
			if (sessions.Sessions.Count() == 0)
			{
				await SendMessage(context, string.Format(lang.NoSpeakersSession, $"{speaker}"), original, SPEAKER_SESSION);
				return;
			}
			bool many = sessions.Count > 1;
			StringBuilder msg = new StringBuilder(many ? string.Format(lang.SpeakerSessionsFound, sessions.Count) : lang.SpeakerSessionsFound1);
			bool needsSpeaker = true;
			switch (sessions.Entityfound)
			{
				case "speaker":
					msg.AppendFormat(lang.SessionSpeaker, speaker);
					needsSpeaker = !speaker.Trim().Contains(" ");
					break;
				case "company":
					msg.AppendFormat(lang.SessionCompany, speaker);
					break;
				case "track":
					msg.AppendFormat(lang.SessionTrack, speaker);
					break;
				default:
					break;
			}
			await SendMessage(context, msg.ToString(), original, SPEAKER_SESSION);
			foreach (Session s in sessions.Sessions)
			{
				msg = new StringBuilder($@"- {s.Sessiontitle.Sanitize()} - {s.Sessiondaytext} {s.Sessiontimetxt} @ {s.Roomname}");
				if (needsSpeaker)
				{
					msg.Append(" (");

					foreach (Speaker sp in s.Speakers)
						msg.Append($"{sp.Speakerfirstname} {sp.Speakerlastname}, ");

					msg = msg.Remove(msg.Length - 2, 2);
					msg.Append(")");
				}

				await SendMessage(context, msg.ToString());
			}
		}

		#endregion

		[LuisIntent(RESTROOM)]
		public async Task Restroom(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);

			StorageLogger.LogData(new DataLog { Question = result.Query, Answer = lang.WhatFloor, Intent = RESTROOM });

			var floors = new[] { 2, 3, 4 };
			PromptDialog.Choice(context, RestroomFloorComplete, floors, string.Format(lang.WhatFloor, floors), string.Format(lang.InvalidFloor, floors), 3, PromptStyle.Auto);
		}

		private async Task RestroomFloorComplete(IDialogContext context, IAwaitable<int> result)
		{
				int floor = await result;
				LanguageManager lang = await LanguageManager.GetLanguage(context, null);
								
				await SendMessage(context, string.Format(lang.BathroomLocation, floor), ImageHelper.GetBathroomImage(floor));

			context.Wait(MessageReceived);
		}

		#region Location

		[LuisIntent(LOCATION)]
		public async Task Location(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);

			ConversationObject convObj = await Watson.Conversation.SendMessage(result.Query);

			if (convObj != null && convObj.entities.Length > 0)
			{
				ConversationEntity entity = convObj.entities[0];
				WatsonEntityHelper.Entity ent = WatsonEntityHelper.GetEntity(entity.entity);
				string message = "";// $"{entity.entity}:{entity.value}";
				string image = null;
				Break b;
				switch (ent)
				{
					case WatsonEntityHelper.Entity.CDS:
						break;
					case WatsonEntityHelper.Entity.Radisson:
						message = lang.Map;
						image = ImageHelper.GetLocationImage();
						break;
					case WatsonEntityHelper.Entity.CoatCheck:
						message = lang.CoatCheck;
						image = ImageHelper.GetCoatCheck();
						break;
					case WatsonEntityHelper.Entity.FrontDesk:
						message = lang.FrontDesk;
						break;
					case WatsonEntityHelper.Entity.Room:
						int floor;
						image = ImageHelper.GetRoomImage(entity.value, out floor);
						message = string.Format(lang.RoomMap, entity.value, floor);
						break;
					case WatsonEntityHelper.Entity.Break:
						b = NextBreak.Find();
						if (b != null)
							message = string.Format(lang.Break, b.Sessionstarttimetext);
						else
							message = lang.NoMoreBreaks;
						break;
					case WatsonEntityHelper.Entity.Snack:
						b = NextBreak.Find();
						if (b != null)
							message = string.Format(lang.Snack, b.Sessionstarttimetext, entity.value);
						else
							message = lang.NoMoreBreaks;
						break;
					case WatsonEntityHelper.Entity.Restroom:
						var floors = new[] { 2, 3, 4 };
						PromptDialog.Choice(context, RestroomFloorComplete, floors, string.Format(lang.WhatFloor, floors), string.Format(lang.InvalidFloor, floors), 3, PromptStyle.Auto);
						return;
					default:
						break;
				}
				
				await SendMessage(context, message, image, result.Query, LOCATION);

			}
			else
				await SendMessage(context, lang.NoLocation, result.Query, LOCATION);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Harassment

		[LuisIntent(HARASSMENT)]
		public async Task Harassment(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);

			await SendMessage(context, lang.Harassment, result.Query, HARASSMENT);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Harassment

		[LuisIntent(DEEP)]
		public async Task Deep(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);

			await SendMessage(context, lang.Deep, result.Query, DEEP);

			context.Wait(MessageReceived);
		}

		#endregion

		#region 42

		[LuisIntent(FORTY_TWO)]
		public async Task FortyTwo(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			await SendMessage(context, " ", ImageHelper.Get42(), result.Query, FORTY_TWO);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Peñarol

		[LuisIntent(PENAROL)]
		public async Task Penarol(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			await SendMessage(context, "Eso depende de qué cuadro seas simpatizante ;)", ImageHelper.GetPanarol(), result.Query, PENAROL);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Gratitude

		[LuisIntent(GRATITUDE)]
		public async Task Gratitude(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);

			await SendMessage(context, lang.YourWelcome, result.Query, GRATITUDE);

			context.Wait(MessageReceived);
		}

		#endregion

		#region None

		[LuisIntent("")]
		[LuisIntent(NONE)]
		public async Task None(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			LanguageManager lang = await LanguageManager.GetLanguage(context, activity);
			string message = "";
			string image = "";
			if (result.Entities.Any(e => e.Type == "Genexus"))
				message = lang.Genexus;
			else
			{
				ConversationObject convObj = await Watson.Conversation.SendMessage(result.Query);
				if (convObj != null && convObj.entities.Length > 0)
				{
					ConversationEntity convEnt = convObj.entities[0];
					WatsonEntityHelper.Entity ent = WatsonEntityHelper.GetEntity(convEnt.entity);
					Break b;
					switch (ent)
					{
						case WatsonEntityHelper.Entity.Radisson:
							message = lang.Map;
							image = ImageHelper.GetLocationImage();
							break;
						case WatsonEntityHelper.Entity.CoatCheck:
							message = lang.CoatCheck;
							image = ImageHelper.GetCoatCheck();
							break;
						case WatsonEntityHelper.Entity.FrontDesk:
							message = lang.FrontDesk;
							break;
						case WatsonEntityHelper.Entity.Room:
							int floor;
							image = ImageHelper.GetRoomImage(convEnt.value, out floor);
							message = string.Format(lang.RoomMap, convEnt.value, floor);
							break;
						case WatsonEntityHelper.Entity.Break:
							b = NextBreak.Find();
							if (b != null)
								message = string.Format(lang.Break, b.Sessionstarttimetext, convEnt.value);
							else
								message = lang.NoMoreBreaks;
							break;
						case WatsonEntityHelper.Entity.Snack:
							b = NextBreak.Find();
							if (b != null)
								message = string.Format(lang.Snack, b.Sessionstarttimetext, convEnt.value);
							else
								message = lang.NoMoreBreaks;
							break;
						case WatsonEntityHelper.Entity.TimeQ:
							break;
						default:
							break;
					}

				}
			}

			if (string.IsNullOrEmpty(message) && result.Query.Length > 2)
			{
				SearchServiceSoapClient client = new SearchServiceSoapClient();
				List<SourceParam> parm = new List<SourceParam>();
				parm.Add(new SourceParam() { sourceName = BotConfiguration.GXSEARCH_KEY });
				Search2Response response = await client.Search2Async($"{result.Query} webidioid:{lang.SearchCode}", parm.ToArray(), "SearchHighlight", 1, 10);
				if (response.Body.Search2Result.DocumentsCount > 0)
				{
					await SendMessage(context, string.Format(lang.SearchFound, response.Body.Search2Result.DocumentsCount, result.Query), result.Query, NONE);
					foreach (var doc in response.Body.Search2Result.Documents)
					{
						StringBuilder msg = new StringBuilder($"- {doc.Description.Sanitize()}");
						foreach (var p in doc.Properties)
							if (p.Key == "charlaexp")
								msg.Append($" ({p.Value.Sanitize()})");
						
						await SendMessage(context, msg.ToString(), result.Query, NONE);
					}
					context.Wait(MessageReceived);
					return;
				}
			}

			if (string.IsNullOrEmpty(message))
			{
				int fails = 1;
				if (context.UserData.TryGetValue<int>(CONSECUTIVES_FAILS, out fails))
				{
					fails++;
					if (fails > 6)
						message = lang.MyBad6;
					else if (fails > 4)
						message = lang.MyBad4;
					else if (fails > 2)
						message = lang.MyBad2;
					else
						message = lang.MyBad1;
				}
				else
					message = lang.MyBad1;

				context.UserData.SetValue<int>(CONSECUTIVES_FAILS, fails);
			}
			else
				OnSuccess(context);

			
			await SendMessage(context, message, image, result.Query, NONE);

			context.Wait(MessageReceived);
		}

		#endregion



		async Task SendMessage(IDialogContext context, string message, string image)
		{
			await SendMessage(context, message, image, null, null);
		}
		
		async Task SendMessage(IDialogContext context, string message)
		{
			await SendMessage(context, message, null, null);
		}

		async Task SendMessage(IDialogContext context, string message, string original, string intent)
		{
			await SendMessage(context, message, null, original, intent);
		}

		async Task SendMessage(IDialogContext context, string message, string image, string original, string intent)
		{
			if (string.IsNullOrEmpty(message))
				return;

			IMessageActivity msg = context.MakeMessage();
			msg.Text = message;
			if (!string.IsNullOrEmpty(image))
			{
				msg.Attachments = new List<Attachment>();
				msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = image });
			}

			try
			{
				if (!string.IsNullOrEmpty(intent))
					StorageLogger.LogData(new DataLog { Question = original, Answer = message, Intent = intent, User = context.MakeMessage().Recipient.Name });
			}
			catch { }

			await context.PostAsync(msg);
		}

		private bool IsScoreTooLow(IDialogContext context, LuisResult result)
		{

			IntentRecommendation intent = result.Intents[0];
			return intent.Score.HasValue && intent.Score.Value < MIN_ALLOWED_SCORE;
		}

		private void OnSuccess(IDialogContext context)
		{
			context.UserData.RemoveValue(CONSECUTIVES_FAILS);
		}

	}
}