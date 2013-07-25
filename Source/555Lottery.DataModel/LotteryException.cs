using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class LotteryException : Exception
	{
		public LotteryException(string message)
			: base(message)
		{ }
	}
}