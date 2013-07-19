using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class TicketSet
	{
		[Column(Order = 1)]
		public int TicketSetId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime CreatedUtc { get; set; }

		[Required]
		[Column(Order = 3)]
		public decimal TotalBTC { get; set; }

		[Required]
		[Column(Order = 4)]
		public string RefundAddress { get; set; }

		//[Required]
		//[Column(Order = 5)]
		//public TicketSetState State { get; set; }

		[Column(Order = 6)]
		public User Owner { get; set; }
	}
}