using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
{
	public class Draw
	{
		[Column(Order = 1)]
		public int DrawId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime DeadlineUtc { get; set; }

		[Required]
		[Column(Order = 3)]
		public string DrawCode { get; set; }

		[Required]
		[Column(Order = 4)]
		public decimal JackpotBTC { get; set; }

		[Required]
		[Column(Order = 5)]
		public string BitCoinAddress { get; set; }

		[Required]
		[Column(Order = 6)]
		public decimal OneGameBTC { get; set; }

		/// <summary>
		/// Semicolon separated BTC values (e.g. 0.12345678) indicating the expected winnings of each winner in each hit category (0, 1, 2, 3, 4 and 5 hits)
		/// </summary>
		[Required]
		[Column(Order = 7)]
		public string ExpectedWinningsBTC { get; set; }


		[Column(Order = 8)]
		public string WinningTicketSequence { get; set; }

		[Column(Order = 9)]
		public byte[] WinningTicketHash { get; set; }

		[Column(Order = 10)]
		public DateTime? WinningTicketGeneratedAt { get; set; }

		[Column(Order = 11)]
		public ExchangeRate ExchangeRateUSDAtDeadline { get; set; }

		[Column(Order = 12)]
		public ExchangeRate ExchangeRateEURAtDeadline { get; set; }


		[Column(Order = 13)]
		public int? ValidGameCount { get; set; }
		
		[Column(Order = 14)]
		public decimal? TotalIncomeBTC { get; set; }

		[Column(Order = 15)]
		public decimal? WinningsBTC { get; set; }

		/// <summary>
		/// Semicolon separated integers indicating the number of winners in each hit category (starting with 0p0, 0p1, 1p1 ... 5p1)
		/// </summary>
		[Column(Order = 16)]
		public string Hits { get; set; }

		
		[InverseProperty("Draw")]
		public virtual ICollection<TicketLot> TicketLots { get; set; }

		public override string ToString()
		{
			return this.DrawCode;
		}
	}
}