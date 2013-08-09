using System;
using System.Collections.Generic;

namespace _555Lottery.Web.Models
{
	public class LotteryException : Exception
	{
		public LotteryException(string message)
			: base(message)
		{ }
	}
}