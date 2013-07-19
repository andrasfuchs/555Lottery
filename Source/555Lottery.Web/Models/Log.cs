using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class Log
	{
		[Column(Order = 1)]
		public int LogId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime UtcTime { get; set; }

		[Required]
		[Column(Order = 3)]
		public string IPAddress { get; set; }
		
		[Required]
		[Column(Order = 4)]
		public string SessionId { get; set; }

		[Required]
		[Column(Order = 5)]
		public string Action { get; set; }

		[Column(Order = 6)]
		public string Parameters { get; set; }

		[Column(Order = 7)]
		public string FormattedMessage { get; set; }
	}
}