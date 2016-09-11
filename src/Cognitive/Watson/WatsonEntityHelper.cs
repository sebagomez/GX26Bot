using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GX26Bot.Cognitive.Watson
{
	public class WatsonEntityHelper
	{
		public enum Entity
		{
			CDS,
			Radisson,
			CoatCheck,
			FrontDesk,
			Room,
			Restroom,
			Snack,
			Break,
			TimeQ
		}

		public static Entity GetEntity(string entityText)
		{
			return (WatsonEntityHelper.Entity)Enum.Parse(typeof(WatsonEntityHelper.Entity), entityText);
		}
	}
}