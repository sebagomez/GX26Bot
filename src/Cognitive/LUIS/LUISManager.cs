﻿using System;
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

namespace GX26Bot.Cognitive.LUIS
{
	[Serializable]
	public class LUISManager : LuisDialog<GX26Manager>
	{
		readonly string QUERY_LANGUAGE = "QUERY_LANGUAGE";
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
			string lang = await DetectLanguage.Execute(result.Query);

			string message = HelpMessage.GetHelp(lang, "");// LanguageHelper.GetGreeting(lang);
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

			string lang = await DetectLanguage.Execute(result.Query);
			context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);

			string speaker = null;
			if (result.Entities != null && result.Entities.Count == 1)
			{
				speaker = result.Entities[0].Entity;
				List<Speaker> speakers = FindSpeaker.Find(speaker);
				if (speakers.Count == 0) //invalid speaker
				{
					await context.PostAsync(string.Format(LanguageHelper.GetNoSpeakersFound(lang), speaker));
					context.Wait(MessageReceived);
					return;
				}
				if (speakers.Count > 1) //must disambiguate
				{
					string msg = string.Format(LanguageHelper.GetManySpeakersFound(lang), speakers.Count, speaker);
					string[] listedSpeakers = speakers.Select<Speaker, string>(s => $"{s.Speakerfirstname} {s.Speakerlastname}").ToArray();
					PromptDialog.Choice(context, OnSpeakerDisambiguated, listedSpeakers, msg, null, 1, PromptStyle.Auto);
					return;
				}
				await SpeakerDisambiguated(context, $"{speakers[0].Speakerfirstname} {speakers[0].Speakerlastname}");
			}
			else
			{
				PromptDialog.Text(context, SpeakerComplete, LanguageHelper.GetSpeakerQuestion(lang), null, 1);
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
					await context.PostAsync(string.Format(LanguageHelper.GetNoSessionsFound(lang), $"{sp.Speakerfirstname} {sp.Speakerlastname}"));
					context.Wait(MessageReceived);
					return;
				}
				bool many = sessions.Count > 1;
				string msg = string.Format(LanguageHelper.GetSessionsFound(lang, sessions.Count > 1), sessions.Count, $"{sp.Speakerfirstname} {sp.Speakerlastname}");
				foreach (Session s in sessions)
					msg += $"{s.Sessiontitle} - {s.Sessiondaytext} {s.Sessiontimetxt}.{s.Roomname}{Environment.NewLine}";

				await context.PostAsync(msg);
			}
			else
				await context.PostAsync(string.Format(LanguageHelper.GetNoSpeakersFound(lang), speaker));

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

			string lang = await DetectLanguage.Execute(result.Query);
			context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);

			var floors = new[] { 2, 3, 4, 6, 25 };
			PromptDialog.Choice(context, RestroomFloorComplete, floors, LanguageHelper.GetFloorQuestion(lang, floors), LanguageHelper.GetFloorQuestion(lang, floors, true), 3, PromptStyle.Auto);
		}

		private async Task RestroomFloorComplete(IDialogContext context, IAwaitable<int> result)
		{
			try
			{
				int floor = await result;

				string lang = context.UserData.Get<string>(QUERY_LANGUAGE);
				context.UserData.RemoveValue(QUERY_LANGUAGE);

				IMessageActivity msg = context.MakeMessage();
				msg.Text = LanguageHelper.GetRestroomMessage(lang, floor);
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

			string lang = await DetectLanguage.Execute(result.Query);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = LanguageHelper.GetClothesMessage(lang);
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

			string lang = await DetectLanguage.Execute(result.Query);

			if (result.Entities.Count == 0)
			{
				context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);
				PromptDialog.Text(context, RoomComplete, LanguageHelper.GetRoomQuestion(lang), null, 1);
				return;
			}
			string room = result.Entities[0].Entity;

			IMessageActivity msg = context.MakeMessage();
			msg.Text = LanguageHelper.GetRoomMessage(lang, room);
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
			msg.Text = LanguageHelper.GetRoomMessage(lang, room);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetRoomImage(room) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		#endregion

		#region None

		[LuisIntent("None")]
		[LuisIntent("")]
		public async Task None(IDialogContext context, LuisResult result)
		{
			string lang = await DetectLanguage.Execute(result.Query);
			List<Watson.Entity> entities = await GetEntities.Execute(result.Query);

			string message = LanguageHelper.GetNotUnderstoodText(lang);
			if (entities.Count > 0)
			{
				message += "Pero creo lo que te interesa es: ";

				foreach (var e in entities)
					message += $"{e.text} ";
			}

			await context.PostAsync(message);
			context.Wait(MessageReceived);
		}

		#endregion

		private bool IsScoreTooLow(IDialogContext context, LuisResult result)
		{
			IntentRecommendation intent = result.Intents[0];
			return intent.Score.HasValue && intent.Score.Value < MIN_ALLOWED_SCORE;
		}

	}
}