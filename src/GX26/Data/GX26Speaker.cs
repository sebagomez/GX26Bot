namespace GX26Bot.GX26.Data
{
	public class GX26Speaker
	{
		public Speaker[] Speakers { get; set; }
		public int Count { get; set; }
		public bool Error { get; set; }
		public string Message { get; set; }
	}

	public class Speaker
	{
		public int Speakerid { get; set; }
		public string Speakerfirstname { get; set; }
		public string Speakerlastname { get; set; }
		public string Photoimage { get; set; }
	}

}