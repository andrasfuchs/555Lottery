using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.ViewModels
{
	public class AffiliateViewModel
	{
		public string SessionId { get; set; }

		public int? UserId { get; set; }

		public string UserName { get; set; }

		public bool HaveAccess { get; set; }

		public string Codes { get; set; }

		public string StartDateTime { get; set; }
		
		public string EndDateTime { get; set; }

		public Statistics[] AffiliateCodeStatistics { get; set; }

		public Statistics GlobalStatistics { get; set; }
	}
}