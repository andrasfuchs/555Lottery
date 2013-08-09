using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace _555Lottery.DataModel
{
	public class LotteryDbContext : DbContext
	{
		public DbSet<Draw> Draws { get; set; }
		public DbSet<TicketLot> TicketLots { get; set; }
		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<Game> Games { get; set; }

		public DbSet<User> Users { get; set; }
		public DbSet<ExchangeRate> ExchangeRates { get; set; }
		public DbSet<Log> Logs { get; set; }
		public DbSet<TransactionLog> TransactionLogs { get; set; }

		public LotteryDbContext() : this("Name=LotteryDbContext") { }

		public LotteryDbContext(string connectionStringId) : base(connectionStringId) { }

		protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
	   {
		   modelBuilder.Entity<Draw>().Property(d => d.JackpotBTC).HasPrecision(24, 8);
		   modelBuilder.Entity<TicketLot>().Property(tl => tl.TotalBTC).HasPrecision(24, 8);
		   modelBuilder.Entity<TicketLot>().Property(tl => tl.TotalBTCDiscount).HasPrecision(24, 8);

		   base.OnModelCreating(modelBuilder);
	   }
	}
}