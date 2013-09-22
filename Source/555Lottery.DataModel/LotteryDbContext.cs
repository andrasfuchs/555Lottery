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
			// NOTE: indexes must be created manually
			// ExchangeRates: [CurrencyISO1](ASC) [CurrencyISO2](ASC) [TimeUtc](DESC)


			modelBuilder.Entity<Draw>().Property(d => d.JackpotBTC).HasPrecision(24, 8);
			modelBuilder.Entity<Draw>().Property(d => d.OneGameBTC).HasPrecision(24, 8);
			modelBuilder.Entity<Draw>().Property(d => d.TotalIncomeBTC).HasPrecision(24, 8);
			modelBuilder.Entity<Draw>().Property(d => d.PoolSecondChanceMonthEndBTC).HasPrecision(24, 8);
			modelBuilder.Entity<Draw>().Property(d => d.PoolSecondChanceNextDrawBTC).HasPrecision(24, 8);
			modelBuilder.Entity<Draw>().Property(d => d.PoolSecondChanceYearEndBTC).HasPrecision(24, 8);
			modelBuilder.Entity<Draw>().Property(d => d.WinningsBTC).HasPrecision(24, 8);
			modelBuilder.Entity<Draw>().Property(d => d.SecondChanceWinningsBTC).HasPrecision(24, 8);

			modelBuilder.Entity<TicketLot>().Property(tl => tl.TotalBTC).HasPrecision(24, 8);
			modelBuilder.Entity<TicketLot>().Property(tl => tl.TotalDiscountBTC).HasPrecision(24, 8);
			modelBuilder.Entity<TicketLot>().Property(tl => tl.WinningsBTC).HasPrecision(24, 8);

			modelBuilder.Entity<Ticket>().Property(t => t.WinningsBTC).HasPrecision(24, 8);

			modelBuilder.Entity<Game>().Property(tl => tl.WinningsBTC).HasPrecision(24, 8);

			modelBuilder.Entity<TransactionLog>().Property(tl => tl.TotalInputBTC).HasPrecision(24, 8);
			modelBuilder.Entity<TransactionLog>().Property(tl => tl.OutputBTC).HasPrecision(24, 8);

			base.OnModelCreating(modelBuilder);
		}
	}
}