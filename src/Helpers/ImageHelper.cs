using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GX26Bot.Helpers
{
	public class ImageHelper
	{
		public static string GetBathroomImage(int floor)
		{
			switch (floor)
			{
				case 2:
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/2A_0415f25a69a941fabd11c916e97cafcc.png";
				case 3:
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/3F_9fdc574e25a64d679d0a54ab0286b6bb.png";
				case 4:
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/4CR_8626ddccb9044bf492e3cd8a9d96b011.png";
				default:
					return "https://fbcdn-sphotos-c-a.akamaihd.net/hphotos-ak-xtf1/t31.0-8/10273170_475451385933581_3709621524634832676_o.jpg";
			}
		}

		public static string GetRoomImage(string room)
		{
			int floor;
			return GetRoomImage(room, out floor);
		}

		public static string GetRoomImage(string room, out int floor)
		{
			floor = 0;
			switch (room.ToLower().Trim())
			{
				case "2a":
				case "ballroom a":
					floor = 2;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/2A_0415f25a69a941fabd11c916e97cafcc.png";
				case "2b":
				case "ballroom b":
					floor = 2;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/2B_2bfff5adb532470ca8cb973015928bab.png";
				case "2c":
				case "ballroom c":
					floor = 2;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/2C_f7f423df74af4d8fb4f30ed82abc3566.png";
				case "4cr":
				case "conference room":
					floor = 4;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/4CR_8626ddccb9044bf492e3cd8a9d96b011.png";
				case "3f":
				case "florida":
				case "florida room":
					floor = 3;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/3F_9fdc574e25a64d679d0a54ab0286b6bb.png";
				case "4r":
				case "renoir":
				case "renoir room":
					floor = 4;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/4R_a201f63aeef041cf9f1099dd50a08ea8.png";
				case "4p":
				case "picasso":
				case "picaso":
				case "picasso room":
					floor = 4;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/4P_b6740aa0609541b091552a03b34e695a.png";
				case "6d":
				case "diplomat":
				case "diplomat room":
					floor = 6;
					return "https://www5.genexus.com/GX26/PublicTempStorage/multimedia/6D_b3fb095a16c140bb81e298a000f5cbdb.png";
				default:
					return "https://fbcdn-sphotos-c-a.akamaihd.net/hphotos-ak-xtf1/t31.0-8/10273170_475451385933581_3709621524634832676_o.jpg";
			}
		}

		public static string GetLocationImage()
		{
			return "http://images.travelpod.com/cache/accom_maps/Radisson_Victoria_Plaza-Montevideo.gif";

		}
	}
}