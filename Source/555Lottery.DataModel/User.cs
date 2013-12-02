using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
{
	public class User
	{
		[Column(Order = 1)]
		public int UserId { get; set; }

		[Required]
		[Column(Order = 2)]
		public string SessionId { get; set; }

		[MaxLength(50)]
		[MinLength(5)]
		[StringLength(50, MinimumLength = 5)]
		[RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$")]
		[Column(Order = 3)]
		public string Email { get; set; }

		[Required]
		[Column(Order = 4)]
		public int NotificationFlags { get; set; }

		[Column(Order = 5)]
		public string Name { get; set; }

		[Column(Order = 6)]
		public string ReturnBitcoinAddress { get; set; }

		[InverseProperty("Owner")]
		public virtual ICollection<TicketLot> TicketLots { get; set; }

		public override string ToString()
		{
			return this.SessionId + " (" + this.Email + ")";
		}
	}
}