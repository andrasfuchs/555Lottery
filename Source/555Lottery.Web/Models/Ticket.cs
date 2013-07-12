using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class Ticket
	{
		public DateTime CreatedUtc { get; private set; }
		public int Index { get; set; }

		public TicketType Type { get; private set; }
		public int Mode { get; private set; }
		public int[] Numbers { get; private set; }
		public int[] Jokers { get; private set; }

		public int NumberOfGames { get; private set; }
		public decimal Price
		{
			get
			{
				return NumberOfGames * 0.01M;
			}
		}

		public string Color
		{
			get
			{
				switch (Type)
				{
					case TicketType.Normal:
						return "blue";
					case TicketType.System:
						return "orange";
					case TicketType.Random:
						return "green";
				}

				return "blue";
			}
		}

		public Ticket(TicketType type, int mode, int[] numbers, int[] jokers)
		{
			this.CreatedUtc = DateTime.UtcNow;

			this.Type = type;
			this.Mode = mode;
			this.Numbers = numbers;
			this.Jokers = jokers;
			this.Index = -1;

			this.NumberOfGames = 1;

			if (type == TicketType.System)
			{
				switch (mode)
				{
					case 1:
						this.NumberOfGames = 6;
						break;
					case 2:
						this.NumberOfGames = 21;
						break;
					case 3:
						this.NumberOfGames = 56;
						break;
					case 4:
						this.NumberOfGames = 126;
						break;
					case 5:
						this.NumberOfGames = 252;
						break;
				}
			}

			this.NumberOfGames *= jokers.Length;
		}
	}
}