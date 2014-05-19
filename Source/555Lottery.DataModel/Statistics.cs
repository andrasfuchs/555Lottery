using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace _555Lottery.DataModel
{
	public class Statistics
	{
		public string AffiliateCode;

		public int SessionCount { get; set; }
		public float SessionPercentage { get; set; }

		public int PageOpenedCount { get; set; }
		public float PageOpenedPercentage { get; set; }

		public int ClickLetsPlayCount { get; set; }
		public float ClickLetsPlayPercentage { get; set; }

		public int ClickNextCount { get; set; }
		public float ClickNextPercentage { get; set; }

		public int ClickPayCount { get; set; }
		public float ClickPayPercentage { get; set; }

		public int ValidPaymentCount { get; set; }
		public float ValidPaymentPercentage { get; set; }

		public override string ToString()
		{
			return AffiliateCode + " (" + SessionCount + ")";
		}
	}

}