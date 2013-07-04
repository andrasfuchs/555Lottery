using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class GameList : List<Game>
	{
		public string SessionId { get; private set; }

		public string TotalBtc { get; private set; }
	}
}