using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class Ticket
	{
		private decimal oneGamePrice = 0.01M;

		[Column(Order = 1)]
		public int TicketId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime CreatedUtc { get; private set; }
		
		[NotMapped]
		public int Index { get; set; }

		[Required]
		[Column(Order = 3)]
		public TicketMode Mode { get; private set; }

		[Required]
		[Column(Order = 4)]
		public int Type { get; private set; }
		
		[NotMapped]
		public int[] Numbers { get; private set; }
		[NotMapped]
		public int[] Jokers { get; private set; }

		[Required]
		[Column(Order = 5)]
		public string Sequence { get; set; }

		[Required]
		[Column(Order = 6)]
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

		[Required]
		[Column(Order = 7)]
		public decimal Price
		{
			get
			{
				return NumberOfGames * oneGamePrice;
			}
		}

		[NotMapped]
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

		[Column(Order = 8)]
		public decimal? Winnings { get; set; }


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