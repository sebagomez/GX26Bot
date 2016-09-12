using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web;

namespace GX26Bot.Helpers
{
	public class ImageHelper
	{
		static ResourceManager s_resmgr = new ResourceManager("GX26Bot.Resources.Images", typeof(ImageHelper).Assembly);

		public static string GetBathroomImage(int floor)
		{
			return s_resmgr.GetString($"Bathroom{floor}");
		}

		static Dictionary<string, int> s_roomFloor = new Dictionary<string, int>()
		{
			{ "Ballroom A", 2 },
			{ "Ballroom B", 2 },
			{ "Ballroom C", 2 },
			{ "Florida", 3 },
			{ "Picasso", 4 },
			{ "Renoir", 4 },
			{ "Conference Room", 4 },
			{ "Diplomat", 6 },
			{ "Torres García", 25 },

		};

		public static string GetRoomImage(string room, out int floor)
		{
			floor = s_roomFloor[room];
			return s_resmgr.GetString(room.Replace(" ",""));
		}

		public static string GetLocationImage()
		{
			return s_resmgr.GetString("GX26");
		}

		public static string GetCoatCheck()
		{
			return s_resmgr.GetString("CoatCheck");
		}

		public static string GetFrontDesk()
		{
			return s_resmgr.GetString("FrontDesk");
		}

		public static string Get42()
		{
			return s_resmgr.GetString("FortyTwo");
		}

		public static string GetPanarol()
		{
			return s_resmgr.GetString("Penarol");
		}
	}
}