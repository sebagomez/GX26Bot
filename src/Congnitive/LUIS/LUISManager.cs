using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Congnitive.Watson;
using GX26Bot.Dialogs;
using GX26Bot.Images;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace GX26Bot.Congnitive.LUIS
{
	[Serializable]
	public class LUISManager : LuisDialog<GX26Manager>
	{
		readonly string QUERY_LANGUAGE = "QUERY_LANGUAGE";
		readonly double MIN_ALLOWED_SCORE = 0.6d;

		static string s_model { get; } = ConfigurationManager.AppSettings["LuisModelId"];
		static string s_key { get; } = ConfigurationManager.AppSettings["LuisSubscriptionKey"];

		static LuisService s_service;
		static LUISManager()
		{
			LuisModelAttribute model = new LuisModelAttribute(ConfigurationManager.AppSettings["LuisModelId"], ConfigurationManager.AppSettings["LuisSubscriptionKey"]);
			s_service = new LuisService(model);
		}

		public LUISManager() :base(s_service) { }

		[LuisIntent("None")]
		[LuisIntent("")]
		public async Task None(IDialogContext context, LuisResult result)
		{
			string message = await LanguageHelper.GetNotUnderstoodText(result.Query);
			await context.PostAsync(message);
			context.Wait(MessageReceived);
		}

		[LuisIntent("SpeakerSession")]
		public async Task SpeakerSession(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}

			TextLanguage lang = await LanguageHelper.GetTextLanguage(result.Query);

			string speaker = null;
			if (result.Entities != null && result.Entities.Count > 0)
			{
				speaker = result.Entities[0].Entity;
				await context.PostAsync($"Asique quieres saber cuando habla {speaker}");
				context.Wait(MessageReceived);
			}
			else
			{
				PromptDialog.Text(context, SpeakerComplete, LanguageHelper.GetSpeakerQuestion(lang), null, 1);
			}
		}

		private async Task SpeakerComplete(IDialogContext context, IAwaitable<string> result)
		{
			string speaker = await result;
			IMessageActivity msg = context.MakeMessage();
			msg.Text = $"Te busco las charlas de {speaker}";

			await context.PostAsync(msg);
			context.Wait(MessageReceived);

		}

		[LuisIntent("Restroom")]
		public async Task Restroom(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}

			TextLanguage lang = await LanguageHelper.GetTextLanguage(result.Query);

			context.UserData.SetValue<TextLanguage>(QUERY_LANGUAGE, lang);

			var floors = new[] { 2, 3, 4, 6, 25 };
			PromptDialog.Choice(context, RestroomFloorComplete, floors, LanguageHelper.GetFloorQuestion(lang, floors), LanguageHelper.GetFloorQuestion(lang, floors, true), 3, PromptStyle.Auto);
		}

		private async Task RestroomFloorComplete(IDialogContext context, IAwaitable<int> result)
		{
			try
			{
				int floor = await result;

				TextLanguage lang = context.UserData.Get<TextLanguage>(QUERY_LANGUAGE);
				context.UserData.RemoveValue(QUERY_LANGUAGE);

				IMessageActivity msg = context.MakeMessage();
				msg.Text = LanguageHelper.GetRestroomMessage(lang, floor);
				msg.Attachments = new List<Attachment>();
				msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetBathroomImage(floor) });

				await context.PostAsync(msg);
			}
			catch (Exception) {  }

			context.Wait(MessageReceived);
		}

		[LuisIntent("Clothes")]
		public async Task Clothes(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}

			IMessageActivity msg = context.MakeMessage();
			msg.Text = await LanguageHelper.GetClothesMessage(result.Query);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetBathroomImage(2) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		[LuisIntent("Room")]
		public async Task Rooms(IDialogContext context, LuisResult result)
		{
			if (IsScoreTooLow(context, result))
			{
				await None(context, result);
				return;
			}


			TextLanguage lang = await LanguageHelper.GetTextLanguage(result.Query);

			if (result.Entities.Count == 0) {
				context.UserData.SetValue<TextLanguage>(QUERY_LANGUAGE, lang);
				PromptDialog.Text(context, RoomComplete, LanguageHelper.GetRoomQuestion(lang), null, 1);
				return;
			}
			string room = result.Entities[0].Entity;

			IMessageActivity msg = context.MakeMessage();
			msg.Text = await LanguageHelper.GetRoomMessage(result.Query, room);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetRoomImage(room) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		public async Task RoomComplete(IDialogContext context, IAwaitable<string> result)
		{
			string room = await result;

			TextLanguage lang = context.UserData.Get<TextLanguage>(QUERY_LANGUAGE);
			context.UserData.RemoveValue(QUERY_LANGUAGE);

			IMessageActivity msg = context.MakeMessage();
			msg.Text = await LanguageHelper.GetRoomMessage(lang, room);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetRoomImage(room) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		private bool IsScoreTooLow(IDialogContext context, LuisResult result)
		{
			IntentRecommendation intent = result.Intents[0];
			return intent.Score.HasValue && intent.Score.Value < MIN_ALLOWED_SCORE;
		}
	}
}