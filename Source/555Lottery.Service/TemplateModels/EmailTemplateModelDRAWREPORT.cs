﻿using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _555Lottery.Service.TemplateModels
{
	public class EmailTemplateModelDRAWREPORT
	{
		public Draw Draw { get; set; }
		public TicketLot[] TicketLots { get; set; }
		public Ticket[] ValidTickets { get; set; }
		public Game[] ValidGames { get; set; }
	}
}
