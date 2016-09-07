using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using GX26Bot.Cognitive.TextAnalytics;
using GX26Bot.Cognitive.Watson;
using GX26Bot.Controllers;
using GX26Bot.GX26;
using GX26Bot.GX26.Data;
using GX26Bot.Helpers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using static GX26Bot.Cognitive.Watson.SentimentAnalysis;

namespace GX26Bot.Cognitive.LUIS
{
	[Serializable]
	public class LUISManager : LuisDialog<GX26Manager>
	{
		readonly string QUERY_LANGUAGE = "LANGUAGE";
		readonly string CONSECUTIVES_FAILS = "FAILS";
		readonly double MIN_ALLOWED_SCORE = 0.5d;

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
		public async Task Greeting(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);

			string message = LanguageHelper.GetMessage(LanguageHelper.Hello, lang);// HelpMessage.GetHelp(lang, "");// LanguageHelper.GetGreeting(lang);
			await context.PostAsync(message);
			context.Wait(MessageReceived);
		}

		#endregion

		#region Speaker Session

		[LuisIntent("SpeakerSession")]
		public async Task SpeakerSession(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);
			context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);

			string speaker = null;
			if (result.Entities != null && result.Entities.Count == 1)
			{
				speaker = result.Entities[0].Entity;
				List<Speaker> speakers = FindSpeaker.Find(speaker);
				if (speakers.Count == 0) //invalid speaker
				{
					await context.PostAsync(string.Format(LanguageHelper.GetMessage(LanguageHelper.NoSpeakersFound, lang), speaker));
					context.Wait(MessageReceived);
					return;
				}
				if (speakers.Count > 1) //must disambiguate
				{
					string msg = string.Format(LanguageHelper.GetMessage(LanguageHelper.TooManySpeakers, lang), speakers.Count, speaker);
					string[] listedSpeakers = speakers.Select<Speaker, string>(s => $"{s.Speakerfirstname} {s.Speakerlastname}").ToArray();
					PromptDialog.Choice(context, OnSpeakerDisambiguated, listedSpeakers, msg, null, 1, PromptStyle.Auto);
					return;
				}
				await SpeakerDisambiguated(context, $"{speakers[0].Speakerfirstname} {speakers[0].Speakerlastname}");
			}
			else
			{
				PromptDialog.Text(context, SpeakerComplete, LanguageHelper.GetMessage(LanguageHelper.NoSpeaker, lang), null, 1);
			}
		}

		private async Task SpeakerDisambiguated(IDialogContext context, string speaker)
		{
			string lang = context.UserData.Get<string>(QUERY_LANGUAGE);

			List<Speaker> speakers = FindSpeaker.Find(speaker);
			if (speakers.Count == 1) //best case
			{
				Speaker sp = speakers[0];
				List<Session> sessions = SpeakerSessions.Find(speakers[0].Speakerid, lang);
				if (sessions.Count == 0)
				{
					await context.PostAsync(string.Format(LanguageHelper.GetMessage(LanguageHelper.NoSpeakerSessions, lang), $"{sp.Speakerfirstname} {sp.Speakerlastname}"));
					context.Wait(MessageReceived);
					return;
				}
				bool many = sessions.Count > 1;
				string msg = string.Format(LanguageHelper.GetMessage(LanguageHelper.SpeakerSessionFound, lang), sessions.Count, $"{sp.Speakerfirstname} {sp.Speakerlastname}");
				foreach (Session s in sessions)
					msg += $@"
- {s.Sessiontitle} - {s.Sessiondaytext} {s.Sessiontimetxt}.{s.Roomname}  ";

				await context.PostAsync(msg);
			}
			else
				await context.PostAsync(string.Format(LanguageHelper.GetMessage(LanguageHelper.NoSpeakersFound,lang), speaker));

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

		#region Restroom

		[LuisIntent("Restroom")]
		public async Task Restroom(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);
			context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);

			var floors = new[] { 2, 3, 4, 6, 25 };
			PromptDialog.Choice(context, RestroomFloorComplete, floors, string.Format(LanguageHelper.GetMessage(LanguageHelper.WhatFloor, lang), floors), string.Format(LanguageHelper.GetMessage(LanguageHelper.WhatFloor2, lang), floors), 3, PromptStyle.Auto);
		}

		private async Task RestroomFloorComplete(IDialogContext context, IAwaitable<int> result)
		{
			try
			{
				int floor = await result;
				string lang = context.UserData.Get<string>(QUERY_LANGUAGE);

				IMessageActivity msg = context.MakeMessage();
				msg.Text = string.Format(LanguageHelper.GetMessage(LanguageHelper.BathroomLocation, lang), floor);
				msg.Attachments = new List<Attachment>();
				msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetBathroomImage(floor) });

				await context.PostAsync(msg);
			}
			catch (Exception) { }

			context.Wait(MessageReceived);
		}

		#endregion

		#region Clothes

		[LuisIntent("Clothes")]
		public async Task Clothes(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = LanguageHelper.GetMessage(LanguageHelper.CoatCheck, lang);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetBathroomImage(2) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Room

		[LuisIntent("Room")]
		public async Task Rooms(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);

			if (result.Entities.Count == 0)
			{
				context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);
				PromptDialog.Text(context, RoomComplete, LanguageHelper.GetMessage(LanguageHelper.MisunderstoodRoom, lang), null, 1);
				return;
			}
			string room = result.Entities[0].Entity;

			IMessageActivity msg = context.MakeMessage();
			msg.Text = string.Format(LanguageHelper.GetMessage(LanguageHelper.RoomMap, lang), room);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetRoomImage(room) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		public async Task RoomComplete(IDialogContext context, IAwaitable<string> result)
		{
			string room = await result;

			string lang = context.UserData.Get<string>(QUERY_LANGUAGE);
			context.UserData.RemoveValue(QUERY_LANGUAGE);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = string.Format(LanguageHelper.GetMessage(LanguageHelper.RoomMap, lang), room);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetRoomImage(room) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Location

		[LuisIntent("Location")]
		public async Task Location(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);
			context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);

			ConversationObject convObj = await Watson.Conversation.SendMessage(result.Query);

			if (convObj != null && convObj.entities.Length > 0)
			{
				foreach (var entity in convObj.entities)
				{
					LocationHelper.Entity ent = (LocationHelper.Entity) Enum.Parse(typeof(LocationHelper.Entity), entity.entity);
					string message = $"{entity.entity}:{entity.value}";
					switch (ent)
					{
						case LocationHelper.Entity.CDS:
							break;
						case LocationHelper.Entity.Radisson:
							break;
						case LocationHelper.Entity.CoatCheck:
							break;
						case LocationHelper.Entity.FrontDesk:
							break;
						case LocationHelper.Entity.Room:
							break;
						case LocationHelper.Entity.Restroom:
							var floors = new[] { 2, 3, 4, 6, 25 };
							PromptDialog.Choice(context, RestroomFloorComplete, floors, string.Format(LanguageHelper.GetMessage(LanguageHelper.WhatFloor, lang), floors), string.Format(LanguageHelper.GetMessage(LanguageHelper.WhatFloor2, lang), floors), 3, PromptStyle.Auto);
							return;
						default:
							break;
					}


					IMessageActivity msg = context.MakeMessage();
					msg.Text = message;// LanguageHelper.GetMessage(LanguageHelper.Map, lang);
					//msg.Attachments = new List<Attachment>();
					//msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetLocationImage() });

					await context.PostAsync(msg);
				}
			}
			else
				await context.PostAsync("No entiendo qué estás buscando");

			

			context.Wait(MessageReceived);
		}

		#endregion

		#region Genexus

		[LuisIntent("Genexus")]
		public async Task Genexus(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);

			await context.PostAsync(LanguageHelper.GetMessage(LanguageHelper.Genexus, lang));

			context.Wait(MessageReceived);
		}

		#endregion

		#region Harassment

		[LuisIntent("Harassment")]
		public async Task Harassment(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);

			await context.PostAsync(LanguageHelper.GetMessage(LanguageHelper.Harassment, lang));

			context.Wait(MessageReceived);
		}

		#endregion

		#region Harassment

		[LuisIntent("Deep")]
		public async Task Deep(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			string lang = await DetectLanguage.Execute(result.Query);

			await context.PostAsync(LanguageHelper.GetMessage(LanguageHelper.Deep, lang));

			context.Wait(MessageReceived);
		}

		#endregion

		#region 42

		[LuisIntent("42")]
		public async Task FortyTwo(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = "";
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = "https://upload.wikimedia.org/wikipedia/commons/5/56/Answer_to_Life.png" });
			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

		#region Peñarol

		[LuisIntent("Peñarol")]
		public async Task Penarol(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}
			else
				OnSuccess(context);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = "Peñarol es el cuadro mas glorioso del Uruguay.";
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c6/Escudo-penarol-2015.png" });
			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

		#region None

		[LuisIntent("None")]
		[LuisIntent("")]
		public async Task None(IDialogContext context, LuisResult result)
		{
			Entities entities = await GetEntities.Execute(result.Query);
			string lang = entities.language.ToLower();
			string message = "";
			int fails = 1;
			if (context.UserData.TryGetValue<int>(CONSECUTIVES_FAILS, out fails))
			{
				fails++;
				if (fails > 6)
					message = LanguageHelper.GetMessage(LanguageHelper.MyBad6, lang);
				else if (fails > 4)
					message = LanguageHelper.GetMessage(LanguageHelper.MyBad4, lang);
				else if (fails > 2)
					message = LanguageHelper.GetMessage(LanguageHelper.MyBad2, lang);
				else
					message = LanguageHelper.GetMessage(LanguageHelper.MyBad, lang);
			}
			else
				message = LanguageHelper.GetMessage(LanguageHelper.MyBad, lang);

			KeywordObject kw = await GetKeywords.Execute(result.Query);
			if (kw.keywords != null && kw.keywords.Length > 0)
			{
				message += @"
Detecté estas palabras claves:";
				foreach (var keyword in kw.keywords)
					message += $"{keyword.text}, ";
			}

			if (entities.entities.Count() > 0)
			{
				message += @"
Detecté estas entidades: ";
				foreach (var e in entities.entities)
					message += $"{e.text}  ";
			}

			Sentiment sentiment = await SentimentAnalysis.Execute(result.Query);
			if (sentiment == Sentiment.negative)
				message += LanguageHelper.GetMessage(LanguageHelper.Negative, lang);

			context.UserData.SetValue<int>(CONSECUTIVES_FAILS, fails);

			await context.PostAsync(message);
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