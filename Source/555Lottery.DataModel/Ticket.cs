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

		[InverseProperty("Ticket")]
		public virtual ICollection<Game> Games { get; set; }
	}
}