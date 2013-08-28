using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace _555Lottery.Web.ViewModels
{
	public class TicketViewModel
	{
		public int TicketId { get; set; }

		public DateTime CreatedUtc { get; set; }

		public int Index { get; set; }

		public TicketMode Mode { get; set; }

		public int Type { get; set; }
		
		public int[] Numbers { get; set; }

		public int[] Jokers { get; set; }

		public string Sequence 
		{
			get
			{
				return String.Join(",", this.Numbers) + "|" + String.Join(",", this.Jokers);
			}

			set 
			{
				string[] segments = value.Split('|');
				this.Numbers = segments[0].Replace(",", "") == "" ? new int[0] : segments[0].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();
				this.Jokers = segments[1] == "" ? new int[0] : segments[1].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();
			}
		}

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

		public decimal OneGameBTC { get; set; }
		
		public decimal Price
		{
			get
			{
				return NumberOfGames * this.OneGameBTC;
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

		public string HighestHits { get; set; }

		public decimal? Winnings { get; set; }

		public TicketViewModel() 
		{
			this.Numbers = new int[0];
			this.Jokers = new int[0];
		}

		private TicketViewModel(decimal oneGamePrice)
		{
			this.CreatedUtc = DateTime.UtcNow;

			this.OneGameBTC = oneGamePrice;

			this.Index = -1;
		}

		public TicketViewModel(decimal oneGamePrice, TicketMode mode, int type, int[] numbers, int[] jokers)
			: this(oneGamePrice)
		{
			this.Mode = mode;
			this.Type = type;
			
			this.Numbers = (numbers == null ? new int[0] : numbers);
			this.Jokers = (jokers == null ? new int[0] : jokers);
		}

		public TicketViewModel(decimal oneGamePrice, string ticketType, string ticketSequence)
			: this(oneGamePrice)
		{
			this.Mode = ticketType[0] == 'N' ? TicketMode.Normal : ticketType[0] == 'S' ? TicketMode.System : ticketType[0] == 'R' ? TicketMode.Random : TicketMode.Empty;
			this.Type = Int32.Parse(ticketType[1].ToString());

			this.Sequence = ticketSequence;
		}


		public override string ToString()
		{
			return this.Mode + "(" + this.Type + ") " + String.Join(",", this.Numbers) + "|" + String.Join(",", this.Jokers);
		}
	}
}