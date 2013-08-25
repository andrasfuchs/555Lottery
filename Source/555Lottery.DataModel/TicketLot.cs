using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
{
	public class TicketLot
	{
		[Column(Order = 1)]
		public int TicketLotId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime CreatedUtc { get; set; }

		[Required]
		[Column(Order = 3)]
		[InverseProperty("TicketLots")]
		public Draw Draw { get; set; }

		[Required]
		[Column(Order = 4)]
		public string Code { get; set; }

		[Required]
		[Column(Order = 5)]
		public decimal TotalBTC { get; set; }

		/// <summary>
		/// The maximum mulitiple of 10'000 of the smallest BTC amount, 1 satoshi (0.000 000 01 BTC)
		/// </summary>
		[Required]
		[Column(Order = 6)]
		public decimal TotalDiscountBTC { get; set; }

		[Column(Order = 7)]
		public string RefundAddress { get; set; }

		[Required]
		[Column(Order = 8)]
		public TicketLotState State { get; set; }

		[Column(Order = 9)]
		public User Owner { get; set; }

		[Column(Order = 10)]
		public string SessionId { get; set; }

		[Column(Order = 11)]
		public string EmailAddress { get; set; }

		[Column(Order = 12)]
		public TransactionLog MostRecentTransactionLog { get; set; }

		[InverseProperty("TicketLot")]
		public virtual ICollection<Ticket> Tickets { get; set; }
	}
}