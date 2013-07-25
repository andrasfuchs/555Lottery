using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.DataModel
{
	public class User
	{
		[Column(Order = 1)]
		public int UserId { get; set; }

		[Required]
		[MaxLength(50)]
		[MinLength(5)]
		[StringLength(50, MinimumLength = 5)]
		[RegularExpression(@"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$")]
		[Column(Order = 2)]
		public string Email { get; set; }

		[Required]
		[Column(Order = 3)]
		public int NotificationFlags { get; set; }
	}
}