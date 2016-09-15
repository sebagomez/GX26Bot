namespace GX26Bot.GX26.Data
{
	public class GX26Session
	{
		public Session[] Sessions { get; set; }
		public int Count { get; set; }
		public bool Error { get; set; }
		public string Message { get; set; }
		public string Entityfound { get; set; }
	}

	public class Session
	{
		public Speaker[] Speakers { get; set; }
		public int Sessionid { get; set; }
		public string Sessiondate { get; set; }
		public int Sessionnum { get; set; }
		public string Roomname { get; set; }
		public string Sessiondaytext { get; set; }
		public string Sessiontimetxt { get; set; }
		public string Sessiontitle { get; set; }
		public string Sessionabstract { get; set; }
	}

}