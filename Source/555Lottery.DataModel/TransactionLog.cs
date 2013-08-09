using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
{
	public class TransactionLog
	{

		[Column(Order = 1)]
		public int TransactionLogId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime DownloadedUtc { get; set; }

		[Required]
		[Column(Order = 3)]
		public string InputAddress { get; set; }

		[Required]
		[Column(Order = 4)]
		public string OutputAddress { get; set; }

		[Required]
		[Column(Order = 5)]
		public string TransactionHash { get; set; }

		[Required]
		[Column(Order = 6)]
		public int BlockHeight { get; set; }

		[Required]
		[Column(Order = 7)]
		public DateTime BlockTimeStampUtc { get; set; }

		[Required]
		[Column(Order = 8)]
		public decimal TotalInput { get; set; }

		[Required]
		[Column(Order = 9)]
		public decimal TotalOutput { get; set; }

		[Required]
		[Column(Order = 10)]
		public int Confirmations { get; set; }
	}
}
