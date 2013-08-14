using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _555Lottery.Service.TemplateModels
{
	public class EmailTemplateModelTEST
	{
		public Draw LastDraw { get; set; }
		public Ticket[] ValidTickets { get; set; }
	}
}
