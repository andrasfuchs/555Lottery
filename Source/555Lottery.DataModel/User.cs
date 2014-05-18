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

		/// <summary>
		/// These are the codes which are associated with this user, so all the visitors using these codes will be counted to this user. Make sure that no code is used by more than one user!
		/// </summary>
		[Column(Order = 7)]
		public string OwnedAffiliateCodes { get; set; }

		/// <summary>
		/// If the user comes with a valid affiliate code, this field will be filled with it. This field should be filled only once
		/// </summary>
		[Column(Order = 8)]
		public string FirstAffiliateCode { get; set; }

		[InverseProperty("Owner")]
		public virtual ICollection<TicketLot> TicketLots { get; set; }

		public override string ToString()
		{
			return this.SessionId + " (" + this.Email + ")";
		}
	}
}