﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using _555Lottery.Web.Models;

namespace _555Lottery.Web.DataAccess
{
	public class LotteryDbContext : DbContext
	{
		public DbSet<Draw> Draws { get; set; }
		public DbSet<TicketLot> TicketSets { get; set; }
		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<ExchangeRate> ExchangeRates { get; set; }
		public DbSet<Log> Logs { get; set; }
		public DbSet<User> Users { get; set; }

		public LotteryDbContext() : this("Name=LotteryDbContext") { }

		public LotteryDbContext(string connectionStringId) : base(connectionStringId) { }
	}
}