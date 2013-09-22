using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
{
	public class ExchangeRate
	{
		public int ExchangeRateId { get; set; }

		[Column(TypeName = "char")]
		[MaxLength(3)]
		[Required]
		public string CurrencyISO1 { get; set; }

		[Column(TypeName = "char")]
		[MaxLength(3)]
		[Required]
		public string CurrencyISO2 { get; set; }

		[Required]
		public DateTime TimeUtc { get; set; }

		[Required]
		public decimal Rate { get; set; }
	}
}