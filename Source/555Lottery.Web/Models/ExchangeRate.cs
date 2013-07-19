using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class ExchangeRate
	{
		public int ExchangeRateId { get; set; }

		[Required]
		public string CurrencyISO1 { get; set; }
		[Required]
		public string CurrencyISO2 { get; set; }

		[Required]
		public DateTime TimeUtc { get; set; }
		[Required]
		public decimal Rate { get; set; }
	}
}