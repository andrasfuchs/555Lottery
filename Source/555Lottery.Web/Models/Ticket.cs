using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class Ticket
	{
		private decimal oneGamePrice = 0.1M;

		public DateTime CreatedUtc { get; private set; }
		public int Index { get; set; }

		public TicketMode Mode { get; private set; }
		public int Type { get; private set; }
		public int[] Numbers { get; private set; }
		public int[] Jokers { get; private set; }

		public int NumberOfGames 
		{
			get
			{
				int result = (this.Mode == TicketMode.Empty ? 0 : 1);

				if (this.Mode == TicketMode.System)
				{
					switch (this.Type)
					{
						case 1:
							result = 6;
							break;
						case 2:
							result = 21;
							break;
						case 3:
							result = 56;
							break;
						case 4:
							result = 126;
							break;
						case 5:
							result = 252;
							break;
					}
				}

				result *= this.Jokers.Length;

				return result;
			}
		}

		public decimal Price
		{
			get
			{
				return NumberOfGames * oneGamePrice;
			}
		}

		public string Color
		{
			get
			{
				switch (Mode)
				{
					case TicketMode.Normal:
						return "blue";
					case TicketMode.System:
						return "orange";
					case TicketMode.Random:
						return "green";
				}

				return "blue";
			}
		}

		public Ticket(TicketMode mode, int type, int[] numbers, int[] jokers)
		{
			this.CreatedUtc = DateTime.UtcNow;

			this.Mode = mode;
			this.Type = type;
			this.Numbers = (numbers == null ? new int[0] : numbers);
			this.Jokers = (jokers == null ? new int[0] : jokers);
			this.Index = -1;
		}

		public override string ToString()
		{
			return this.Mode + "(" + this.Type + ") " + String.Join(",", this.Numbers) + "|" + String.Join(",", this.Jokers);
		}
	}
}