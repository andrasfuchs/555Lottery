using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace _555Lottery.Web.Models
{
	public class Ticket
	{
		[Column(Order = 1)]
		public int TicketId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime CreatedUtc { get; private set; }

		[ScriptIgnore]
		[Required]
		[Column(Order = 3)]
		public TicketLot TicketLot { get; set; }

		[NotMapped]
		public int Index { get; set; }

		[Required]
		[Column(Order = 4)]
		public TicketMode Mode { get; private set; }

		[Required]
		[Column(Order = 5)]
		public int Type { get; private set; }
		
		[NotMapped]
		public int[] Numbers { get; private set; }
		[NotMapped]
		public int[] Jokers { get; private set; }

		[Required]
		[Column(Order = 6)]
		public string Sequence 
		{
			get
			{
				return String.Join(",", this.Numbers) + "|" + String.Join(",", this.Jokers);
			}

			set { }
		}

		[Required]
		[Column(Order = 7)]
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
		[Column(Order = 8)]
		public decimal Price
		{
			get
			{
				return NumberOfGames * this.TicketLot.Draw.OneGamePrice;
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

		[Column(Order = 9)]
		public decimal? Winnings { get; set; }


		public void Initialize(TicketLot tl, TicketMode mode, int type, int[] numbers, int[] jokers)
		{
			this.CreatedUtc = DateTime.UtcNow;

			this.Mode = mode;
			this.Type = type;
			this.Numbers = (numbers == null ? new int[0] : numbers);
			this.Jokers = (jokers == null ? new int[0] : jokers);
			this.Index = -1;

			this.TicketLot = tl;
		}

		public void Initialize(TicketLot tl, string ticketType, string ticketSequence)
		{
			string[] segments = ticketSequence.Split('|');
			int[] numbers = segments[0].Replace(",", "") == "" ? new int[0] : segments[0].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();
			int[] jokers = segments[1] == "" ? new int[0] : segments[1].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();

			this.Initialize(tl, ticketType[0] == 'N' ? TicketMode.Normal : ticketType[0] == 'S' ? TicketMode.System : ticketType[0] == 'R' ? TicketMode.Random : TicketMode.Empty, Int32.Parse(ticketType[1].ToString()), numbers, jokers);
		}


		public override string ToString()
		{
			return this.Mode + "(" + this.Type + ") " + String.Join(",", this.Numbers) + "|" + String.Join(",", this.Jokers);
		}
	}
}