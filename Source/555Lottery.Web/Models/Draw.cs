using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
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

		[NotMapped]
		public decimal JackpotUSD { get; set; }

		[Column(Order = 5)]
		public decimal JackpotUSDAtDeadline { get; set; }

		[NotMapped]
		public decimal JackpotEUR { get; set; }

		[Column(Order = 6)]
		public decimal JackpotEURAtDeadline { get; set; }

		[Required]
		[Column(Order = 7)]
		public string BitCoinAddress { get; set; }

		[Column(Order = 8)]
		public decimal OneGamePrice { get; set; }

		[Column(Order = 9)]
		public string WinningTicketSequence { get; set; }

		[InverseProperty("Draw")]
		public ICollection<TicketLot> TicketLots { get; set; }
	}
}