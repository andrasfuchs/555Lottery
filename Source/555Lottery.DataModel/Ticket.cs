using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
{
	public class Ticket
	{
		[Column(Order = 1)]
		public int TicketId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime CreatedUtc { get; set; }

		[InverseProperty("Tickets")]
		[Required]
		[Column(Order = 3)]
		public TicketLot TicketLot { get; set; }

		[Required]
		[Column(Order = 4)]
		public TicketMode Mode { get; set; }

		[Required]
		[Column(Order = 5)]
		public int Type { get; set; }

		[Required]
		[Column(Order = 6)]
		public string Sequence { get; set; }

		[Required]
		[Column(Order = 7)]
		public byte[] SequenceHash { get; set; }

		[Required]
		[Column(Order = 8)]
		public int Index { get; set; }

		[Column(Order = 9)]
		public decimal? WinningsBTC { get; set; }

		[InverseProperty("Ticket")]
		public virtual ICollection<Game> Games { get; set; }

		public override string ToString()
		{
			return this.Sequence;
		}
	}
}