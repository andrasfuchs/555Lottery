using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class Game
	{
		public DateTime CreatedUtc { get; private set; }

		public GameType Type { get; private set; }
		public int Mode { get; private set; }
		public int[] Numbers { get; private set; }
		public int[] Jokers { get; private set; }

		public int NumberOfTickets { get; private set; }

		public Game(GameType type, int mode, int[] numbers, int[] jokers)
		{
			this.CreatedUtc = DateTime.UtcNow;

			this.Type = type;
			this.Mode = mode;
			this.Numbers = numbers;
			this.Jokers = jokers;

			this.NumberOfTickets = 1;

			if (type == GameType.System)
			{
				switch (mode)
				{
					case 1:
						this.NumberOfTickets = 6;
						break;
					case 2:
						this.NumberOfTickets = 21;
						break;
					case 3:
						this.NumberOfTickets = 56;
						break;
					case 4:
						this.NumberOfTickets = 126;
						break;
					case 5:
						this.NumberOfTickets = 252;
						break;
				}
			}

			this.NumberOfTickets *= jokers.Length;
		}
	}
}