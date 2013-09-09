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

		[Column(Order = 5)]
		public ExchangeRate ExchangeRateUSDAtDeadline { get; set; }

		[Column(Order = 6)]
		public ExchangeRate ExchangeRateEURAtDeadline { get; set; }

		[Required]
		[Column(Order = 7)]
		public string BitCoinAddress { get; set; }

		[Required]
		[Column(Order = 8)]
		public decimal OneGameBTC { get; set; }

		/// <summary>
		/// Semicolon separated floats to indicate how much of the total income should go into which winning pool (starting with 0p0, 0p1, 1p1 ... 5p1)
		/// </summary>
		[Required]
		[Column(Order = 9)]
		public string PoolRatios { get; set; }

		[Column(Order = 10)]
		public int? ValidGameCount { get; set; }


		[Column(Order = 11)]
		public string WinningTicketSequence { get; set; }

		[Column(Order = 12)]
		public byte[] WinningTicketHash { get; set; }

		[Column(Order = 13)]
		public DateTime? WinningTicketGeneratedAt { get; set; }


		[Column(Order = 14)]
		public decimal? TotalIncomeBTC { get; set; }

		/// <summary>
		/// Semicolon separated decimals indicating the amount of BTC in each winning pool (starting with 0p0, 0p1, 1p1 ... 5p1)
		/// </summary>
		[Column(Order = 15)]
		public string AmountInPools { get; set; }

		[Column(Order = 26)]
		public decimal? PoolSecondChanceNextDrawBTC { get; set; }

		[Column(Order = 27)]
		public decimal? PoolSecondChanceMonthEndBTC { get; set; }

		[Column(Order = 28)]
		public decimal? PoolSecondChanceYearEndBTC { get; set; }


		[Column(Order = 29)]
		public decimal? WinningsBTC { get; set; }

		[Column(Order = 30)]
		public decimal? SecondChanceWinningsBTC { get; set; }

		/// <summary>
		/// Semicolon separated integers indicating the number of winners in each hit category (starting with 0p0, 0p1, 1p1 ... 5p1)
		/// </summary>
		[Column(Order = 31)]
		public string Hits { get; set; }

		
		[InverseProperty("Draw")]
		public virtual ICollection<TicketLot> TicketLots { get; set; }

		public override string ToString()
		{
			return this.DrawCode;
		}
	}
}