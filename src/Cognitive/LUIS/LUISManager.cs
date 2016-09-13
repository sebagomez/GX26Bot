using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using GX26Bot.Cognitive.Watson;
using GX26Bot.GX26;
using GX26Bot.GX26.Data;
using GX26Bot.Helpers;
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

		static LuisService s_service;
		static LUISManager()
		{
			LuisModelAttribute model = new LuisModelAttribute(ConfigurationManager.AppSettings["LuisModelId"], ConfigurationManager.AppSettings["LuisSubscriptionKey"]);
			s_service = new LuisService(model);
		}

		public LUISManager() : base(s_service) { }

		#region Greeting

		[LuisIntent("Greeting")]
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

			string message = string.Format(lang.Hello, (await activity).From.Name);
			await context.PostAsync(message);
			context.Wait(MessageReceived);
		}

		#endregion

		#region Speaker Session

		[LuisIntent("SpeakerSession")]
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
				speaker = result.Entities[0].Entity;

				List<Session> sessions = SpeakerCompanySessions.Find(speaker, lang.Language);
				if (sessions.Count == 0)
				{
					await context.PostAsync(string.Format(lang.NoSpeakersSession, $"{speaker}"));
					context.Wait(MessageReceived);
					return;
				}
				bool many = sessions.Count > 1;
				string msg = string.Format(lang.SpeakerSessionsFound, sessions.Count, $"{speaker}");
				foreach (Session s in sessions)
					msg += $@"
- {s.Sessiontitle} - {s.Sessiondaytext} {s.Sessiontimetxt}.{s.Roomname}  ";

				await context.PostAsync(msg);
				context.Wait(MessageReceived);
			}
			else
			{
				PromptDialog.Text(context, SpeakerComplete, lang.NoSpeaker, null, 1);
			}
		}

		private async Task SpeakerDisambiguated(IDialogContext context, string speaker)
		{
			LanguageManager lang = await LanguageManager.GetLanguage(context, null);

			List<Session> sessions = SpeakerCompanySessions.Find(speaker, lang.Language);
			if (sessions.Count == 0)
			{
				await context.PostAsync(string.Format(lang.NoSpeakersSession, $"{speaker}"));
				context.Wait(MessageReceived);
				return;
			}
			bool many = sessions.Count > 1;
			string msg = string.Format(lang.SpeakerSessionsFound, sessions.Count, $"{speaker}");
			foreach (Session s in sessions)
				msg += $@"
- {s.Sessiontitle} - {s.Sessiondaytext} {s.Sessiontimetxt}.{s.Roomname}  ";

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		private async Task OnSpeakerDisambiguated(IDialogContext context, IAwaitable<string> result)
		{
			string speaker = await result;
			await SpeakerDisambiguated(context, speaker);
		}

		private async Task SpeakerComplete(IDialogContext context, IAwaitable<string> result)
		{
			string speaker = await result;
			await SpeakerDisambiguated(context, speaker);
		}

		#endregion

		[LuisIntent("Restroom")]
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

			var floors = new[] { 2, 3, 4, 6, 25 };
			PromptDialog.Choice(context, RestroomFloorComplete, floors, string.Format(lang.WhatFloor, floors), string.Format(lang.InvalidFloor, floors), 3, PromptStyle.Auto);
		}

		private async Task RestroomFloorComplete(IDialogContext context, IAwaitable<int> result)
		{
			try
			{
				int floor = await result;
				LanguageManager lang = await LanguageManager.GetLanguage(context, null);

				IMessageActivity msg = context.MakeMessage();
				msg.Text = string.Format(lang.BathroomLocation, floor);
				msg.Attachments = new List<Attachment>();
				msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetBathroomImage(floor) });

				await context.PostAsync(msg);
			}
			catch (Exception) { }

			context.Wait(MessageReceived);
		}

		#region Location

		[LuisIntent("Location")]
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
				string message = $"{entity.entity}:{entity.value}";
				string image = null;
				Break b;
				switch (ent)
				{
					case WatsonEntityHelper.Entity.CDS:
						break;
					case WatsonEntityHelper.Entity.Radisson:
						break;
					case WatsonEntityHelper.Entity.CoatCheck:
						message = lang.CoatCheck;
						image = ImageHelper.GetCoatCheck();
						break;
					case WatsonEntityHelper.Entity.FrontDesk:
						image = ImageHelper.GetFrontDesk();
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
						var floors = new[] { 2, 3, 4, 6, 25 };
						PromptDialog.Choice(context, RestroomFloorComplete, floors, string.Format(lang.WhatFloor, floors), string.Format(lang.InvalidFloor, floors), 3, PromptStyle.Auto);
						return;
					default:
						break;
				}

				IMessageActivity msg = context.MakeMessage();
				msg.Text = message;
				if (!string.IsNullOrEmpty(image))
				{
					msg.Attachments = new List<Attachment>();
					msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = image });
				}
				await context.PostAsync(msg);
			}
			else
				await context.PostAsync("No entiendo qué estás buscando");

			context.Wait(MessageReceived);
		}

		#endregion

		#region Harassment

		[LuisIntent("Harassment")]
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

			await context.PostAsync(lang.Harassment);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Harassment

		[LuisIntent("Deep")]
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

			await context.PostAsync(lang.Deep);

			context.Wait(MessageReceived);
		}

		#endregion

		#region 42

		[LuisIntent("42")]
		public async Task FortyTwo(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = "";
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.Get42() });
			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Peñarol

		[LuisIntent("Peñarol")]
		public async Task Penarol(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, activity, result);
				return;
			}
			else
				OnSuccess(context);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = "Peñarol es el cuadro mas glorioso del Uruguay.";
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetPanarol() });
			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Gratitude

		[LuisIntent("Gratitude")]
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
			await context.PostAsync(lang.YourWelcome);

			context.Wait(MessageReceived);
		}

		#endregion

		#region None

		[LuisIntent("None")]
		[LuisIntent("")]
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
				if (convObj.entities.Length > 0)
				{
					ConversationEntity convEnt = convObj.entities[0];
					WatsonEntityHelper.Entity ent = WatsonEntityHelper.GetEntity(convEnt.entity);
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
							image = ImageHelper.GetFrontDesk();
							message = lang.FrontDesk;
							break;
						case WatsonEntityHelper.Entity.Room:
							int floor;
							image = ImageHelper.GetRoomImage(convEnt.value, out floor);
							message = string.Format(lang.RoomMap, convEnt.value, floor);
							break;
						case WatsonEntityHelper.Entity.Restroom:
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

			IMessageActivity msg = context.MakeMessage();
			msg.Text = message;
			if (!string.IsNullOrEmpty(image))
			{
				msg.Attachments = new List<Attachment>();
				msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = image });
			}
			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

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