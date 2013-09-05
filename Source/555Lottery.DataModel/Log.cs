using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
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

		[Column(Order = 5)]
		public LogLevel Level { get; set; }

		[Required]
		[Column(Order = 6)]
		public string Action { get; set; }

		[Column(Order = 7)]
		public string Parameters { get; set; }

		[Column(Order = 8)]
		public string FormattedMessage { get; set; }
	}
}