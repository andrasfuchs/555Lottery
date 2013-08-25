using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.ViewModels
{
	public class DrawViewModel
	{
		public int DrawId { get; set; }

		public DateTime DeadlineUtc { get; set; }

		public string DrawCode { get; set; }

		public decimal JackpotBTC { get; set; }

		public decimal JackpotUSD { get; set; }

		public decimal JackpotUSDAtDeadline { get; set; }

		public decimal JackpotEUR { get; set; }

		public decimal JackpotEURAtDeadline { get; set; }

		public string BitCoinAddress { get; set; }

		public decimal OneGameBTC { get; set; }

		public string WinningTicketSequence { get; set; }

		public byte[] WinningTicketHash { get; set; }

		public DateTime? WinningTicketGeneratedAt { get; set; }
	}
}