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

		[Column(Order = 8)]
		public decimal OneGamePrice { get; set; }

		[Column(Order = 9)]
		public string WinningTicketSequence { get; set; }

		[Column(Order = 10)]
		public byte[] WinningTicketHash { get; set; }

		[Column(Order = 11)]
		public DateTime? WinningTicketGeneratedAt { get; set; }

		[Column(Order = 12)]
		public decimal NonJackpotWonBTC { get; set; }

		[Column(Order = 13)]
		public decimal SecondChanceWonBTC { get; set; }

		[InverseProperty("Draw")]
		public virtual ICollection<TicketLot> TicketLots { get; set; }
	}
}